using Microsoft.AspNetCore.Mvc;
using hrconnectbackend.Data;
using hrconnectbackend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using hrconnectbackend.Repositories;
using hrconnectbackend.IRepositories;
using AutoMapper;
using hrconnectbackend.Models.DTOs;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepositories _employeeRepository;
        private readonly IMapper _mapper;
        public EmployeeController (IEmployeeRepositories employeeRepository, IMapper mapper) 
        { 
            _employeeRepository = employeeRepository; 
            _mapper = mapper; 
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            
            var employees = await _employeeRepository.GetAllEmployeesAsync();

            var employeesDTO =  _mapper.Map<ICollection<EmployeeDTO>>(employees);

            if (employees.Count == 0)
            {
                return NotFound();
            }

            return Ok(employeesDTO);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            return Ok(employee);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] Employee employee)
        {
            try
            {
                if (employee == null)
                    return BadRequest();

                await _employeeRepository.AddEmployeeAsync(employee);

                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                "Error creating new employee record");
            }

        }


    }
}
