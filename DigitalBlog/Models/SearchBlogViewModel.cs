using System.ComponentModel.DataAnnotations;

namespace DigitalBlog.Models
{
    public class SearchBlogViewModel
    {
        public List<BlogEdit> BlogEdits { get; set; } = [];
    }
}
