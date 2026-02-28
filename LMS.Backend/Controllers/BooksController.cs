using System.Security.Claims;
using FluentValidation;
using LMS.Backend.Data;
using LMS.Backend.DTOs;
using LMS.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LMSDbContext _context;
        private readonly ILogger<BooksController> _logger;
        private readonly IValidator<BookUpdateDto> _updateValidator;

        public BooksController(
            LMSDbContext context,
            ILogger<BooksController> logger,
            IValidator<BookUpdateDto> updateValidator)
        {
            _context = context;
            _logger = logger;
            _updateValidator = updateValidator;
        }

        private static string? NormalizeIsbn(string? input)
        {
            var digitsOnly = new string((input ?? "").Where(char.IsDigit).ToArray());
            return string.IsNullOrWhiteSpace(digitsOnly) ? null : digitsOnly;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks()
        {
            var activeBorrows = _context.StudentBooks
                .Where(sb => sb.ReturnDate == null)
                .Select(sb => sb.BookId);

            var books = await _context.Books
                .Include(b => b.Library)
                .Select(b => new
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Isbn = b.Isbn, 
                    PageCount = b.PageCount,
                    LibraryId = b.LibraryId,
                    LibraryName = b.Library != null ? b.Library.Name : null,
                    IsBorrowed = activeBorrows.Contains(b.Id)
                })
                .ToListAsync();

            return Ok(books);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook([FromBody] BookCreateDto bookDto)
        {
            var normalizedIsbn = NormalizeIsbn(bookDto.Isbn);

           
            if (normalizedIsbn != null)
            {
                var exists = await _context.Books.AnyAsync(b => b.Isbn == normalizedIsbn);
                if (exists)
                    return Conflict("Bu ISBN zaten kayıtlı.");
            }

            var book = new Book
            {
                Title = bookDto.Title,
                Author = bookDto.Author,
                Isbn = normalizedIsbn,
                PageCount = bookDto.PageCount,
                LibraryId = bookDto.LibraryId
            };

            _context.Books.Add(book);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                
                if (normalizedIsbn != null)
                    return Conflict("Bu ISBN zaten kayıtlı.");
                throw;
            }

            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Book create. BookId: {BookId}, AdminId: {AdminId}", book.Id, adminId);

            var library = await _context.Library.FindAsync(book.LibraryId);
            var libraryName = library?.Name;

            return CreatedAtAction(nameof(GetBooks), new { id = book.Id }, new
            {
                book.Id,
                book.Title,
                book.Author,
                book.Isbn,
                book.PageCount,
                book.LibraryId,
                LibraryName = libraryName
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, [FromBody] BookUpdateDto updateDto)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return NotFound("Güncellenecek kitap bulunamadı.");

            var vr = await _updateValidator.ValidateAsync(updateDto);
            if (!vr.IsValid)
                return BadRequest(vr.Errors.Select(e => e.ErrorMessage));

            var normalizedIsbn = NormalizeIsbn(updateDto.Isbn);

            
            if (normalizedIsbn != null)
            {
                var exists = await _context.Books.AnyAsync(b => b.Isbn == normalizedIsbn && b.Id != id);
                if (exists)
                    return Conflict("Bu ISBN zaten kayıtlı.");
            }

            book.Title = updateDto.Title;
            book.Author = updateDto.Author;
            book.Isbn = normalizedIsbn;
            book.PageCount = updateDto.PageCount;

            
            if (updateDto.LibraryId.HasValue)
                book.LibraryId = updateDto.LibraryId.Value;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (normalizedIsbn != null)
                    return Conflict("Bu ISBN zaten kayıtlı.");
                throw;
            }

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
                return NotFound("Kitap bulunamadı.");

            var hasActiveBorrow = await _context.StudentBooks
                .AnyAsync(sb => sb.BookId == id && sb.ReturnDate == null);

            if (hasActiveBorrow)
                return Conflict("Bu kitap şu anda ödünç alınmış. Silinemez.");

            var hasAnyBorrowHistory = await _context.StudentBooks
                .AnyAsync(sb => sb.BookId == id);

            if (hasAnyBorrowHistory)
                return Conflict("Bu kitabın ödünç geçmişi var. Silinemez.");

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

//Bu dosya, kitapları listeleyen ve Admin yetkisiyle kitap ekleme, güncelleme ve silme
//işlemlerini gerçekleştiren API controller’ıdır; ISBN tekrarını ve ödünç durumunu kontrol eder.
