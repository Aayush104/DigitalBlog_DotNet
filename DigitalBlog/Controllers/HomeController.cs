using DigitalBlog.DataSecurity;
using DigitalBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;

namespace DigitalBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DigitalBlogContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IDataProtector _dataProtector;

        public HomeController(ILogger<HomeController> logger, DigitalBlogContext context, IWebHostEnvironment env, IDataProtectionProvider provider, DataSecurityProvider security)
        {
            _logger = logger;
            _context = context;
            _env = env;
            _dataProtector = provider.CreateProtector(security.Datakey);
        }


        [Authorize(Roles = "Admin,Editor")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]

        public IActionResult AddUser()
        {

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult AddUser(UserEdit edit)
        {

            try
            {

                //it take only 16bbit
                short maxId;

                if (_context.Users.Any())



                    //incase of using short
                    maxId = Convert.ToInt16(_context.Users.Max(e => e.UserId) + 1);


                else

                    maxId = 1;


                edit.UserId = maxId;

                if (edit.UserFile != null)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(edit.UserFile.FileName);
                    string filePath = Path.Combine(_env.WebRootPath, "UserProfile", filename);
                    using (FileStream stram = new FileStream(filePath, FileMode.Create))
                    {
                        edit.UserFile.CopyTo(stram);
                    }

                    edit.UserProfile = filename;
                }

                User u = new()
                {
                    UserId = edit.UserId,
                    UserProfile = edit.UserProfile,
                    UserRole = "User",
                    LoginName = edit.LoginName,
                    FullName = edit.FullName,
                    LoginPassword = _dataProtector.Protect(edit.LoginPassword),
                    LoginStatus = true,
                    Phone = edit.Phone,
                    EmailAddress = edit.EmailAddress,
                };

                _context.Users.Add(u);
                _context.SaveChanges();
                return RedirectToAction("Login", "Account");

            }



            catch (Exception ex)
            {

                return View(edit);


            }

        }

        [HttpGet]
        public IActionResult ProfileImage()
        {

            var user = _context.Users.Where(e => e.UserId == Convert.ToInt16(User.Identity!.Name)).FirstOrDefault();
            ViewData["img"] = user.UserProfile;



            return PartialView("_ProfileImage");

        }

        [HttpGet]
        public IActionResult Profileinfo()
        {

            var user = _context.Users.Where(e => e.UserId == Convert.ToInt16(User.Identity!.Name)).FirstOrDefault();
            ViewData["email"] = user.EmailAddress;
            ViewData["fullName"] = user.FullName;
            return PartialView("_Profileinfo");
        }

        [HttpGet]
        public IActionResult ProfileUpdate()
        {
            int userId = Convert.ToInt16(User.Identity!.Name);
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            ViewBag.psw = user.LoginPassword;

            UserEdit editModel = new UserEdit
            {
                UserId = user.UserId,
                EmailAddress = user.EmailAddress,
                UserProfile = user.UserProfile,
                UserRole = user.UserRole,
                FullName = user.FullName,
                LoginName = user.LoginName,
                LoginPassword = user.LoginPassword,
                LoginStatus = user.LoginStatus,
                Phone = user.Phone
            };

            return View(editModel);
        }



        [HttpPost]
        public IActionResult ProfileUpdate(UserEdit edit)
        {


            int userId = Convert.ToInt16(User.Identity!.Name);
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            if (edit.UserFile != null)
            {
                string oldFilePath = Path.Combine(_env.WebRootPath, "UserProfile", user.UserProfile);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }

                string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(edit.UserFile.FileName);
                string newFilePath = Path.Combine(_env.WebRootPath, "UserProfile", newFileName);
                using (FileStream stream = new FileStream(newFilePath, FileMode.Create))
                {
                    edit.UserFile.CopyTo(stream);
                }

                user.UserProfile = newFileName;
            }

            user.EmailAddress = edit.EmailAddress;
            user.UserRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)!.Value;
            user.FullName = edit.FullName;
            user.LoginName = edit.LoginName;
            user.LoginPassword = edit.LoginPassword;
            user.LoginStatus = edit.LoginStatus;
            user.Phone = edit.Phone;

            _context.Users.Update(user);
            _context.SaveChanges();
            return RedirectToAction("ProfileUpdate", "Home");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


      

    }
}
