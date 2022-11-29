using Allup.Areas.Admin.Data;
using Allup.Areas.Admin.Models;
using Allup.Areas.Admin.Services;
using Allup.DAL;
using Allup.DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace Allup.Areas.Admin.Controllers
{
    public class ProductsController : BaseController
    {
        private readonly AppDbContext _dbContext;
        private readonly CategoryService _categoryService;
        public ProductsController(AppDbContext dbContext, CategoryService categoryService)
        {
            _dbContext = dbContext;
           _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            return View(new List<Product>());
        }

        public async Task<IActionResult> Create()
        {
            var model = await _categoryService.GetCategories();

            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            var viewModel = await _categoryService.GetCategories();

            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var createdProduct = new Product
            {
                Name = model.Name,
                Brand = model.Brand,
                ExTax = model.ExTax,
                Price = model.Price,
                Description = model.Description,
                Discount = model.Discount,
                Rate = model.Rate,
                ProductCategories = new List<ProductCategory>(),
                ProductImages = new List<ProductImage>()
            };

            var productImages = new List<ProductImage>();

            foreach (var image in model.Images)
            {
                if (!image.IsImage())
                {
                    ModelState.AddModelError("", "Sekil secmelisiz");
                    return View(viewModel);
                }

                if (!image.IsAllowedSize(10))
                {
                    ModelState.AddModelError("", "Sekil 10mb-den cox ola bilmez");
                    return View(viewModel);
                }

                var unicalName = await image.GenerateFile(Constants.ProductPath);
                productImages.Add(new ProductImage
                {
                    Name = unicalName,
                    ProductId = createdProduct.Id
                });
            }

            createdProduct.ProductImages.AddRange(productImages);

            var parentCategory = await _dbContext
              .Categories
              .Where(x => !x.IsDeleted && x.IsMain && x.Id == model.ParentCategoryId)
              .Include(x => x.Children).FirstOrDefaultAsync();

            var childCategory = parentCategory?.Children.FirstOrDefault(x => x.Id == model.ChildCategoryId);

            var productCategories = new List<ProductCategory>
            {
                new ProductCategory
                {
                    CategoryId = parentCategory.Id,
                    ProductId = createdProduct.Id
                },

                new ProductCategory
                {
                    CategoryId = childCategory.Id,
                    ProductId = createdProduct.Id
                }
            };

            createdProduct.ProductCategories.AddRange(productCategories);

            await _dbContext.Products.AddAsync(createdProduct);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
