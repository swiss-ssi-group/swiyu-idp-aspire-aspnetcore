using Duende.IdentityModel;
using Idp.Swiyu.IdentityProvider.Data;
using Idp.Swiyu.IdentityProvider.Models;
using Idp.Swiyu.IdentityProvider.SwiyuServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Security.Claims;

namespace Swiyu.Aspire.Mgmt.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatusController : ControllerBase
{
    private readonly VerificationService _verificationService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _applicationDbContext;

    public StatusController(VerificationService verificationService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext applicationDbContext)
    {
        _verificationService = verificationService;
        _userManager = userManager;
        _applicationDbContext = applicationDbContext;
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

                var user = await _userManager.FindByEmailAsync(GetEmail(User.Claims)!);

                // TODO check if swiyu already used somewhere

                if (user != null && user.SwiyuIdentityId == null)
                {
                    var swiyuIdentity = new SwiyuIdentity
                    {
                        UserId = user.Id,
                        BirthDate = verificationClaims.BirthDate,
                        FamilyName = verificationClaims.FamilyName,
                        BirthPlace = verificationClaims.BirthPlace,
                        GivenName = verificationClaims.GivenName,
                        Email = user.Email!
                    };

                    // Save to DB
                    user.SwiyuIdentityId = swiyuIdentity.Id;

                    await _applicationDbContext.SaveChangesAsync();
                }
            }

            return Ok(verificationModel);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "400", error_description = ex.Message });
        }
    }

    public static string? GetEmail(IEnumerable<Claim> claims)
    {
        var email = claims.FirstOrDefault(t => t.Type == ClaimTypes.Email);

        if (email != null)
        {
            return email.Value;
        }

        email = claims.FirstOrDefault(t => t.Type == JwtClaimTypes.Email);

        if (email != null)
        {
            return email.Value;
        }

        email = claims.FirstOrDefault(t => t.Type == "preferred_username");

        if (email != null)
        {
            var isNameAndEmail = IsEmailValid(email.Value);
            if (isNameAndEmail)
            {
                return email.Value;
            }
        }

        return null;
    }

    public static bool IsEmailValid(string email)
    {
        if (!MailAddress.TryCreate(email, out var mailAddress))
            return false;

        // And if you want to be more strict:
        var hostParts = mailAddress.Host.Split('.');
        if (hostParts.Length == 1)
            return false; // No dot.
        if (hostParts.Any(p => p == string.Empty))
            return false; // Double dot.
        if (hostParts[^1].Length < 2)
            return false; // TLD only one letter.

        if (mailAddress.User.Contains(' '))
            return false;
        if (mailAddress.User.Split('.').Any(p => p == string.Empty))
            return false; // Double dot or dot at end of user part.

        return true;
    }
}
