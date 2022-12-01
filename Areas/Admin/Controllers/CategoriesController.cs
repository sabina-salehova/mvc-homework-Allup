using Allup.Areas.Admin.Data;
using Allup.Areas.Admin.Models;
using Allup.DAL;
using Allup.DAL.Entities;
using Allup.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Allup.Areas.Admin.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly AppDbContext _dbContext;

        public CategoriesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _dbContext.Categories.ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _dbContext.Categories.Where(c => !c.IsDeleted && c.IsMain).ToListAsync();
            var categoryListItems = new List<SelectListItem>();

            categoryListItems.Add(new SelectListItem(" --- Select List Item --- ","0"));

            categories.ForEach(c => categoryListItems.Add(new SelectListItem(c.Name, c.Id.ToString())));

            var model = new CategoryCreateViewModel
            {
                ParentCategories = categoryListItems
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel model)
        {
            var parentCategories = await _dbContext.Categories
                .Where(c => !c.IsDeleted && c.IsMain)
                .Include(x => x.Children)
                .ToListAsync();
            var categoryListItems = new List<SelectListItem>();

            categoryListItems.Add(new SelectListItem(" --- Select List Item --- ", "0"));

            parentCategories.ForEach(c => categoryListItems.Add(new SelectListItem(c.Name, c.Id.ToString())));

            var viewModel = new CategoryCreateViewModel
            {
                ParentCategories = categoryListItems
            };

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var createCategory = new Category();

            if (model.IsMain)
            {
                if (parentCategories.Any(c => c.Name.ToLower().Equals(model.Name.ToLower())))
                {
                    ModelState.AddModelError("", "Bu adli category movcuddur");
                    return View(viewModel);
                }

                if (!model.Image.IsImage())
                {
                    ModelState.AddModelError("", "Sekil secmelisiz");
                    return View(viewModel);
                }

                if (!model.Image.IsAllowedSize(10))
                {
                    ModelState.AddModelError("", "Sekil 10mb-den cox ola bilmez");
                    return View(viewModel);
                }

                var unicalName = await model.Image.GenerateFile(Constants.CategoryPath);

                createCategory.ImageUrl = unicalName;
            }
            else 
            {
                if (model.ParentId == 0)
                {
                    ModelState.AddModelError("", "Parent category secilmelidir");
                    return View(viewModel);
                }

                var parentCategory = parentCategories
                    .FirstOrDefault(c => c.Id == model.ParentId);

                if (parentCategory.Children.Any(c => c.Name.ToLower().Equals(model.Name.ToLower())))
                {
                    ModelState.AddModelError("", "Bu adli alt category movcuddur");
                    return View(viewModel);
                }
                createCategory.ImageUrl = ""; //bazada yazmali
                createCategory.ParentId = model.ParentId;
            }

            createCategory.Name = model.Name;
            createCategory.IsMain = model.IsMain;
            createCategory.IsDeleted = false;

            await _dbContext.Categories.AddAsync(createCategory);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
