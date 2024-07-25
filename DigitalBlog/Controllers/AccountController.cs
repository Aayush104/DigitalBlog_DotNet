using DigitalBlog.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DigitalBlog.Controllers
{
    public class AccountController : Controller
    {
        private readonly DigitalBlogContext _context;
        public AccountController(DigitalBlogContext context) {
            _context = context;
        
        
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

        public async Task <IActionResult> Login(UserEdit edit)
        {

            var users= await _context.Users.ToListAsync();
             
            if (users != null)
            {
                var u = users.Where(e => e.LoginName.ToUpper().Equals (edit.LoginName.ToUpper()) || e.EmailAddress.ToUpper().Equals (edit.EmailAddress.ToUpper()) 
                && e.LoginPassword.Equals(edit.LoginPassword)&& e.LoginStatus == true).FirstOrDefault();

                if (u != null)
                {
                    List<Claim> claim = new();
                    {
                        new Claim(ClaimTypes.Name, u.UserId.ToString());
                        new Claim(ClaimTypes.Email, u.EmailAddress);
                        new Claim(ClaimTypes.Role, u.UserRole);
                        new Claim("Fullname", u.FullName);

                    };

                    var claimIdentity = new ClaimsIdentity(claim, CookieAuthenticationDefaults.AuthenticationScheme);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimIdentity), new AuthenticationProperties { IsPersistent = edit.RememberMe });
					return RedirectToAction("Index");


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

            return Json (edit);

        }

    }
}
