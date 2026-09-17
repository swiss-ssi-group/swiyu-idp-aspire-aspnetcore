using Duende.IdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace Idp.Swiyu.IdentityProvider.Pages.Home;

[AllowAnonymous]
public class Index : PageModel
{
    public string Version => typeof(Duende.IdentityServer.Hosting.IdentityServerMiddleware).Assembly
                                 .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                                 ?.InformationalVersion.Split('+').First()
                             ?? "unavailable";
}
