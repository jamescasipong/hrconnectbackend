using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Authorize]
    [Route("api/v{version:apiVersion}/department")]
    [ApiVersion("1.0")]
    public class DepartmentController(
        IDepartmentServices departmentServices,
        IEmployeeServices employeeServices,
        IMapper mapper)
        : ControllerBase
    {
        [Authorize]
        [UserRole("Employee")]
        [HttpGet("my-department")]
        public async Task<IActionResult> GetDepartment(){
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (currentUserId == null){
                    return Unauthorized(new ApiResponse(false, $"User not authenticated."));
                }

                var employee = await employeeServices.GetByIdAsync(int.Parse(currentUserId));


                if (employee == null){
                    return NotFound(new ApiResponse(false, $"Employee not found."));
                }

                if (employee.EmployeeDepartmentId == null){
                    return Ok(new ApiResponse<dynamic>(false, $"Employee not assigned to any department.", null));
                }

                var department = await departmentServices.GetByIdAsync(employee.EmployeeDepartmentId.Value);

                if (department == null){
                    return NotFound(new ApiResponse<ReadDepartmentDto>(false, $"Department not found.", null));
                }

                var mappedDepartment = mapper.Map<ReadDepartmentDto>(department);

                return Ok(new ApiResponse<ReadDepartmentDto>(true, $"Department retrieved successfully!", mappedDepartment));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        // [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto departmentDTO)
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
                    DeptName = departmentDTO.DeptName,
                    Description = departmentDTO.Description,
                    TenantId = orgIdParse
                };

                if (!ModelState.IsValid) return BadRequest(new ApiResponse(false, ModelState.ToJson().ToString()));

                await departmentServices.AddAsync(department);

                return Ok(new ApiResponse<ReadDepartmentDto>(true, $"Department created successfully!",
                    mapper.Map<ReadDepartmentDto>(department)));
            }
            catch (DbUpdateException ex)
            {
                return BadRequest(new ApiResponse(false, "Department name should be unique"));
            }
            catch (Exception ex)
            {
                return StatusCode(500,"Internal Server Error");
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> RetrieveDepartments(int? pageIndex, int? pageSize)
        {
            try
            {
                var departments = await departmentServices.RetrieveDepartment(pageIndex, pageSize);

                return Ok(new ApiResponse<List<EmployeeDepartment>>(true, $"Departments retrieved successfully", departments));
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
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var userRoles = User.FindAll(ClaimTypes.Role).Select(a => a.Value).ToList();

                if (currentUserId == null){
                    return Unauthorized(new ApiResponse(false, $"User not authenticated."));
                }

                bool isAdmin = userRoles.Contains("Admin");

                if (!isAdmin && currentUserId == null){
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
                    return Ok(new ApiResponse<List<ReadDepartmentDto>>(true, $"Department containing {name} not found.", mappedDepartment));
                }

                return Ok(new ApiResponse<List<ReadDepartmentDto>>(true, $"Department containing {name} retrieved sucessfully!", mappedDepartment));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{departmentId:int}")]
        public async Task<IActionResult> UpdateDepartment(int departmentId, CreateDepartmentDto departmentDTO)
        {
            try
            {
                var department = await departmentServices.GetByIdAsync(departmentId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Department with id: {departmentId} not found."));
                }

                await departmentServices.UpdateAsync(mapper.Map<Department>(departmentDTO));

                return Ok(new ApiResponse(true, $"Department with id: {departmentId} updated successfully!"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Erro"));
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

        [Authorize(Roles = "Admin")]
        [HttpPut("{departmentId:int}/supervisor")]
        public async Task<IActionResult> UpdateSupervisor(int departmentId, int employeeId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse(false, ModelState.ToString()));
                }

                var department = await departmentServices.GetEmployeeDepartment(departmentId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Unable to process. Department with {departmentId} not found."));
                }

                var employee = await employeeServices.GetByIdAsync(employeeId);

                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Unable to process. Employee with id: {employeeId} not found."));
                }
                
                var employeeDepartment = await departmentServices.UpdateEmployeeDepartmentSupervisor(employee.Id, department.Id);
                
                if (employeeDepartment == null) return NotFound();

                return Ok(new ApiResponse(true, $"Supervisor was added or updated into a department: {departmentId} successfully!"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{departmentId:int}/employee")]
        public async Task<IActionResult> UpdateEmployeeDepartment(int departmentId, int employeeId)
        {
            try
            {
                var department = await departmentServices.GetByIdAsync(departmentId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Department with id: {departmentId} not found."));
                }

                var employee = await employeeServices.GetByIdAsync(employeeId);

                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Unable to process. Employee with id: {employeeId} not found."));
                }

                employee.EmployeeDepartmentId = departmentId;
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
