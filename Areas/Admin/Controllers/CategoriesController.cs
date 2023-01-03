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

            categoryListItems.Add(new SelectListItem(" --- Select List Item --- ", "0"));

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
                    ModelState.AddModelError("", "Bu adli parent category movcuddur");
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

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null) return NotFound();

            var existCategory = await _dbContext.Categories.Where(c => !c.IsDeleted && c.Id == id).FirstOrDefaultAsync();

            if (existCategory is null) return NotFound();

            var updateCategory = new CategoryUpdateViewModel
            {
                Id = existCategory.Id,
                Name = existCategory.Name,
                ParentId = existCategory.ParentId,
                IsMain = existCategory.IsMain,
                ImageUrl = existCategory.ImageUrl
            };

            var categories = await _dbContext.Categories.Where(c => !c.IsDeleted && c.IsMain && c.Id!=id).ToListAsync();
            var categoriesSelectListItem = new List<SelectListItem>();
            categories.ForEach(x => categoriesSelectListItem.Add(new SelectListItem(x.Name, x.Id.ToString())));
            updateCategory.ParentCategories = categoriesSelectListItem;

            return View(updateCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(CategoryUpdateViewModel model)
        {
            if (model.Id is null)
                return NotFound();

            var existCategory = await _dbContext.Categories.FindAsync(model.Id);

            if (existCategory is null)
                return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }            

            var updateImage = existCategory.ImageUrl;

            var parentCategories = await _dbContext.Categories
               .Where(c => !c.IsDeleted && c.IsMain)
               .Include(x => x.Children)
               .ToListAsync();

            if (parentCategories.Count == 0)
            {
                ModelState.AddModelError("", "Parent kateqoriya movcud deyil!");
                return View(model);
            }

            if (model.IsMain)
            {
                if (parentCategories.Any(c => c.Id!=model.Id && c.Name.ToLower().Equals(model.Name.ToLower())))
                {
                    ModelState.AddModelError("", "Bu adli parent category movcuddur");
                    return View(model);
                }

                if (updateImage is null)
                {
                    if (!model.Image.IsImage())
                    {
                        ModelState.AddModelError("", "Sekil secmelisiz");
                        return View(model);
                    }
                }                

                existCategory.ParentId = null;
            }
            else
            {
                var existParentCategories = await _dbContext.Categories
                    .Where(c => !c.IsDeleted && c.IsMain && c.Id != model.Id)
                    .ToListAsync();

                if (existParentCategories.Count == 0)
                {
                    ModelState.AddModelError("", "hazirda secmek ucun parent kateqoriya yoxdur, evvelce parent elave edin!!!");
                    return View(model);
                }

                if (existCategory.ParentId==0)
                {
                    if (model.ParentId == 0)
                    {
                        ModelState.AddModelError("", "Parent category secilmelidir");
                        return View(model);
                    }                    
                }

                if (model.ParentId != 0)
                {
                    var childCategories = await _dbContext.Categories
                    .Where(c => !c.IsDeleted && !c.IsMain && c.ParentId==model.ParentId && c.Id!=existCategory.Id)
                    .ToListAsync();

                    if (childCategories.Any(c => c.Name.ToLower().Equals(model.Name.ToLower())))
                    {
                        ModelState.AddModelError("", "Bu adli alt category movcuddur");
                        return View(model);
                    }

                    existCategory.ParentId = model.ParentId;
                }

                existCategory.ImageUrl = "";

                var usedChildCategories = await _dbContext.Categories
                    .Where(c => !c.IsDeleted && !c.IsMain && c.ParentId == model.Id)
                    .ToListAsync();

                if (usedChildCategories.Count>0)
                {
                    ModelState.AddModelError("", "Hazirda category parent olaraq istifade edilir, evvelce alt kateqoriyalar silinmelidir !!!");
                    return View(model);
                }
            }

            if (model.Image is not null)
            {
                var newImage = model.Image;

                if (!newImage.IsImage())
                {
                    ModelState.AddModelError("Image", "Shekil formati secilmelidir");

                    return View(model);
                }
                int imageMbCount = 10;
                if (!newImage.IsAllowedSize(imageMbCount))
                {
                    ModelState.AddModelError("Image", $"Shekilin olcusu {imageMbCount} mb-dan boyuk ola bilmez");

                    return View(model);
                }

                var path = Path.Combine(Constants.CategoryPath, existCategory.ImageUrl);

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);

                var unicalFileName = await model.Image.GenerateFile(Constants.CategoryPath);

                updateImage = unicalFileName;
                existCategory.ImageUrl = updateImage;
            }

            existCategory.Name = model.Name;
            existCategory.IsMain = model.IsMain;

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null) return NotFound();

            var existCategory = await _dbContext.Categories.FindAsync(id);

            if (existCategory is null) return NotFound();

            if (existCategory.IsMain)
            {
                var children = await _dbContext.Categories.Where(c => !c.IsMain && c.ParentId == id).ToListAsync();
                children.ForEach(c => _dbContext.Categories.Remove(c));
            }

            var path = Path.Combine(Constants.CategoryPath, existCategory.ImageUrl);

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            _dbContext.Categories.Remove(existCategory);

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
