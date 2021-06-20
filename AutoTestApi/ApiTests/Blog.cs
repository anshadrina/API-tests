using System;

namespace ApiTests
{
    public class Blog
    {
        public Guid? Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Author { get; set; }
    }
}