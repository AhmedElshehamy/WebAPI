using System.ComponentModel.DataAnnotations.Schema;

namespace Library_Managment.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public float Price { get; set; }
        public bool Ordered { get; set; }
        [ForeignKey("BookCategory")]
        public int BookCategoryId { get; set; }
        public BookCategory? BookCategory { get; set; }
    }
}
