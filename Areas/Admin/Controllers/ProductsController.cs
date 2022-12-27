﻿using Allup.Areas.Admin.Data;
using Allup.Areas.Admin.Models;
using Allup.Areas.Admin.Services;
using Allup.DAL;
using Allup.DAL.Entities;
using Allup.Data;
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

        public async Task<IActionResult> Index()
        {
            var products = await _dbContext.Products.Include(x => x.ProductCategories).ThenInclude(x => x.Category).ToListAsync();

            return View(products);
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

            var productCategories = new List<ProductCategory>
            {
                new ProductCategory
                {
                    CategoryId = parentCategory.Id,
                    ProductId = createdProduct.Id
                } 
            }; 

            var childCategory = parentCategory?.Children.FirstOrDefault(x => x.Id == model.ChildCategoryId);

            if (childCategory is not null)
            {
                productCategories.Add(
                    new ProductCategory
                    {
                        CategoryId = childCategory.Id,
                        ProductId = createdProduct.Id
                    }
                 ); 
            }

            createdProduct.ProductCategories.AddRange(productCategories);

            await _dbContext.Products.AddAsync(createdProduct);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> LoadChildCategories(int parentCategoryId)
        {
            var parentCategory = await _dbContext.Categories.Where(x => !x.IsDeleted && x.IsMain && x.Id == parentCategoryId).Include(x => x.Children).FirstOrDefaultAsync();
            var childCategoriesSelectListItem = new List<SelectListItem>();

            parentCategory.Children.ToList().ForEach(x => childCategoriesSelectListItem.Add(new SelectListItem(x.Name, x.Id.ToString())));

            return Json(childCategoriesSelectListItem);
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id is null) return BadRequest();

            var product = await _dbContext.Products
                .Where(p => p.Id == id)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync();

            if (product is null) return NotFound();

            var parentCategory = product.ProductCategories
                .Where(c => c.Category.IsMain)
                .First();

            var childCategory = product.ProductCategories
                .Where(c => !c.Category.IsMain)
                .First();

            var parentCategories = await _dbContext.Categories
                .Where(c => !c.IsDeleted && c.IsMain)
                .Include(c=>c.Children)
                .ToListAsync();

            var parentCategoriesSelectListItem = new List<SelectListItem>();
            var childCategoriesSelectListItem = new List<SelectListItem>();

            parentCategories
                .ForEach(c => parentCategoriesSelectListItem.Add(new SelectListItem(c.Name, c.Id.ToString())));

            if (childCategory is not null)
            {
                parentCategory.Category.Children
                .ToList()
                .ForEach((c => childCategoriesSelectListItem.Add(new SelectListItem(c.Name, c.Id.ToString()))));
            }

            return View(new ProductUpdateViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Discount = product.Discount,
                Rate = product.Rate,
                ExTax = product.ExTax,
                Brand = product.Brand,
                ProductImages = product.ProductImages,
                ParentCategoryId = parentCategory.CategoryId,
                ParentCategories = parentCategoriesSelectListItem,
                ChildCategoryId = childCategory.CategoryId,
                ChildCategories = childCategoriesSelectListItem
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update( ProductUpdateViewModel model, int? id)
        {
            if (id is null) return BadRequest();

            var product = await _dbContext.Products
                .Where(p => p.Id == id)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductCategories)
                .ThenInclude(c => c.Category)
                .FirstOrDefaultAsync();

            if (product is null) return NotFound();

            return Ok(model);
        }

    }
}
