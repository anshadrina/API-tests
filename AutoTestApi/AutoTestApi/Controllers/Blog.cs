using System;
using System.ComponentModel.DataAnnotations;

namespace AutoTestApi.Controllers
{
    public class Blog
    {
        public Guid? Id { get; set; }
        [Required]
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Author { get; set; }
    }
}
