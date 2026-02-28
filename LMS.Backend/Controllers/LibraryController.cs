using LMS.Backend.Data;
using LMS.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Backend.DTOs;
using Microsoft.AspNetCore.Authorization;

[Route("api/[controller]")]
[ApiController]
public class LibraryController : ControllerBase
{
    private readonly LMSDbContext _context;

    public LibraryController(LMSDbContext context)
    {
        _context = context;
    }

   
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Library>>> GetLibrary()
    {
        return await _context.Library
            .Include(x => x.Books)
            .ToListAsync();
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<Library>> PostLibrary([FromBody] LibraryCreateDto libraryDto)
    {
        var library = new Library
        {
            Name = libraryDto.Name,
            Location = libraryDto.Location
        };

        _context.Library.Add(library);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLibrary), new { id = library.Id }, library);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutLibrary(int id, [FromBody] LibraryCreateDto dto)
    {
        var library = await _context.Library.FindAsync(id);
        if (library == null)
            return NotFound("Güncellenecek kütüphane bulunamadı.");

        library.Name = dto.Name;
        library.Location = dto.Location;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}

//Bu dosya, kütüphaneleri listeleyen ve Admin yetkisiyle kütüphane ekleme ile güncelleme
//işlemlerini gerçekleştiren API controller’ıdır.

