using System.Security.Claims;
using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.RequestModel;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController(IOrganizationServices organizationServices, ILogger<OrganizationController> logger) : ControllerBase
    {
        // [UserRole("Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganization organization)
        {
            var validate = TryValidateModel(organization);
            
            if (!validate) return BadRequest(ModelState);
            
            var createdOrganization = await organizationServices.CreateOrganization(organization);
            
            return Ok(createdOrganization);
        }
        
        [UserRole("Admin,Operator")]
        [HttpPatch("{organizationId}")]
        public async Task<IActionResult> Patch(int organizationId, [FromBody] JsonPatchDocument<Organization> patch)
        {

            
            // Call the service to apply the patch to the Organization entity
            var (original, patched, isValid) = await organizationServices.ApplyPatchAsync(organizationId, patch);
            
            // If the organization was not found or if the patch is invalid, return BadRequest
            if (original == null || !isValid || patched == null)
            {
                return NotFound("Organization not found or invalid patch.");
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
                var storageUsedMb = storageUsedBytes / (1024 * 1024);  // Convert bytes to MB
                return Ok(new { StorageUsedMB = storageUsedMb });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        
        [UserRole("Employee")]
        [HttpGet("my-organization")]
        public async Task<IActionResult> GetMyOrganization()
        {
            var organization = User.FindFirstValue("organizationId");
            
            logger.LogInformation("org: {organization}", organization);

            if (!int.TryParse(organization, out var organizationId))
            {
                return NotFound("Not found");
            }
            
            var org = await organizationServices.GetByIdAsync(organizationId);
            
            return Ok(org);
        }
        
        [UserRole("Operator")]
        [HttpGet]
        public async Task<IActionResult> GetOrganizations()
        {
            var orgs = await organizationServices.GetAllAsync();
            
            return Ok(orgs);
        }
        
        [UserRole("Operator")]
        [HttpDelete("{organizationId}")]
        public async Task<IActionResult> DeleteOrganization(int organizationId)
        {
            var org = await organizationServices.GetByIdAsync(organizationId);
            
            if (org == null) return NotFound("Not found");
            
            await organizationServices.DeleteAsync(org);
            return Ok($"{organizationId} deleted");
        }
    }
}
