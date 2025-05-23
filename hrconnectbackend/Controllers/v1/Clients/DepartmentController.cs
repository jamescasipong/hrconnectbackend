﻿using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Authorize]
    [Route("api/v{version:apiVersion}/department")]
    [ApiVersion("1.0")]
    public class DepartmentController(
        IDepartmentServices departmentServices,
        IEmployeeServices employeeServices,
        IMapper mapper,
        ILogger<DepartmentController> logger)
        : ControllerBase
    {
        [Authorize]
        [UserRole("Employee")]
        [HttpGet("my-department")]
        public async Task<IActionResult> GetDepartment(){
            try
            {
                var currentUserId = User.FindFirstValue("EmployeeId");

                if (currentUserId == null){
                    return Unauthorized(new ApiResponse(false, $"User not authenticated."));
                }

                var employee = await employeeServices.GetByIdAsync(int.Parse(currentUserId));


                if (employee == null){
                    return NotFound(new ApiResponse(false, $"Employee not found."));
                }

                if (employee.EmployeeDepartmentId == null){
                    return Ok(new ApiResponse(false, $"Employee not assigned to any department."));
                }

                var department = await departmentServices.GetEmployeeDepartment(employee.EmployeeDepartmentId.Value);

                if (department == null){
                    return NotFound(new ApiResponse(false, $"Department not found."));
                }

                var mappedDepartment = mapper.Map<ReadEmployeeDepartmentDTO>(department);

                return Ok(new ApiResponse<ReadEmployeeDepartmentDTO?>(true, $"Department retrieved successfully!", mappedDepartment));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [HttpGet("get-department/guid/{id}")]
        public async Task<IActionResult> GetDepartmentByGuid(Guid id)
        {
            var deparment = await departmentServices.GetDepartmentByGuid(id);
            
            return Ok(deparment);
        }

        // [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto departmentDto)
        {
            var orgId = User.FindFirstValue("organizationId")!;

            if (!int.TryParse(orgId, out var orgIdParse))
            {
                return Ok("Organization id is not a valid integer.");
            }

            try
            {
                var department = new Department
                {
                    DeptName = departmentDto.DeptName,
                    Description = departmentDto.Description,
                    OrganizationId = orgIdParse
                };

                if (!ModelState.IsValid) return BadRequest(new ApiResponse(false, "Invalid Request"));

                await departmentServices.AddAsync(department);

                return Ok(new ApiResponse<ReadDepartmentDto?>(true, $"Department created successfully!",
                    mapper.Map<ReadDepartmentDto>(department)));
            }
            catch (DbUpdateException)
            {
                return BadRequest(new ApiResponse(false, "Department name should be unique"));
            }
            catch (Exception)
            {
                return StatusCode(500,"Internal Server Error");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> RetrieveDepartments(int? pageIndex, int? pageSize)
        {
            if (!int.TryParse(User.FindFirstValue("organizationId"), out var orgId))
            {
                return BadRequest(new ApiResponse(false, "Organization id is not a valid integer."));
            }

            try
            {
                var departments = await departmentServices.RetrieveDepartment(orgId);

                return Ok(new ApiResponse<dynamic>(true, $"Departments retrieved successfully", departments));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }
        [Authorize]
        [Authorize(Roles = "Admin")]
        [HttpGet("{departmentId:int}")]
        public async Task<IActionResult> RetrieveDepartment(int departmentId)
        {
            try
            {
                var currentUserId = User.FindFirstValue("EmployeeId");
                var userRoles = User.FindAll(ClaimTypes.Role).Select(a => a.Value).ToList();

                if (currentUserId == null){
                    return Unauthorized(new ApiResponse(false, $"User not authenticated."));
                }

                bool isAdmin = userRoles.Contains("Admin");

                if (!isAdmin){
                    return Forbid();
                }

                var department = await departmentServices.GetByIdAsync(departmentId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Department with id: {departmentId} not found."));
                }

                return Ok(new ApiResponse<Department>(true, $"Deparment with id: {departmentId} retrieved successfully!"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("department-name")]
        public async Task<IActionResult> GetDepartmentByName(string name)
        {
            try
            {
                var departments = await departmentServices.GetAllAsync();

                var departmentByName = departments.Where(d => d.DeptName.Contains(name)).ToList();

                var mappedDepartment = mapper.Map<List<ReadDepartmentDto>>(departmentByName);

                if (!departmentByName.Any())
                {
                    return Ok(new ApiResponse<List<ReadDepartmentDto>?>(true, $"Department containing {name} not found.", mappedDepartment));
                }

                return Ok(new ApiResponse<List<ReadDepartmentDto>?>(true, $"Department containing {name} retrieved successfully!", mappedDepartment));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{departmentId:int}")]
        public async Task<IActionResult> UpdateDepartment(int departmentId, CreateDepartmentDto departmentDto)
        {
            try
            {
                var department = await departmentServices.GetByIdAsync(departmentId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Department with id: {departmentId} not found."));
                }

                await departmentServices.UpdateAsync(mapper.Map<Department>(departmentDto));

                return Ok(new ApiResponse(true, $"Department with id: {departmentId} updated successfully!"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{departmentId:int}")]
        public async Task<IActionResult> DeleteDepartment(int departmentId)
        {
            try
            {
                var department = await departmentServices.GetByIdAsync(departmentId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Department with id: {departmentId} not found."));
                }

                await departmentServices.DeleteAsync(department);

                return Ok(new ApiResponse(true, $"Department with id: {departmentId} deleted successfully!"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet("employee-department")]
        public async Task<IActionResult> RetrieveEmpDepartments(int? pageIndex, int? pageSize)
        {
            var orgId = User.FindFirstValue("organizationId");

            if (!int.TryParse(orgId, out var orgIdParse))
            {
                return BadRequest(new ApiResponse(false, "Organization id is not a valid integer."));
            }

            try
            {
                var employeeDepartments = await departmentServices.RetrieveEmployeeDepartments(orgIdParse, pageIndex, pageSize);


                var employeeDepartmentDTO = mapper.Map<List<ReadEmployeeDepartmentDTO>>(employeeDepartments);


                return Ok(new ApiResponse<List<ReadEmployeeDepartmentDTO>>(true, $"Employee departments retrieved successfully", employeeDepartmentDTO));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrieving employee departments");
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{departmentId:int}/create-emp-department")]
        public async Task<IActionResult> CreateEmployeeDepartment(int departmentId, int supervisorId)
        {
            var organization = User.FindFirstValue("organizationId");

            if (!int.TryParse(organization, out var orgId))
            {
                return BadRequest(new ApiResponse(false, "Organization id is not a valid integer."));
            }

            try
            {
                await departmentServices.CreateEmployeeDepartment(departmentId, supervisorId, orgId);

                return Ok(new ApiResponse(true, $"Employee department created successfully!"));
            }
            catch (Exception ex)
            {
                if (ex is CustomException)
                {
                    return BadRequest(new ApiResponse(false, ex.Message));
                }
                else
                {
                    return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
                }
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{employeeDeptId:int}/supervisor")]
        public async Task<IActionResult> UpdateDeptSupervisor(int employeeDeptId, int supervisorId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse(false, ModelState.ToString()));
                }

                var department = await departmentServices.GetEmployeeDepartment(employeeDeptId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Unable to process. Department with {employeeDeptId} not found."));
                }

                var employee = await employeeServices.GetByIdAsync(supervisorId);

                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Unable to process. Employee with id: {supervisorId} not found."));
                }
                
                var employeeDepartment = await departmentServices.UpdateEmployeeDepartmentSupervisor(employee.Id, department.Id);
                
                if (employeeDepartment == null) return NotFound();

                return Ok(new ApiResponse(true, $"Supervisor was added or updated into a department: {supervisorId} successfully!"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{supervisorDepartmentId:int}/employee")]
        public async Task<IActionResult> AddORUpdateEmployeeDepartment(int supervisorDepartmentId, int employeeId)
        {
            try
            {
                var department = await departmentServices.GetEmployeeDepartment(supervisorDepartmentId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Department with id: {supervisorDepartmentId} not found."));
                }
                
                var employee = await employeeServices.GetByIdAsync(employeeId);

                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Unable to process. Employee with id: {employeeId} not found."));
                }

                employee.EmployeeDepartmentId = supervisorDepartmentId;
                await employeeServices.UpdateAsync(employee);

                return Ok(new ApiResponse(true, $"Employee with id: {employeeId} was added or deleted"));
            }
            catch(Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }


    }
}
