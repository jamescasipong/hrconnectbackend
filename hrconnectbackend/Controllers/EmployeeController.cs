using Microsoft.AspNetCore.Mvc;
using hrconnectbackend.Data;
using hrconnectbackend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using hrconnectbackend.Repositories;
using hrconnectbackend.IRepositories;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeRepositories _employeeRepositories;
        public EmployeeController (IEmployeeRepositories employeeRepositories) { _employeeRepositories = employeeRepositories; }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_employeeRepositories.GetAll());
        }

        [HttpGet("{id}")]
        public IActionResult GetEmployee(int id)
        {
            return Ok(_employeeRepositories.GetEmployee(id));
        }
    }
}
