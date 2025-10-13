using Duende.IdentityModel.Client;
using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Idp.Swiyu.IdentityProvider.Models;
using Idp.Swiyu.IdentityProvider.SwiyuServices;
using ImageMagick;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using Net.Codecrete.QrCodeGenerator;
using System.Security.Claims;
using System.Text.Json;

namespace Idp.Swiyu.IdentityProvider.Pages.Login;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IEventService _events;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IIdentityProviderStore _identityProviderStore;
    private readonly IHttpClientFactory _clientFactory;

    [BindProperty]
    public string ReturnUrl { get; set; } = default!;

    private readonly VerificationService _verificationService;
    private readonly string? _swiyuOid4vpUrl;

    [BindProperty]
    public string? VerificationId { get; set; }

    [BindProperty]
    public string? QrCodeUrl { get; set; } = string.Empty;

    [BindProperty]
    public byte[]? QrCodePng { get; set; } = [];

    public LoginModel(
        IIdentityServerInteractionService interaction,
        IAuthenticationSchemeProvider schemeProvider,
        IIdentityProviderStore identityProviderStore,
        IEventService events,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        VerificationService verificationService,
        IHttpClientFactory clientFactory,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _interaction = interaction;
        _schemeProvider = schemeProvider;
        _identityProviderStore = identityProviderStore;
        _events = events;

        _clientFactory = clientFactory;

        _verificationService = verificationService;
        _swiyuOid4vpUrl = configuration["SwiyuOid4vpUrl"];
        QrCodeUrl = QrCodeUrl.Replace("{OID4VP_URL}", _swiyuOid4vpUrl);
    }

    public async Task<IActionResult> OnGet(string? returnUrl)
    {
        if (returnUrl != null)
        {
            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);

            ReturnUrl = returnUrl;
        }

        var presentation = await _verificationService
            .CreateBetaIdVerificationPresentationAsync();

        var verificationResponse = JsonSerializer.Deserialize<CreateVerificationPresentationModel>(presentation);
        // verification_url
        QrCodeUrl = verificationResponse!.verification_url;

        var qrCode = QrCode.EncodeText(verificationResponse!.verification_url, QrCode.Ecc.Quartile);
        QrCodePng = qrCode.ToPng(20, 4, MagickColors.Black, MagickColors.White);

        VerificationId = verificationResponse.id;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        VerificationClaims verificationClaims = null;
        try
        {
            if (VerificationId == null)
            {
                return BadRequest(new { error = "400", error_description = "Missing argument 'VerificationId'" });
            }

            var verificationModel = await RequestSwiyuClaimsAsync(1, VerificationId);

            verificationClaims = _verificationService.GetVerifiedClaims(verificationModel);

            // check if we are in the context of an authorization request
            var context = await _interaction.GetAuthorizationContextAsync(ReturnUrl);

            if (ModelState.IsValid)
            {
                var claims = new List<Claim>
                {
                    new Claim("name", verificationClaims.GivenName),
                    new Claim("family_name", verificationClaims.FamilyName),
                    new Claim("birth_place", verificationClaims.BirthPlace),
                    new Claim("birth_date", verificationClaims.BirthDate)
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    "Identity.Application", // scheme
                    verificationClaims.GivenName,
                    verificationClaims.FamilyName);

                var authProperties = new AuthenticationProperties();

                //TODO switch to user login once the register process is complete
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // var user = await _userManager.FindByNameAsync(Input.Username!);
                // await _events.RaiseAsync(new UserLoginSuccessEvent(user!.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));
                Telemetry.Metrics.UserLogin(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider);

                if (context != null)
                {
                    // This "can't happen", because if the ReturnUrl was null, then the context would be null
                    ArgumentNullException.ThrowIfNull(ReturnUrl, nameof(ReturnUrl));

                    if (context.IsNativeClient())
                    {
                        // The client is native, so this change in how to
                        // return the response is for better UX for the end user.
                        return this.LoadingPage(ReturnUrl);
                    }

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                    return Redirect(ReturnUrl ?? "~/");
                }

                // request for a local page
                if (Url.IsLocalUrl(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }
                else if (string.IsNullOrEmpty(ReturnUrl))
                {
                    return Redirect("~/");
                }
                else
                {
                    // user might have clicked on a malicious link - should be logged
                    throw new ArgumentException("invalid return URL");
                }
                

                const string error = "invalid credentials";
                //await _events.RaiseAsync(new UserLoginFailureEvent(Username, error, clientId: context?.Client.ClientId));
                Telemetry.Metrics.UserLoginFailure(context?.Client.ClientId, IdentityServerConstants.LocalIdentityProvider, error);
                ModelState.AddModelError(string.Empty, LoginOptions.InvalidCredentialsErrorMessage);
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "400", error_description = ex.Message });
        }

        return Page();
    }

    internal async Task<VerificationManagementModel> RequestSwiyuClaimsAsync(int interval, string verificationId)
    {
        var client = _clientFactory.CreateClient();

        while (true)
        {
            
            var verificationModel = await _verificationService.GetVerificationStatus(verificationId);

            if (verificationModel != null && verificationModel.state == "SUCCESS")
            {
                return verificationModel;
            }
            else
            {
                await Task.Delay(interval * 1000);
            }
        }
    }
}
