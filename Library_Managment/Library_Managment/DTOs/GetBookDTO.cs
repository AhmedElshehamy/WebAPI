namespace Library_Managment.DTOs
{
    public class GetBookDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public float Price { get; set; }
        public bool ordered { get; set; }
        public int bookCategoryId { get; set; }
    }
}
