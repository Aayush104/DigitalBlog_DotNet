﻿using DigitalBlog.DataSecurity;
using DigitalBlog.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace DigitalBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly DigitalBlogContext _context;
		private readonly IDataProtector _dataProtector;
		public AccountController(DigitalBlogContext context, DataSecurityProvider security, IDataProtectionProvider provider) {
            _context = context;
			_dataProtector = provider.CreateProtector(security.Datakey);


		}


        [HttpGet]
        public IActionResult Login()
        {

            if(User.Identity!.IsAuthenticated)
            {
				return RedirectToAction("Index","Home");

			}

            return View();
           
        }


		[HttpPost]
		public async Task<IActionResult> Login(UserEdit edit)
		{
			// Fetch the user directly from the database based on the login name
			var u = await _context.Users.ToListAsync();

			if (u != null)
			{
				// Compare the passwords
				var user = u.Where(e => e.LoginName.ToUpper().Equals(edit.LoginName.ToUpper()) && _dataProtector.Unprotect(e.LoginPassword).Equals(edit.LoginPassword)
				   && e.LoginStatus == true).FirstOrDefault();

				if (user != null)
				{
					List<Claim> claim = new()
			{
				new Claim(ClaimTypes.Name, user.UserId.ToString()),
				new Claim("Email", user.EmailAddress),
				new Claim(ClaimTypes.Role, user.UserRole),
				new Claim("Fullname", user.FullName)
			};

					var claimsIdentity = new ClaimsIdentity(claim, CookieAuthenticationDefaults.AuthenticationScheme);
					await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties { IsPersistent = edit.RememberMe });

					return RedirectToAction("Dashboard");
				}
				else
				{
					ModelState.AddModelError("", "Invalid UserName Or Password");
					return View(edit);
				}
			}
			else
			{
				ModelState.AddModelError("", "This User Doesn't Exist");
				return View(edit);
			}
		}



		[Authorize]
		public IActionResult Dashboard()
		{
			if (User.IsInRole("Admin"))
			{
				return RedirectToAction("Index"); 
			}
			else if (User.IsInRole("Editor"))
			{
				return RedirectToAction("Index"); 
			}
			else
			{
				return RedirectToAction("Privacy","Home");
			}
		}

		[Authorize]

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Index", "Static");
		}

	}
}
