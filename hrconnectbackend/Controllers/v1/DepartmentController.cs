using System.Security.Claims;
using AutoMapper;
using hrconnectbackend.Helper;
using hrconnectbackend.Interface.Services;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using MongoDB.Bson;

namespace hrconnectbackend.Controllers.v1
{
    [Authorize]
    [Route("api/v{version:apiVersion}/department")]
    [ApiVersion("1.0")]
    public class DepartmentController : ControllerBase
    {

        private readonly IDepartmentServices _departmentServices;
        private readonly IMapper _mapper;
        private readonly IEmployeeServices _employeeServices;
        public DepartmentController(IDepartmentServices departmentServices, IEmployeeServices employeeServices, IMapper mapper) {
            _departmentServices = departmentServices;
            _mapper = mapper;
            _employeeServices = employeeServices;
        }

        [Authorize]
        [HttpGet("my-department")]
        public async Task<IActionResult> GetDepartment(){
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (currentUserId == null){
                    return Unauthorized(new ApiResponse(false, $"User not authenticated."));
                }

                var employee = await _employeeServices.GetByIdAsync(int.Parse(currentUserId));


                if (employee == null){
                    return NotFound(new ApiResponse(false, $"Employee not found."));
                }

                if (employee.DepartmentId == null){
                    return Ok(new ApiResponse<dynamic>(false, $"Employee not assigned to any department.", null));
                }

                var department = await _departmentServices.GetByIdAsync(employee.DepartmentId.Value);

                if (department == null){
                    return NotFound(new ApiResponse<ReadDepartmentDTO>(false, $"Department not found.", null));
                }

                var mappedDepartment = _mapper.Map<ReadDepartmentDTO>(department);

                return Ok(new ApiResponse<ReadDepartmentDTO>(true, $"Department retrieved successfully!", mappedDepartment));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateDepartment(CreateDepartmentDTO departmentDTO)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(new ApiResponse(false, ModelState.ToJson().ToString()));

                var department = _mapper.Map<Department>(departmentDTO);

                await _departmentServices.AddAsync(department);

                return Ok(new ApiResponse<ReadDepartmentDTO>(true, $"Department created successfully!", _mapper.Map<ReadDepartmentDTO>(department)));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> RetrieveDepartments(int? pageIndex, int? pageSize)
        {
            try
            {
                var departments = await _departmentServices.GetAllAsync();

                if (pageIndex != null && pageSize != null)
                {
                    departments = departments.Take((pageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
                }

                return Ok(new ApiResponse<List<Department>>(true, $"Departments retrieved successfully", departments));
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

                var department = await _departmentServices.GetByIdAsync(departmentId);

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
                var departments = await _departmentServices.GetAllAsync();

                var departmentByName = departments.Where(d => d.DeptName.Contains(name)).ToList();

                var mappedDepartment = _mapper.Map<List<ReadDepartmentDTO>>(departmentByName);

                if (!departmentByName.Any())
                {
                    return Ok(new ApiResponse<List<ReadDepartmentDTO>>(true, $"Department containing {name} not found.", mappedDepartment));
                }

                return Ok(new ApiResponse<List<ReadDepartmentDTO>>(true, $"Department containing {name} retrieved sucessfully!", mappedDepartment));
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{departmentId:int}")]
        public async Task<IActionResult> UpdateDepartment(int departmentId, CreateDepartmentDTO departmentDTO)
        {
            try
            {
                var department = await _departmentServices.GetByIdAsync(departmentId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Department with id: {departmentId} not found."));
                }

                await _departmentServices.UpdateAsync(_mapper.Map<Department>(departmentDTO));

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
                var department = await _departmentServices.GetByIdAsync(departmentId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Department with id: {departmentId} not found."));
                }

                await _departmentServices.DeleteAsync(department);

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

                var department = await _departmentServices.GetByIdAsync(departmentId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Unable to process. Department with {departmentId} not found."));
                }

                var employee = await _employeeServices.GetByIdAsync(employeeId);

                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Unable to process. Employee with id: {employeeId} not found."));
                }

                department.ManagerId = employeeId;
                await _departmentServices.UpdateAsync(department);

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
                var department = await _departmentServices.GetByIdAsync(departmentId);

                if (department == null)
                {
                    return NotFound(new ApiResponse(false, $"Department with id: {departmentId} not found."));
                }

                var employee = await _employeeServices.GetByIdAsync(employeeId);

                if (employee == null)
                {
                    return NotFound(new ApiResponse(false, $"Unable to process. Employee with id: {employeeId} not found."));
                }

                employee.DepartmentId = departmentId;
                await _employeeServices.UpdateAsync(employee);

                return Ok(new ApiResponse(true, $"Employee with id: {employeeId} was added or deleted"));
            }
            catch(Exception)
            {
                return StatusCode(500, new ApiResponse(false, $"Internal Server Error"));
            }
        }


    }
}
