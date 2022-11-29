using Allup.Areas.Admin.Models;
using Allup.DAL;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Admin.Services
{
    public class CategoryService
    {
        private readonly AppDbContext _dbContext;

        public CategoryService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<ProductCreateViewModel> GetCategories()
        {
            var categories = await _dbContext
               .Categories
               .Where(x => !x.IsDeleted && x.IsMain)
               .Include(x => x.Children).ToListAsync();

            var parentCategoriesSelectListItem = new List<SelectListItem>();
            var childCategoriesSelectListItem = new List<SelectListItem>();

            categories.ForEach(x => parentCategoriesSelectListItem.Add(new SelectListItem(x.Name, x.Id.ToString())));
            categories[0].Children.ToList().ForEach(x => childCategoriesSelectListItem.Add(new SelectListItem(x.Name, x.Id.ToString())));

            var model = new ProductCreateViewModel
            {
                ParentCategories = parentCategoriesSelectListItem,
                ChildCategories = childCategoriesSelectListItem
            };

            return model;
        }
    }
}
