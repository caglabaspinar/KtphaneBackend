using Microsoft.AspNetCore.Authorization;
using LMS.Backend.Data;
using LMS.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
    {
        var books = await _context.Books
            .Select(b => new Book
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Isbn = b.Isbn,
                LibraryId = b.LibraryId,
                LibraryName = b.Library != null ? b.Library.Name : null
            })
            .ToListAsync();

        return Ok(books);
    }


    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Book>> PostBook([FromBody] BookCreateDto bookDto)
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

        var library = await _context.Library.FindAsync(book.LibraryId);
        var libraryName = library?.Name;

        return CreatedAtAction(nameof(GetBooks), new { id = book.Id }, new
        {
            book.Id,
            book.Title,
            book.Author,
            book.Isbn,
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

        book.Title = updateDto.Title;
        book.Author = updateDto.Author;
        book.Isbn = updateDto.Isbn;
        book.LibraryId = updateDto.LibraryId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await _context.Books.FindAsync(id);

        if (book == null)
            return NotFound("Silinecek kitap bulunamadı.");

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();

        
        return Ok(new { message = "Kitap başarıyla silinmiştir." });
    }
}
