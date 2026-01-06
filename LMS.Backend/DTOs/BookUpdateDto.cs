namespace LMS.Backend.DTOs
{
    public class BookUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public int LibraryId { get; set; }
    }
}