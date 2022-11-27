namespace Allup.Areas.Admin.Models
{
    public class LanguageCreateViewModel
    {
        public string Name { get; set; }
        public string IsoCode { get; set; }
        public IFormFile Image { get; set; }
    }
}
