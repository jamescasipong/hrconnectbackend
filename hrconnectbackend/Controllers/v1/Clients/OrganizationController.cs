using System.Security.Claims;
using hrconnectbackend.Attributes.Authorization.Requirements;
using hrconnectbackend.Interface.Services.Clients;
using hrconnectbackend.Models;
using hrconnectbackend.Models.DTOs;
using hrconnectbackend.Models.RequestModel;
using hrconnectbackend.Models.Response;
using hrconnectbackend.Services.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace hrconnectbackend.Controllers.v1.Clients
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController(IOrganizationServices organizationServices, IUserAccountServices userAccountServices, IAuthService authServices, ILogger<OrganizationController> logger) : ControllerBase
    {
        // [UserRole("Admin")]
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationDTO organization)
        {
            var validate = TryValidateModel(organization);
            
            if (!validate) return BadRequest(ModelState);

            if (User == null)
            {
                return Unauthorized();
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return BadRequest(new ApiResponse(false, $"Invalid Request"));
            }

            var newOrg = new Organization
            {
                Name = organization.OrgName,
                Address = organization.Address,
                ContactEmail = organization.ContactEmail,
                Zipcode = organization.Zipcode,
                City = organization.City,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            
            try
            {
                var createdOrganization = await organizationServices.CreateOrganization(userId, newOrg);

                if (createdOrganization == null)
                {
                    return BadRequest(new ApiResponse<Organization>(false, $"Failed to create organization"));
                }

                var user = await userAccountServices.GetByIdAsync(userId);

                if (user == null)
                {
                    return NotFound(new ApiResponse<Organization>(false, $"User not found"));
                }

                var userExist = await userAccountServices.GetByIdAsync(userId);

                if (userExist == null)
                {
                    return NotFound(new ApiResponse<Organization>(false, $"User not found"));
                }


                AuthResponse authResponse = await authServices.GenerateTokens(userExist);
                authServices.SetAccessTokenCookie(authResponse, Response);

                return Ok(new ApiResponse<Organization>(true, $"Account created successfully", createdOrganization));

            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new ApiResponse<Organization>(false, $"User not found: {ex.Message}"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<Organization>(false, $"Error creating organization: {ex.Message}"));
            }
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
        
        [Authorize]
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
