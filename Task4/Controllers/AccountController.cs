using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Task4.Data;
using Task4.Models;
using Task4.ViewModels;

namespace Task4.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserContext _context;

        public AccountController(UserContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerViewModel.Email);
                if (user == null)
                {
                    _context.Users.Add(new User
                    {
                        Name = registerViewModel.Name,
                        Email = registerViewModel.Email,
                        Password = registerViewModel.Password,
                        IsBlocked = false,
                        RegisterDate = DateTime.Now,
                        LastLoginDate = DateTime.Now
                    });
                    await _context.SaveChangesAsync();

                    await Authenticate(registerViewModel.Email);

                    return RedirectToAction("Index", "Home");
                }
                else
                    ModelState.AddModelError("","Неккорректные логин и(или) пароль");
            }
            return View(registerViewModel);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginViewModel.Email
                                                                     && u.Password == loginViewModel.Password);
                if (user != null && user.IsBlocked == false)
                {
                    await Authenticate(loginViewModel.Email);
                    user.LastLoginDate = DateTime.Now;
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("","Некорректные логин и(или) пароль.");
            }
            return View(loginViewModel);
        }

        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };

            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType,ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
