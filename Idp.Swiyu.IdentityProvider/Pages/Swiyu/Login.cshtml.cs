using Duende.IdentityModel.Client;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using Idp.Swiyu.IdentityProvider.Models;
using Idp.Swiyu.IdentityProvider.SwiyuServices;
using ImageMagick;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.VisualBasic;
using Net.Codecrete.QrCodeGenerator;
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
    public InputModel Input { get; set; } = default!;

    private readonly VerificationService _verificationService;
    private readonly string? _swiyuOid4vpUrl;

    [BindProperty]
    public string? VerificationId { get; set; }

    [BindProperty]
    public string? QrCodeUrl { get; set; } = string.Empty;

    [BindProperty]
    public byte[] QrCodePng { get; set; } = [];

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

            Input = new InputModel
            {
                ReturnUrl = returnUrl
            };
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
            
            // TODO
            // Check user is registered
            // signin
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "400", error_description = ex.Message });
        }

        // check if we are in the context of an authorization request
        var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

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
