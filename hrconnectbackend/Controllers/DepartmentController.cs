using AutoMapper;
using hrconnectbackend.IRepositories;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers;

[ApiController]
[Route("[controller]")]
public class DepartmentController : Controller
{
    private readonly IMapper _mapper;
    private readonly IDepartmentRepositories _departmentRepository;
    private readonly IEmployeeRepositories _employeeRepositories;

    public DepartmentController(IDepartmentRepositories departmentRepository, IMapper mapper, IEmployeeRepositories employeeRepositories)
    {
        _employeeRepositories = employeeRepositories;
        _departmentRepository = departmentRepository;
        _mapper = mapper;
    }


    [HttpGet("department")]
    public async Task<IActionResult> GetAllDeparments()
    {
        var departments = await _departmentRepository.GetAllAsync();

        if (departments.Count == 0)
        {
            return NotFound(new { message = "No departments found" });
        }

        return Ok(departments);
    }

    [HttpPut("create/assignEmployeeDepartment/{employeeId}/{departmentId}")]
    public async Task<IActionResult> AssignDept(int employeeId, int departmentId)
    {
        var employee = await _employeeRepositories.GetEmployeeByIdAsync(employeeId);
        var department = await _departmentRepository.GetByIdAsync(departmentId);

        if (employee == null)
        {
            return NotFound(new { message = "Employee not found" });
        }

        if (department == null)
        {
            return NotFound(new { message = "Department not found" });
        }

        employee.DepartmentId = department.DepartmentId;

        if (employee.Id != department.ManagerId && employee.SupervisorId == null)
        {
            employee.SupervisorId = department.ManagerId;
        }


        var dto = _mapper.Map<Employee>(employee);

        await _employeeRepositories.UpdateEmployeeAsync(dto);

        return Ok("employee assigned to department successfully");
    }


    [HttpGet("department/{id}")]
    public async Task<IActionResult> GetDepartment(int id)
    {
        var department = await _departmentRepository.GetByIdAsync(id);

        if (department == null)
        {
            return NotFound(new { message = "Department not found" });
        }

        return Ok(department);
    }


    [HttpPost("department/create")]
    public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDTO department)
    {
        try
        {
            if (department == null)
            {
                return BadRequest(new { message = "Invalid department data" });
            }

            var departmentEntity = _mapper.Map<Department>(department);

            await _departmentRepository.AddAsync(departmentEntity);


            return CreatedAtAction(nameof(GetDepartment), new { id = departmentEntity.DepartmentId }, department);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    [HttpPut("department/update/{id}")]
    public async Task<IActionResult> UpdateDepartment(int id, [FromBody] CreateDepartmentDTO department)
    {
        try
        {
            if (department == null)
            {
                return BadRequest(new { message = "Invalid department data" });
            }

            // Get the existing department by ID
            var existingDepartment = await _departmentRepository.GetByIdAsync(id);

            if (existingDepartment == null)
            {
                return NotFound(new { message = "Department not found" });
            }

            // Update specific properties if provided in the request
            if (!string.IsNullOrEmpty(department.DeptName))
            {
                existingDepartment.DeptName = department.DeptName;
            }


            // Save changes to the repository
            await _departmentRepository.UpdateDepartmentByAsync(existingDepartment);

            // Return the updated department as DTO

            return Ok(_departmentRepository); // Return the updated department in the response
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating department record");
        }
    }

    [HttpDelete("department/delete/{id}")]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        try
        {
            var department = await _departmentRepository.GetByIdAsync(id);

            if (department == null)
                return NotFound(new { message = "Department not found" });

            await _departmentRepository.DeleteAsync(department);

            return Ok(new { message = "Department deleted successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting department record");
        }
    }

}