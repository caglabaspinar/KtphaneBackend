using System.Collections.Generic;

namespace LMS.Backend.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        
        public string Role { get; set; } = "Student";

        public ICollection<StudentBook> StudentBooks { get; set; } = new List<StudentBook>();
    }
}
