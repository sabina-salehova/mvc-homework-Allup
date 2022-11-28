using Allup.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Allup.ViewComponents
{
    public class FeaturedCategoryViewComponent : ViewComponent
    {
        private readonly AppDbContext _dbContext;

        public FeaturedCategoryViewComponent(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var parentCategories = await _dbContext.Categories.Where(c => c.IsMain && !c.IsDeleted).ToListAsync();
            return View(parentCategories);
        }
    }
}
