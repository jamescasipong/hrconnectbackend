using AutoMapper;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly AuthRepositories _authRepositories;
        private readonly IMapper _mapper;

        public AuthController(AuthRepositories authRepositories, IMapper mapper)
        {
            _authRepositories = authRepositories;
            _mapper = mapper;
        }


        // GET: /auth/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuth(int id)
        {
            var auth = await _authRepositories.GetByIdAsync(id);

            if (auth == null)
                return NotFound(new { message = "Not Found" });

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var authDTO = _mapper.Map<AuthDTO>(auth);
            return Ok(authDTO);
        }

        // GET: /auth
        [HttpGet]
        public async Task<IActionResult> GetListAuth()
        {
            var auths = await _authRepositories.GetAllAsync();

            if (auths.Count == 0) return NotFound();

            if (!ModelState.IsValid) return BadRequest(ModelState);

            var authsDTO = _mapper.Map<List<AuthDTO>>(auths);
            return Ok(authsDTO);
        }

        // POST: /auth (Create)
        [HttpPost]
        public async Task<IActionResult> CreateAuth([FromBody] AuthDTO authDTO)
        {
            if (authDTO == null)
                return BadRequest(new { message = "Invalid data" });

            // Map the DTO to the entity
            var auth = _mapper.Map<Auth>(authDTO);

            // Save the entity in the repository
            var createdAuth = await _authRepositories.AddAsync(auth);

            if (createdAuth == null)
                return StatusCode(500, new { message = "Failed to create the record" });

            // Map the created entity to DTO for response
            var createdAuthDTO = _mapper.Map<AuthDTO>(createdAuth);

            // Return the created object with 201 status code
            return CreatedAtAction(nameof(GetAuth), new { id = createdAuthDTO.AuthEmpId }, createdAuthDTO);
        }

        // DELETE: /auth/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuth(int id)
        {
            var auth = await _authRepositories.GetByIdAsync(id);

            if (auth == null)
                return NotFound(new { message = "Auth not found" });

            await _authRepositories.DeleteAsync(auth);

            return Ok(new
            {
                message = "Auth deleted"
            });
        }


    }
}
