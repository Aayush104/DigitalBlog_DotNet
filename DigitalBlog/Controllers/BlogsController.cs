using DigitalBlog.DataSecurity;
using DigitalBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection.Metadata.Ecma335;

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

                
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(edit.ImageForm.FileName);
					string filePath = Path.Combine(_env.WebRootPath, "BlogImage", fileName);

					if (edit.ImageForm != null)
                    {

						edit.BlogImage = fileName;
					}
                  
                   

                  
                

                Blog b = new()
                {
                    Bid = edit.Bid,
                    BlogImage = edit.BlogImage,
                    Bdescription = edit.Bdescription,
                    Title = edit.Title,
                    BlogPostDate = DateOnly.FromDateTime(DateTime.Today),
                    UserId =Convert.ToInt16 (User.Identity.Name), 
                    Bstatus = edit.Bstatus, 
                    Amount = edit.Amount,   

                };

                _context.Blogs.Add(b);
                _context.SaveChanges();
				if (edit.ImageForm != null)
				{

					using (FileStream stream = new FileStream(filePath, FileMode.Create))
					{
						edit.ImageForm.CopyTo(stream);
					}
				}
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

        public IActionResult GetBlogList(BlogEdit edit)
        {
            var blogs = _context.Blogs
                .Include(b => b.User)
                .Where(b => b.Bstatus == edit.Bstatus)
                .Select(e => new BlogEdit 
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

                })
                .ToList();

            return PartialView("_GetBlogList", blogs);
        }



        [HttpGet]
        public IActionResult BlogDetails(string id)
        {
            int blogid = Convert.ToInt32(_dataProtector.Unprotect(id));

            var blogs = _context.Blogs.Where(x => x.Bid == blogid).First();

            return View(blogs);
        }


        [HttpPost]
        public async Task<IActionResult> SearchBlog(Search edit)
        {
            if (edit == null || string.IsNullOrWhiteSpace(edit.Searched))
            {
                ModelState.AddModelError("", "Add search value");
                return View(new SearchBlogViewModel());
            }

            var blogs = await _context.Blogs.Where(e => e.Title.Contains(edit.Searched)).Include(b => b.User).ToListAsync();

            if (!blogs.Any())
            {
                ModelState.AddModelError("", "No result found");
                return View(new SearchBlogViewModel());
            }

            var searchedBlog = blogs.Select(e => new BlogEdit
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
      
            return View(new SearchBlogViewModel () {
                    BlogEdits = searchedBlog,
                });

            
        }



        public IActionResult Success(string q, string oid, string amt, string refId)
        {
         
            string decryptedId = _dataProtector.Unprotect(oid);
            int blogId = Convert.ToInt32(decryptedId); 


            Blog? sub = _context.Blogs.Where(x =>  x.Bid == blogId).FirstOrDefault();
            if (sub != null)
            {

                int subscriptionId = _context.BlogSubscriptions.Any() ? _context.BlogSubscriptions.Max(x => x.Subid) + 1 : 1;


                BlogSubscription newSub = new BlogSubscription
                {
                    Subid = subscriptionId,
                    SubAmount = Convert.ToDecimal(amt), 
                    UserId = Convert.ToInt16(User.Identity.Name),
                    Bid = sub.Bid
                };
                _context.BlogSubscriptions.Add(newSub); 
                _context.SaveChanges();

                string msg = "Payment Successful. Rs. " + amt;
                return View((object)msg);
            }
            return View();

        }
    

        public IActionResult Failure()
        {
            return View();
        }



    }
}
