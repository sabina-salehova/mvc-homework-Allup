using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Allup.Controllers
{
    public class LanguagesController : Controller
    {
        public IActionResult ChangeLangugage(string culture)
        {
            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName,
                 CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                 new CookieOptions() { Expires = DateTimeOffset.UtcNow.AddYears(1) });

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
