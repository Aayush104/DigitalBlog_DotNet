using DigitalBlog.DataSecurity;
using DigitalBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DigitalBlog.Controllers
{
    public class BlogsController : Controller
    {
        private readonly DigitalBlogContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IDataProtector _dataProtector;
        public BlogsController(DigitalBlogContext context, IWebHostEnvironment env, IDataProtectionProvider provider, DataSecurityProvider security)
        {
            _context = context;
            _env = env;
            _dataProtector = provider.CreateProtector(security.Datakey);
        }

        [Authorize(Roles = "Admin,Editor")]
        public IActionResult AddBlog()
        {
            return View();
        }



        [HttpPost]
        [Authorize(Roles = "Admin,Editor")]

	   public IActionResult AddBlog(BlogEdit edit)
        {

            try
            {
                int Bid;

                if (_context.Blogs.Any())
                {
                    Bid = Convert.ToInt32(_context.Blogs.Max(e => e.Bid) + 1);

                }
                else
                {
                    Bid = 1;
                }

               edit.Bid = Bid;  

                if(edit.ImageForm != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(edit.ImageForm.FileName);
                    string filePath = Path.Combine(_env.WebRootPath, "BlogImage", fileName);
                    using (FileStream stream = new FileStream(filePath, FileMode.Create))
                    {
                            edit.ImageForm.CopyTo(stream);  
                    }

                    edit.BlogImage = fileName;

                }

                Blog b = new()
                {
                    Bid = edit.Bid,
                    BlogImage = edit.BlogImage,
                    Bdescription = edit.Bdescription,
                    Title = edit.Title,
                    BlogPostDate = DateOnly.MaxValue,
                    UserId =Convert.ToInt16 (User.Identity.Name), 
                    Bstatus = edit.Bstatus, 
                    Amount = edit.Amount,   

                };

                _context.Blogs.Add(b);
                _context.SaveChanges();
				return RedirectToAction("AddBlog", "Blogs");


			}
			catch(Exception ex){


                ModelState.AddModelError("", "An error occurred while processing your request. Please try again later.");

                return View(edit);

            }
        }


        [HttpGet]
        public IActionResult GetBlog()
        {

            return View();  
        }

		public IActionResult GetBlogList()
		{

            var user = _context.Blogs.Include(b=>b.User).ToList();
			var blogs = user.Select(e => new BlogEdit
			{
				Bid = e.Bid,
				Bdescription = e.Bdescription,
				BlogImage = e.BlogImage,
				Title = e.Title,
				BlogPostDate = e.BlogPostDate,
				Bstatus = e.Bstatus,
				Amount = e.Amount,
                EncId = _dataProtector.Protect(e.Bid.ToString()),
                PublishedBy = e.User.FullName

			}).ToList();

			
			return PartialView("_GetBlogList", blogs);
		}


        [HttpGet]
        public IActionResult BlogDetails(string id)
        {
            int blogid = Convert.ToInt32(_dataProtector.Unprotect(id));

            var blogs = _context.Blogs.Where(x => x.Bid == blogid).First();

            return View(blogs);
        }

	}
}
