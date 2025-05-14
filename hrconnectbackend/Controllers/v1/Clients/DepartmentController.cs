using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Constants;
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
        public async Task<IActionResult> GetDepartment()
        {

            var currentUserId = User.FindFirstValue("EmployeeId");

            if (currentUserId == null)
            {
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, $"User not authenticated."));
            }

            var employee = await employeeServices.GetByIdAsync(int.Parse(currentUserId));

            if (employee == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeNotFound, $"Employee not found with the provided ID {currentUserId}."));
            }

            if (employee.EmployeeDepartmentId == null)
            {
                return StatusCode(409, new ErrorResponse(ErrorCodes.EmployeeNotAssignedToDepartment, $"Employee does not belong to any department."));
            }

            var department = await departmentServices.GetEmployeeDepartment(employee.EmployeeDepartmentId.Value);

            if (department == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.DepartmentNotFound, $"Department not found with the provided ID {employee.EmployeeDepartmentId}."));
            }

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
            var orgId = User.FindFirstValue("organizationId");

            if (orgId == null)
            {
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, $"User not authenticated."));
            }

            if (!int.TryParse(orgId, out var orgIdParse))
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, $"Organization id is not a valid integer."));
            }

            var department = new Department
            {
                DeptName = departmentDto.DeptName,
                Description = departmentDto.Description,
                OrganizationId = orgIdParse
            };

            if (!ModelState.IsValid) return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, "Your body request is invalid."));

            await departmentServices.AddAsync(department);

            return Ok(new SuccessResponse<Department>(department, $"Department created successfully!"));

        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> RetrieveDepartments(int? pageIndex, int? pageSize)
        {
            if (!int.TryParse(User.FindFirstValue("organizationId"), out var orgId))
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, $"Organization id is not a valid integer."));
            }

            var departments = await departmentServices.RetrieveDepartment(orgId);

            return Ok(new SuccessResponse<object>(departments, $"Departments retrieved successfully!"));

        }
        [Authorize(Roles = "Admin,HR")]
        [HttpGet("{departmentId:int}")]
        public async Task<IActionResult> RetrieveDepartment(int departmentId)
        {

            var currentUserId = User.FindFirstValue("EmployeeId");
            var userRoles = User.FindAll(ClaimTypes.Role).Select(a => a.Value).ToList();

            if (currentUserId == null)
            {
                return StatusCode(401, new ErrorResponse(ErrorCodes.Unauthorized, $"User not authenticated. Please login."));
            }

            var department = await departmentServices.GetByIdAsync(departmentId);

            if (department == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.DepartmentNotFound, $"Department with id: {departmentId} not found."));
            }

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
                return StatusCode(404, new ErrorResponse(ErrorCodes.DepartmentNotFound, $"Department with name: {name} not found."));
            }

            return StatusCode(200, new SuccessResponse<List<ReadDepartmentDto>>(mappedDepartment, $"Department with name: {name} retrieved successfully!"));

        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{departmentId:int}")]
        public async Task<IActionResult> UpdateDepartment(int departmentId, CreateDepartmentDto departmentDto)
        {

            var department = await departmentServices.GetByIdAsync(departmentId);

            if (department == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.DepartmentNotFound, $"Department with id: {departmentId} not found."));
            }

            await departmentServices.UpdateAsync(mapper.Map<Department>(departmentDto));

            return Ok(new SuccessResponse<Department>(department, $"Department with id: {departmentId} updated successfully!"));

        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{departmentId:int}")]
        public async Task<IActionResult> DeleteDepartment(int departmentId)
        {

            var department = await departmentServices.GetByIdAsync(departmentId);

            if (department == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.DepartmentNotFound, $"Department with id: {departmentId} not found."));
            }

            await departmentServices.DeleteAsync(department);

            return Ok(new SuccessResponse<Department>(department, $"Department with id: {departmentId} deleted successfully!"));

        }

        [Authorize(Roles = "Admin,HR")]
        [HttpGet("employee-department")]
        public async Task<IActionResult> RetrieveEmpDepartments(int? pageIndex, int? pageSize)
        {
            var orgId = User.FindFirstValue("organizationId");

            if (!int.TryParse(orgId, out var orgIdParse))
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, $"Organization id is not a valid integer."));
            }

            var employeeDepartments = await departmentServices.RetrieveEmployeeDepartments(orgIdParse, pageIndex, pageSize);
            var employeeDepartmentDTO = mapper.Map<List<ReadEmployeeDepartmentDTO>>(employeeDepartments);

            return Ok(new SuccessResponse<List<ReadEmployeeDepartmentDTO>>(employeeDepartmentDTO, $"Employee departments retrieved successfully!"));

        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{departmentId:int}/create-emp-department")]
        public async Task<IActionResult> CreateEmployeeDepartment(int departmentId, int supervisorId)
        {
            var organization = User.FindFirstValue("organizationId");

            if (!int.TryParse(organization, out var orgId))
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, $"Organization id is not a valid integer."));
            }

            await departmentServices.CreateEmployeeDepartment(departmentId, supervisorId, orgId);

            return Ok(new SuccessResponse("Employee department created successfully!"));

        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{employeeDeptId:int}/supervisor")]
        public async Task<IActionResult> UpdateDeptSupervisor(int employeeDeptId, int supervisorId)
        {

            if (!ModelState.IsValid)
            {
                return StatusCode(400, new ErrorResponse(ErrorCodes.InvalidRequestModel, "Your body request is invalid."));
            }

            var department = await departmentServices.GetEmployeeDepartment(employeeDeptId);

            if (department == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeDepartmentNotFound, $"Employee department with id: {employeeDeptId} not found."));
            }

            var employee = await employeeServices.GetByIdAsync(supervisorId);

            if (employee == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeNotFound, $"Employee with id: {supervisorId} not found."));
            }

            var employeeDepartment = await departmentServices.UpdateEmployeeDepartmentSupervisor(employee.Id, department.Id);

            if (employeeDepartment == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeDepartmentNotFound, $"Employee department with id: {employeeDeptId} not found."));
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
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeDepartmentNotFound, $"Employee department with id: {supervisorDepartmentId} not found."));
            }

            var employee = await employeeServices.GetByIdAsync(employeeId);

            if (employee == null)
            {
                return StatusCode(404, new ErrorResponse(ErrorCodes.EmployeeNotFound, $"Employee with id: {employeeId} not found."));
            }

            employee.EmployeeDepartmentId = supervisorDepartmentId;
            await employeeServices.UpdateAsync(employee);

            return Ok(new SuccessResponse($"Employee department updated successfully!"));

        }


    }
}
