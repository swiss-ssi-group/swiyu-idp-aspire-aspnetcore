using Idp.Swiyu.IdentityProvider.SwiyuServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Swiyu.Aspire.Mgmt.Controllers;

[AllowAnonymous]
[Route("api/[controller]")]
[ApiController]
public class StatusController : ControllerBase
{
    private readonly VerificationService _verificationService;

    public StatusController(VerificationService verificationService)
    {
        _verificationService = verificationService;
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

            // In a business app we can use the data from the verificationModel
            // Verification data:
            // Use: wallet_response/credential_subject_data

            return Ok(verificationModel);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "400", error_description = ex.Message });
        }
    }
}
