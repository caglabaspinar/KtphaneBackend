using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Backend.Data;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly LMSDbContext _context;

    public ReportsController(LMSDbContext context)
    {
        _context = context;
    }

    
    [HttpGet("library/{libraryId}")]
    public async Task<IActionResult> GetLibraryBooksReport(int libraryId)
    {
        var books = await _context.Books
            .Include(b => b.Library)
            .Where(b => b.LibraryId == libraryId) 
            .Select(b => new
            {
                Id = b.Id,             
                Title = b.Title,       
                Author = b.Author,     
                Isbn = b.Isbn,         
                LibraryId = b.LibraryId,
                LibraryName = b.Library != null ? b.Library.Name : "Unknown"
            })
            .ToListAsync();

        return Ok(books);
    }

    
    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentBooksReport(int studentId)
    {
        var studentBooks = await _context.StudentBooks
            .Include(sb => sb.Book)
            .ThenInclude(b => b.Library)
            .Where(sb => sb.StudentId == studentId)
            .Select(sb => new
            {
                Id = sb.Book.Id,
                Title = sb.Book.Title,
                Author = sb.Book.Author,
                Isbn = sb.Book.Isbn,
                LibraryName = sb.Book.Library != null ? sb.Book.Library.Name : "Unknown",
                BorrowDate = sb.BorrowDate
            })
            .ToListAsync();

        return Ok(studentBooks);
    }
}