using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Backend.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty; 

        public string Author { get; set; } = string.Empty; 

        public string Isbn { get; set; } = string.Empty; 

        public int LibraryId { get; set; } 

        [NotMapped]
        public string? LibraryName { get; set; } 

        public Library? Library { get; set; }
    }
}