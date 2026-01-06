using LMS.Backend.Data;
using LMS.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using LMS.Backend.DTOs;

[Route("api/[controller]")]
[ApiController]

public class BooksController : ControllerBase
{
    private readonly LMSDbContext _context;
    public BooksController(LMSDbContext context)
    {
        _context = context;
    }
    [HttpPost]
    public async Task<ActionResult<Book>> PostBook(BookCreateDto bookDto)
    {
        var book = new Book
        {
            Title = bookDto.Title,
            Author = bookDto.Author,
            Isbn = bookDto.Isbn,
            LibraryId = bookDto.LibraryId
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBooks), new { id = book.Id }, book);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
    {
        var books = await _context.Books
            .Include(b => b.Library)
            .ToListAsync();

        foreach (var book in books)
        {
            book.LibraryName = book.Library?.Name; 
        }

        return Ok(books);
    }

    
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBook(int id, BookUpdateDto updateDto)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
        {
            return NotFound("Güncellenecek kitap bulunamadı.");
        }

       
        book.Title = updateDto.Title;
        book.Author = updateDto.Author;
        book.Isbn = updateDto.Isbn;
        book.LibraryId = updateDto.LibraryId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent(); 
    }

    private bool BookExists(int id)
    {
        return _context.Books.Any(e => e.Id == id);
    }
}
