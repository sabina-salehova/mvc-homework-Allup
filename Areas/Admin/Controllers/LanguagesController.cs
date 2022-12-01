using Allup.Areas.Admin.Data;
using Allup.Areas.Admin.Models;
using Allup.DAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Constants = Allup.Data.Constants;

namespace Allup.Areas.Admin.Controllers
{
    public class LanguagesController : BaseController
    {
        private readonly AppDbContext _dbContext;

        public LanguagesController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var languages = await _dbContext.Languages.Where(l => !l.IsDeleted).ToListAsync();
            return View(languages);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LanguageCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (!model.Image.IsImage())
            {
                ModelState.AddModelError("", "Sekil secmelisiz");
                return View();
            }

            if (!model.Image.IsAllowedSize(10))
            {
                ModelState.AddModelError("", "Sekil 10mb-den cox ola bilmez");
                return View();
            }

            var unicalName = await model.Image.GenerateFile(Constants.FlagPath);

            await _dbContext.Languages.AddAsync(new DAL.Entities.Language { ImageUrl = unicalName, Name = model.Name, IsoCode = model.IsoCode });
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
