using Microsoft.AspNetCore.Mvc.Rendering;

namespace Allup.Areas.Admin.Models
{
    public class CategoryUpdateViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public bool IsMain { get; set; }
        public IFormFile? Image { get; set; }
        public string ImageUrl { get; set; } = String.Empty;
        public int? ParentId { get; set; }
        public List<SelectListItem> ParentCategories { get; set; } = new();
    }
}
