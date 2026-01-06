using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Backend.Data;
using LMS.Backend.Models;
using LMS.Backend.DTOs;
using System.Security.Claims;

namespace LMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentBooksController : ControllerBase
    {
        private readonly LMSDbContext _context;

        public StudentBooksController(LMSDbContext context)
        {
            _context = context;
        }

        
        [HttpPost]
        public async Task<IActionResult> BorrowBook([FromBody] StudentBookRequestDto request)
        {
            var book = await _context.Books.FindAsync(request.BookId);
            if (book == null) return NotFound("Kitap bulunamadı.");

            var alreadyBorrowed = await _context.StudentBooks
                .AnyAsync(sb => sb.StudentId == request.StudentId && sb.BookId == request.BookId);

            if (alreadyBorrowed)
            {
                return Conflict("Bu kitap daha önce ödünç alındı.");
            }

            var studentBook = new StudentBook
            {
                StudentId = request.StudentId,
                BookId = request.BookId,
                BorrowDate = DateTime.Now
            };

            _context.StudentBooks.Add(studentBook);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Kitap başarıyla alındı",
                Title = book.Title,
                Author = book.Author
            });
        }


    }
}