using Allup.DAL;
using Allup.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.ViewComponents
{
    public class LanguageViewComponent : ViewComponent
    {
        private readonly AppDbContext _dbContext;

        public LanguageViewComponent(AppDbContext dbContext)
        {
            _dbContext = dbContext; 
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var culture = Request.Cookies[CookieRequestCultureProvider.DefaultCookieName];
            var isoCode = culture?.Substring(culture.IndexOf("uic=")+4) ?? "en-Us";

            var languages = await _dbContext.Languages.ToListAsync();
            var selectedLanguage = languages.FirstOrDefault(l=>l.IsoCode.ToLower().Equals(isoCode.ToLower()));

            var model = new LanguageViewModel
            {
                Languages = languages,
                SelectedLanguage=selectedLanguage
            };

            return View(model);
        }
    }
}
