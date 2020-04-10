using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using B2C.PASSDemo.Authentication;
using B2C.PASSDemo.Middleware;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

namespace B2C.PASSDemo.Controllers
{
    [RequireHttps]
    public class HomeController : Controller
    {
        public HomeController(
            IConfiguration configuration,
            IAuthenticationSchemeProvider authenticationSchemeProvider
        ) : base()
        {
            Configuration = configuration;
            AuthenticationSchemeProvider = authenticationSchemeProvider;
        }

        public IConfiguration Configuration { get; }

        public IAuthenticationSchemeProvider AuthenticationSchemeProvider { get; }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult SignIn(string policy)
        {
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(Confirmed))
            }, policy);
        }

        [Authorize]
        public IActionResult Confirmed()
        {
            ViewBag.Name = User.Claims?.FirstOrDefault(c => c.Type == "name")?.Value ?? "";
            ViewBag.Email = User.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "";
            ViewBag.Phone = User.Claims?.FirstOrDefault(c => c.Type == "phone_number")?.Value ?? "";
            ViewBag.Phone = TryGetProperlyFormattedNumber(ViewBag.Phone) ?? ViewBag.Phone;
            ViewBag.Claims = User.Claims ?? Enumerable.Empty<Claim>();
            return View();
        }

        public async Task<IActionResult> SignOut()
        {
            var schemesToSignOut = new[] { CookieAuthenticationDefaults.AuthenticationScheme }.ToList();

            var currentAuthPolicy = User.Claims?.FirstOrDefault(c => c.Type == AuthConfig.ClaimTypes.TrustFrameworkPolicy)
                ?? User.Claims?.FirstOrDefault(c => c.Type == AuthConfig.ClaimTypes.AuthnContextReference);
            if (currentAuthPolicy != null)
            {
                schemesToSignOut.Add(currentAuthPolicy.Value);
            }

            var availableSchemes = (await AuthenticationSchemeProvider.GetAllSchemesAsync())
                            .Select(s => s.Name).ToList();
            schemesToSignOut = schemesToSignOut.Where(s => availableSchemes.Contains(s)).ToList();

            return new SignOutResult(schemesToSignOut);
        }

        [EnableCors(AuthConfig.AuthTemplateCorsPolicyName)]
        [EnableAbsoluteUrlRewriting]
        public IActionResult LoginTemplate()
        {
            return View();
        }

        private string TryGetProperlyFormattedNumber(string internationNumber)
        {
            if (string.IsNullOrWhiteSpace(internationNumber))
            {
                return null;
            }
            var phoneUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
            var culture = Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.UICulture;

            try
            {
                var region = culture.IsNeutralCulture ? null : new RegionInfo(culture.LCID);
                var number = phoneUtil.Parse(internationNumber, null);
                var isLocalNumber = region == null ? false : phoneUtil.IsValidNumberForRegion(number, region.TwoLetterISORegionName);
                var numberFormat = isLocalNumber
                    ? PhoneNumbers.PhoneNumberFormat.NATIONAL
                    : PhoneNumbers.PhoneNumberFormat.INTERNATIONAL;
                return phoneUtil.Format(number, numberFormat);
            }
            catch
            {
                return null;
            }
        }
    }
}
