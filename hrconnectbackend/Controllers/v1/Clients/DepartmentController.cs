using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Constants;
using hrconnectbackend.Exceptions;
using hrconnectbackend.Extensions;
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
        IMapper mapper)
        : ControllerBase
    {
        [Authorize]
        [UserRole("Employee")]
        [HttpGet("my-department")]
        public async Task<IActionResult> GetDepartment()
        {
            var currentUserId = User.RetrieveSpecificUser("EmployeeId");

            var employee = await employeeServices.GetByIdAsync(int.Parse(currentUserId));

            if (employee.EmployeeDepartmentId == null)
            {
                throw new ConflictException(ErrorCodes.EmployeeDepartmentNotFound, $"Employee department not found for employee with id: {currentUserId}.");
            }

            var department = await departmentServices.GetEmployeeDepartment(employee.EmployeeDepartmentId.Value);

            var mappedDepartment = mapper.Map<ReadEmployeeDepartmentDTO>(department);

            return Ok(new SuccessResponse<ReadEmployeeDepartmentDTO>(mappedDepartment, $"Department retrieved successfully!"));

        }

        [HttpGet("get-department/guid/{id}")]
        public async Task<IActionResult> GetDepartmentByGuid(Guid id)
        {
            var deparment = await departmentServices.GetDepartmentByGuid(id);

            return Ok(new SuccessResponse<Department>(deparment, $"Department with id: {id} retrieved successfully!"));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto departmentDto)
        {
            var orgId = User.RetrieveSpecificUser("organizationId");

            if (!int.TryParse(orgId, out var orgIdParse))
            {
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, $"Organization id is not a valid integer.");
            }

            var department = new Department
            {
                DeptName = departmentDto.DeptName,
                Description = departmentDto.Description,
                OrganizationId = orgIdParse
            };

            if (!ModelState.IsValid) throw new BadRequestException(ErrorCodes.InvalidRequestModel, $"Your body request is invalid.");

            await departmentServices.AddAsync(department);

            return Ok(new SuccessResponse<Department>(department, $"Department created successfully!"));

        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> RetrieveDepartments(int? pageIndex, int? pageSize)
        {
            var organizationId = User.RetrieveSpecificUser("organizationId");

            if (!int.TryParse(organizationId, out var orgId))
            {
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, $"Organization id is not a valid integer.");
            }

            var departments = await departmentServices.RetrieveDepartment(orgId);

            return Ok(new SuccessResponse<object>(departments, $"Departments retrieved successfully!"));

        }
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("{departmentId:int}")]
        public async Task<IActionResult> RetrieveDepartment(int departmentId)
        {
            var department = await departmentServices.GetByIdAsync(departmentId);

            return Ok(new SuccessResponse<Department>(department, $"Department with id: {departmentId} retrieved successfully!"));

        }

        [Authorize(Roles = "Admin")]
        [HttpGet("department-name")]
        public async Task<IActionResult> GetDepartmentByName(string name)
        {

            var departments = await departmentServices.GetAllAsync();

            var departmentByName = departments.Where(d => d.DeptName.Contains(name)).ToList();

            var mappedDepartment = mapper.Map<List<ReadDepartmentDto>>(departmentByName);

            if (!departmentByName.Any())
            {
                throw new NotFoundException(ErrorCodes.DepartmentNotFound, $"Department with name: {name} not found.");
            }

            return Ok(new SuccessResponse<List<ReadDepartmentDto>>(mappedDepartment, $"Department with name: {name} retrieved successfully!"));

        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{departmentId:int}")]
        public async Task<IActionResult> UpdateDepartment(int departmentId, CreateDepartmentDto departmentDto)
        {
            var department = await departmentServices.GetByIdAsync(departmentId);

            await departmentServices.UpdateAsync(mapper.Map<Department>(departmentDto));

            return Ok(new SuccessResponse<Department>(department, $"Department with id: {departmentId} updated successfully!"));

        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{departmentId:int}")]
        public async Task<IActionResult> DeleteDepartment(int departmentId)
        {
            var department = await departmentServices.GetByIdAsync(departmentId);

            await departmentServices.DeleteAsync(department);

            return Ok(new SuccessResponse<Department>(department, $"Department with id: {departmentId} deleted successfully!"));

        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet("employee-department")]
        public async Task<IActionResult> RetrieveEmpDepartments(PaginationParams pageParams)
        {
            var orgId = User.RetrieveSpecificUser("organizationId");

            if (!int.TryParse(orgId, out var orgIdParse))
            {
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, $"Organization id is not a valid integer.");
            }

            var employeeDepartments = await departmentServices.RetrieveEmployeeDepartments(orgIdParse, pageParams);

            var mappedEmployeeDepartments = mapper.Map<ReadEmployeeDepartmentDTO>(employeeDepartments.Data);

            var response = new PagedResponse<ReadEmployeeDepartmentDTO>(mappedEmployeeDepartments, employeeDepartments.Pagination, $"Employee departments retrieved successfully!");

            return Ok(response);

        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{departmentId:int}/create-emp-department")]
        public async Task<IActionResult> CreateEmployeeDepartment(int departmentId, int supervisorId)
        {
            var organization = User.FindFirstValue("organizationId");

            if (!int.TryParse(organization, out var orgId))
            {
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, $"Organization id is not a valid integer.");
            }

            await departmentServices.CreateEmployeeDepartment(departmentId, supervisorId, orgId);

            return Ok(new SuccessResponse("Employee department created successfully!"));

        }

        [Authorize(Roles = "Admin,HR")]
        [HttpPut("{employeeDeptId:int}/supervisor")]
        public async Task<IActionResult> UpdateDeptSupervisor(int employeeDeptId, int supervisorId)
        {

            if (!ModelState.IsValid)
            {
                throw new BadRequestException(ErrorCodes.InvalidRequestModel, "Your body request is invalid.");
            }

            var department = await departmentServices.GetEmployeeDepartment(employeeDeptId);

            var employee = await employeeServices.GetByIdAsync(supervisorId);

            var employeeDepartment = await departmentServices.UpdateEmployeeDepartmentSupervisor(employee.Id, department.Id);

            if (employeeDepartment == null)
            {
                throw new NotFoundException(ErrorCodes.EmployeeDepartmentNotFound, $"Employee department with id: {employeeDeptId} not found.");
            }

            return Ok(new SuccessResponse($"Employee department supervisor updated successfully!"));

        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{supervisorDepartmentId:int}/employee")]
        public async Task<IActionResult> AddORUpdateEmployeeDepartment(int supervisorDepartmentId, int employeeId)
        {

            var department = await departmentServices.GetEmployeeDepartment(supervisorDepartmentId);

            if (department == null)
            {
                throw new NotFoundException(ErrorCodes.EmployeeDepartmentNotFound, $"Employee department with id: {supervisorDepartmentId} not found.");
            }

            var employee = await employeeServices.GetByIdAsync(employeeId);

            if (employee == null)
            {
                throw new NotFoundException(ErrorCodes.EmployeeNotFound, $"Employee with id: {employeeId} not found.");
            }

            employee.EmployeeDepartmentId = supervisorDepartmentId;
            await employeeServices.UpdateAsync(employee);

            return Ok(new SuccessResponse($"Employee department updated successfully!"));

        }


    }
}
