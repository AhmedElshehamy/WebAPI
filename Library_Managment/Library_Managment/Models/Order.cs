using System.ComponentModel.DataAnnotations.Schema;

namespace Library_Managment.Models
{
    public class Order
    {
        public int id { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [ForeignKey("Book")]
        public int bookId { get; set; }
        public Book? Book { get; set; }

        public DateTime orderDate { get; set; }
        public bool returned { get; set; }
        public DateTime? returnedDate { get; set; }
        public int finePaid { get; set; }
    }
}
