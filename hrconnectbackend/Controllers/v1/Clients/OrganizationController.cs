using System.Security.Claims;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.RequestModel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController(IOrganizationServices organizationServices) : ControllerBase
    {

        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganization organization)
        {
            var validate = TryValidateModel(organization);
            
            if (!validate) return BadRequest(ModelState);
            
            var createdOrganization = await organizationServices.CreateOrganization(organization);
            
            return Ok(createdOrganization);
        }
        
        
        [HttpPatch("{organizationId}")]
        public async Task<IActionResult> Patch(int organizationId, [FromBody] JsonPatchDocument<Organization> patch)
        {

            
            // Call the service to apply the patch to the Organization entity
            var (original, patched, isValid) = await organizationServices.ApplyPatchAsync(organizationId, patch);
            
            // If the organization was not found or if the patch is invalid, return BadRequest
            if (original == null || !isValid)
            {
                return NotFound("Not found nigga"); // Return appropriate error message
            }

            // Perform additional validation on the patched entity (in the controller)
            if (string.IsNullOrWhiteSpace(patched.Name))
            {
                ModelState.AddModelError("Name", "Name is required.");
            }

            if (string.IsNullOrWhiteSpace(patched.Address))
            {
                ModelState.AddModelError("Address", "Address is required.");
            }

            if (organizationServices.IsNameDuplicate(patched.Name, organizationId))
            {
                ModelState.AddModelError("Name", "An organization with the same name already exists.");
            }

            // If ModelState is invalid, return a BadRequest with validation errors
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Return validation errors
            }

            // Save changes to the database
            await organizationServices.SaveChangesAsync();

            // Return both the original and patched models
            var model = new
            {
                original,
                patched
            };

            return Ok(model);
        }
        
        [HttpGet("{organizationId}/storage-usage")]
        public async Task<IActionResult> GetStorageUsed(int organizationId)
        {
            try
            {
                long storageUsedBytes = await organizationServices.GetStorageUsedByOrganizationAsync(organizationId);
                var storageUsedMB = storageUsedBytes / (1024 * 1024);  // Convert bytes to MB
                return Ok(new { StorageUsedMB = storageUsedMB });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrganization()
        {
            var organization = User.FindFirstValue("organizationId");

            if (!int.TryParse(organization, out var organizationId))
            {
                return NotFound("Not found");
            }
            
            var org = await organizationServices.GetByIdAsync(organizationId);
            
            return Ok(org);
        }

    }
}
