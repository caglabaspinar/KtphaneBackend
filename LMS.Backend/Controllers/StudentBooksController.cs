using Microsoft.Extensions.Logging;
using LMS.Backend.Data;
using LMS.Backend.DTOs;
using LMS.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentBooksController : ControllerBase
    {
        private readonly LMSDbContext _context;
        private readonly ILogger<StudentBooksController> _logger;


        public StudentBooksController(LMSDbContext context, ILogger<StudentBooksController> logger)
        {
            _context = context;
            _logger = logger;
        }



        private int GetStudentIdFromToken()
        {
            var idStr =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(idStr))
                throw new UnauthorizedAccessException("Token içinde kullanıcı id bulunamadı.");

            return int.Parse(idStr);
        }

        
        [HttpPost("borrow")]
        public async Task<IActionResult> BorrowBook([FromBody] StudentBookRequestDto request)
        {
            var studentId = GetStudentIdFromToken();

            var book = await _context.Books.FindAsync(request.BookId);
            if (book == null) return NotFound("Kitap bulunamadı.");

            var alreadyBorrowed = await _context.StudentBooks.AnyAsync(sb =>
                sb.StudentId == studentId &&
                sb.BookId == request.BookId &&
                sb.ReturnDate == null);

            if (alreadyBorrowed)
                return Conflict("Bu kitabı zaten ödünç aldın (teslim edilmedi).");

            var studentBook = new StudentBook
            {
                StudentId = studentId,
                BookId = request.BookId,
                BorrowDate = DateTime.Now,
                ReturnDate = null
            };

            _context.StudentBooks.Add(studentBook);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Borrow. StudentId: {StudentId}, BookId: {BookId}", studentId, request.BookId);


            return Ok(new
            {
                message = "Kitap başarıyla alındı",
                borrowId = studentBook.Id,
                bookId = studentBook.BookId,
                title = book.Title,
                author = book.Author,
                borrowDate = studentBook.BorrowDate
            });
        }

        
        [HttpPost("return")]
        public async Task<IActionResult> ReturnBook([FromBody] StudentBookRequestDto request)
        {
            var studentId = GetStudentIdFromToken();

            var record = await _context.StudentBooks
                .Include(sb => sb.Book)
                .FirstOrDefaultAsync(sb =>
                    sb.StudentId == studentId &&
                    sb.BookId == request.BookId &&
                    sb.ReturnDate == null);

            if (record == null)
                return NotFound("Aktif ödünç kaydı bulunamadı.");

            record.ReturnDate = DateTime.Now;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Return. StudentId: {StudentId}, BookId: {BookId}", studentId, request.BookId);


            return Ok(new
            {
                message = "Kitap teslim edildi",
                borrowId = record.Id,
                bookId = record.BookId,
                title = record.Book.Title,
                author = record.Book.Author,
                borrowDate = record.BorrowDate,
                returnDate = record.ReturnDate
            });
        }

       
        [HttpGet("my")]
        public async Task<IActionResult> MyActiveBorrows()
        {
            var studentId = GetStudentIdFromToken();

            var list = await _context.StudentBooks
                .Where(sb => sb.StudentId == studentId && sb.ReturnDate == null)
                .Include(sb => sb.Book)
                .OrderByDescending(sb => sb.BorrowDate)
                .Select(sb => new
                {
                    borrowId = sb.Id,
                    bookId = sb.BookId,
                    title = sb.Book.Title,
                    author = sb.Book.Author,
                    borrowDate = sb.BorrowDate,
                    returnDate = sb.ReturnDate
                })
                .ToListAsync();

            return Ok(list);
        }

       
        [HttpGet("my/history")]
        public async Task<IActionResult> MyBorrowHistory()
        {
            var studentId = GetStudentIdFromToken();

            var list = await _context.StudentBooks
                .Where(sb => sb.StudentId == studentId)
                .Include(sb => sb.Book)
                .OrderByDescending(sb => sb.BorrowDate)
                .Select(sb => new
                {
                    borrowId = sb.Id,
                    bookId = sb.BookId,
                    title = sb.Book.Title,
                    author = sb.Book.Author,
                    borrowDate = sb.BorrowDate,
                    returnDate = sb.ReturnDate
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}

//Bu dosya, giriş yapan öğrencinin token’ından kendi kimliğini alarak kitap ödünç
//alma/teslim etme işlemlerini yapar ve öğrencinin aktif ödünçlerini ile ödünç geçmişini
//listeleyen API controller’ıdır.

