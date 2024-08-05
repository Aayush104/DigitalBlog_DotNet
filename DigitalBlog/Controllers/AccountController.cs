using DigitalBlog.DataSecurity;
using DigitalBlog.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Net.Mail;
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
				return RedirectToAction("AdminDash","Home"); 
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
		[HttpGet]
		[Authorize]

		public IActionResult ChangePassword()
		{
			return View();
		}

		[HttpPost]
		[Authorize]

		public IActionResult ChangePassword(ChangePsw psw)
		{
			try
			{

				var user = _context.Users.Where(e => e.UserId == Convert.ToInt16(User.Identity.Name)).FirstOrDefault();
				if (user != null)
				{

					if (_dataProtector.Unprotect(user.LoginPassword) == psw.CurrentPassword)
					{
						if (psw.NewPassword == psw.ConfirmPassword)
						{

							user.LoginPassword = _dataProtector.Protect(psw.NewPassword);
							_context.Update(user);
							_context.SaveChanges();
							return Content("Success");


						}
						else
						{
							ModelState.AddModelError("", "Confirm Password Doesnot match");
							return View(psw);
						}
					}
					else
					{
						ModelState.AddModelError("", "Please Recheck yOUR cURRENT pASSWORD");
						return View(psw);

					}
				}
				else
				{
					return View();
				}
			}
			catch (Exception ex)
			{
				return View(ex);
			}


		}


		[HttpGet]
		[AllowAnonymous]
		public IActionResult ForgetPassword()
		{

			return View();

		}


		[HttpPost]

		public IActionResult ForgetPassword(UserEdit edit)
		{
			try
			{
				var user = _context.Users.Where(e => e.EmailAddress == edit.EmailAddress).FirstOrDefault();

				if (user == null)
				{	

					ModelState.AddModelError("", "The User DoesNOt exist");
					return View(edit);
				}


				//otp gemeration
				Random r = new Random();

				HttpContext.Session.SetString("token", r.Next(9999).ToString());	
				var token = HttpContext.Session.GetString("token");
				SmtpClient smtpclient = new()
				{
					Host = "smtp.gmail.com",
					Port = 587,
					UseDefaultCredentials = false,
					Credentials = new NetworkCredential("aayushadhikari601@gmail.com", "hzrn ggvy swkq xfax"),
					EnableSsl = true,
					DeliveryMethod = SmtpDeliveryMethod.Network

                };

				MailMessage m = new()
				{
					From = new MailAddress("aayushadhikari601@gmail.com"),
					
					Subject = "FORGET PASSWORD",
                    Body = $"<a style='background-color:Green; color:white; padding:5px;' href='https://localhost:7043/Account/VerifyOtp?email={user.EmailAddress}'>ResetPassword</a> " +
                    $"<p>token number:{token}</p>",
                    IsBodyHtml = true,

				};

				m.To.Add(user.EmailAddress);
				smtpclient.Send(m);
                return RedirectToAction("VerifyOtp", new { email = user.EmailAddress });




            }
            catch (Exception ex)
			{
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");
                return View(edit);
            }
		}


		[AllowAnonymous]
		[HttpGet]
		public IActionResult VerifyOtp(string email)

		{

			ViewBag.Email = email;
			return View();
		}
        [HttpPost]
        public IActionResult VerifyOtp(UserEdit edit)
        {
            try
            {
               
				
                var token = HttpContext.Session.GetString("token");

                if (string.IsNullOrEmpty(token))
                {
                    ModelState.AddModelError("", "No token provided or token expired.");
                    return View(edit);
                }

                if (token != edit.Token)
                {
                    ModelState.AddModelError("", "Please enter the correct OTP.");
                    return View(edit);
                }

				
                return RedirectToAction("NewPassword", new { email = edit.EmailAddress });
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                return View(edit);
            }
        }



        [HttpGet]

		public IActionResult NewPassword(string email)
		{

			ViewBag.Email = email;
			return View();	
		}


        [HttpPost]
        public IActionResult NewPassword(NewPassword psw)
        {
            // Validate the input
            if ( string.IsNullOrEmpty(psw.NewPasswords) || string.IsNullOrEmpty(psw.ConfirmPassword))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View(psw); 
            }

            if (psw.NewPasswords != psw.ConfirmPassword)
            {
                ModelState.AddModelError("", "New password and confirmation do not match.");
                return View(psw); 
            }

            // Find the user by email
            var user = _context.Users.FirstOrDefault(e => e.EmailAddress == psw.email);

            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return View(psw);
            }

            // Update the user's password
            user.LoginPassword = _dataProtector.Protect(psw.NewPasswords);
            _context.Update(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

			

    }


	


}
