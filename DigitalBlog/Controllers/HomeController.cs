using DigitalBlog.DataSecurity;
using DigitalBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DigitalBlog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DigitalBlogContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IDataProtector _dataProtector;

        public HomeController(ILogger<HomeController> logger, DigitalBlogContext context, IWebHostEnvironment env,IDataProtectionProvider provider,DataSecurityProvider security )
        {
            _logger = logger;
            _context = context;
            _env = env;
            _dataProtector = provider.CreateProtector(security.Datakey);
        }


        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]

        public IActionResult AddUser()
        {
            return View();
        }

        [HttpPost]
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
                
                if(edit.UserFile != null)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(edit.UserFile.FileName);
                    string filePath = Path.Combine(_env.WebRootPath, "UserProfile", filename);
                    using(FileStream stram = new FileStream(filePath, FileMode.Create))
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
                return Json(u);

            }



            catch (Exception ex)
            {

                return View(edit);


            }

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
