using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Task4.Data;
using Task4.Models;
using Task4.ViewModels;

namespace Task4.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserContext _context;

        public HomeController(UserContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = _context.Users.OrderBy(c => c.Id);

            foreach (var user in users)
            {
                if (User.Identity.Name == user.Email && user.IsBlocked)
                {
                    RedirectToAction("Logout", "Account");
                    await Response.WriteAsync("<script>alert('Your account has been blocked or deleted')</script>");
                }
                    
            }

            return View(users);
        }

        private async Task<IActionResult> ChangeUsers(int[] itemsId, Func<User, Task> handler)
        {
            if (itemsId == null) return RedirectToAction("Index", "Home");
            var userLogout = false;
            foreach (var itemId in itemsId)
            {
                var item = await _context.Users.FirstOrDefaultAsync(c => c.Id == itemId);
                if (item == null) continue;
                {
                    await handler(item);
                    var changedUser = await _context.Users.FirstOrDefaultAsync(c => c.Id == item.Id);
                    if (User.Identity.Name == item.Email && (item.IsBlocked || changedUser == null))
                        userLogout = true;
                }
            }

            if (userLogout)
                return RedirectToAction("Logout", "Account");

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Block(int[] itemsId)
        {
            return await ChangeUsers(itemsId, async user =>
            {
                user.IsBlocked = true;
                await _context.SaveChangesAsync();
            });
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(int[] itemsId)
        {
            return await ChangeUsers(itemsId, async user =>
            {
                user.IsBlocked = false;
                await _context.SaveChangesAsync();
            });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int[] itemsId)
        {
            return await ChangeUsers(itemsId, async user =>
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            });
        }
    }
}
