using LMS.Backend.Data;
using LMS.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Backend.DTOs; 

[Route("api/[controller]")]
[ApiController]
public class LibraryController : ControllerBase
{
    private readonly LMSDbContext _context;
    public LibraryController(LMSDbContext context)
    {
        _context = context;
    }
    [HttpPost]
    public async Task<ActionResult<Library>> PostLibrary(LibraryCreateDto libraryDto) 
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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Library>>> GetLibrary()
    {
        
        return await _context.Library
                             .Include(x => x.Books)
                             .ToListAsync();
    }
   
}