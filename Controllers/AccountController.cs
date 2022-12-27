using Allup.DAL.Entities;
using Allup.Data;
using Allup.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Allup.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var existUser = await _userManager.FindByNameAsync(model.Username);

            if (existUser != null)
            {
                ModelState.AddModelError("", "Bele username movcuddur");
                return View();
            }

            var user = new User
            {
                Firstname = model.Firstname,
                Lastname = model.Lastname,
                Email = model.Email,
                UserName = model.Username
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View();
            }

            var createdUser = await _userManager.FindByNameAsync(model.Username);

            result = await _userManager.AddToRoleAsync(createdUser, Constants.UserRole);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View();
            }

            return RedirectToAction(nameof(Login));
        }

        public IActionResult Login(string? returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(new LoginViewModel { ReturnUrl = model.ReturnUrl});
            }

            var existUser = await _userManager.FindByNameAsync(model.Username);

            if (existUser == null)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View(new LoginViewModel { ReturnUrl = model.ReturnUrl });
            }

            var signResult = await _signInManager.PasswordSignInAsync(existUser, model.Password, model.RememberMe, false);

            if (signResult.IsLockedOut)
            {
                ModelState.AddModelError("", "locked out");
                return View(new LoginViewModel { ReturnUrl = model.ReturnUrl });
            }

            if (!signResult.Succeeded)
            {
                ModelState.AddModelError("", "Invalid credentials");
                return View(new LoginViewModel { ReturnUrl = model.ReturnUrl });
            }

            if (!string.IsNullOrEmpty(model.ReturnUrl))
            {
                if (User.Identity.IsAuthenticated)
                {
                    Console.WriteLine("kecdi");
                }
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
