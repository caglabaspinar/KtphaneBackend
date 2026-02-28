namespace LMS.Backend.DTOs
{
    public class BookCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Isbn { get; set; } 
        public int? PageCount { get; set; }

        public int LibraryId { get; set; }
    }
}