using Idp.Swiyu.IdentityProvider.Models;
using Idp.Swiyu.IdentityProvider.SwiyuServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Swiyu.Aspire.Mgmt.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatusController : ControllerBase
{
    private readonly VerificationService _verificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public StatusController(VerificationService verificationService,
        UserManager<ApplicationUser> userManager)
    {
        _verificationService = verificationService;
        _userManager = userManager;
    }

    [HttpGet("verification-response")]
    public async Task<ActionResult> VerificationResponseAsync([FromQuery] string? id)
    {
        try
        {
            if (id == null)
            {
                return BadRequest(new { error = "400", error_description = "Missing argument 'id'" });
            }

            var verificationModel = await _verificationService.GetVerificationStatus(id);

            if (verificationModel != null && verificationModel.state == "SUCCESS")
            {
                // In a business app we can use the data from the verificationModel
                // Verification data:
                // Use: wallet_response/credential_subject_data
                var verificationClaims = _verificationService.GetVerifiedClaims(verificationModel);

                var email = User.Claims.FirstOrDefault(c => c.Type == "email");
                var user = await _userManager.FindByEmailAsync(email.Value);
            }
            

            return Ok(verificationModel);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "400", error_description = ex.Message });
        }
    }
}
