using System.ComponentModel.DataAnnotations;

namespace DigitalBlog.Models
{
    public class BlogEdit
    {
        public long Bid { get; set; }

        public string Title { get; set; } = null!;

        public string? Bdescription { get; set; }

        public string? BlogImage { get; set; }

        public DateOnly BlogPostDate { get; set; }

   
        public string Bstatus { get; set; } = null!;

        public decimal Amount { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? ImageForm { get; set; }   

        public string? EncId = string.Empty; 
        public string? PublishedBy { get; set; }
        

    }
}
