using Microsoft.Extensions.Localization;

namespace Allup.Services
{
    public class LanguageService
    {
        private readonly IStringLocalizer _stringLocalizer;

        public LanguageService(IStringLocalizerFactory factory)
        {
            var assemblyName = typeof(LanguageService).Assembly.GetName().Name;
            _stringLocalizer = factory.Create("SharedResource", assemblyName);
        }

        public LocalizedString GetValue(string key)
        {
            return _stringLocalizer[key];
        }
    }
}
