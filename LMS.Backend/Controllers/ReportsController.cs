using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Backend.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly LMSDbContext _context;
    private readonly ILogger<ReportsController> _logger;


    public ReportsController(LMSDbContext context, ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

   
    private int GetUserIdFromToken()
    {
        var idStr =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(idStr))
            throw new UnauthorizedAccessException("Token içinde kullanıcı id bulunamadı.");

        return int.Parse(idStr);
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
                PageCount = b.PageCount, 
                LibraryId = b.LibraryId,
                LibraryName = b.Library != null ? b.Library.Name : "Unknown"
            })
            .ToListAsync();

        return Ok(books);
    }



    [Authorize]
    [HttpGet("student/{studentId}")]
    public async Task<IActionResult> GetStudentBooksReport(int studentId)
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        var currentUserId = GetUserIdFromToken();

        var isAdmin = role == "Admin";
        var isSelf = currentUserId == studentId;

        if (!isAdmin && !isSelf)
        {
            _logger.LogWarning("Yetkisiz rapor denemesi. CurrentUserId: {CurrentUserId}, TargetStudentId: {TargetStudentId}, Role: {Role}",
                currentUserId, studentId, role);
            return Forbid("Başka öğrencinin raporunu görüntüleyemezsin.");
        }


        var studentBooks = await _context.StudentBooks
            .Include(sb => sb.Book)
            .ThenInclude(b => b.Library)
            .Where(sb => sb.StudentId == studentId)
            .OrderByDescending(sb => sb.BorrowDate)
            .Select(sb => new
            {
                BorrowId = sb.Id,
                StudentId = sb.StudentId,
                BookId = sb.Book.Id,
                Title = sb.Book.Title,
                Author = sb.Book.Author,
                Isbn = sb.Book.Isbn,
                LibraryName = sb.Book.Library != null ? sb.Book.Library.Name : "Unknown",
                BorrowDate = sb.BorrowDate,
                ReturnDate = sb.ReturnDate
            })
            .ToListAsync();

        return Ok(studentBooks);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/borrows")]

    public async Task<IActionResult> GetAllBorrows()
    {

        var adminId = GetUserIdFromToken();
        _logger.LogInformation("Admin borrows raporu görüntülendi. AdminId: {AdminId}", adminId);

        var list = await _context.StudentBooks
            .Include(sb => sb.Student)
            .Include(sb => sb.Book)
            .ThenInclude(b => b.Library)
            .OrderByDescending(sb => sb.BorrowDate)
            .Select(sb => new
            {
                BorrowId = sb.Id,

                StudentId = sb.StudentId,
                StudentName = sb.Student != null ? sb.Student.FullName : "Unknown",
                StudentEmail = sb.Student != null ? sb.Student.Email : "Unknown",

                BookId = sb.BookId,
                BookTitle = sb.Book != null ? sb.Book.Title : "Unknown",
                BookAuthor = sb.Book != null ? sb.Book.Author : "Unknown",

                LibraryId = sb.Book != null ? sb.Book.LibraryId : 0,
                LibraryName = sb.Book != null && sb.Book.Library != null ? sb.Book.Library.Name : "Unknown",

                BorrowDate = sb.BorrowDate,
                ReturnDate = sb.ReturnDate
            })
            .ToListAsync();

        return Ok(list);
    }

    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin/borrows/student/{studentId}")]
    public async Task<IActionResult> GetBorrowsByStudent(int studentId)
    {
        var list = await _context.StudentBooks
            .Include(sb => sb.Student)
            .Include(sb => sb.Book)
            .ThenInclude(b => b.Library)
            .Where(sb => sb.StudentId == studentId)
            .OrderByDescending(sb => sb.BorrowDate)
            .Select(sb => new
            {
                BorrowId = sb.Id,

                StudentId = sb.StudentId,
                StudentName = sb.Student != null ? sb.Student.FullName : "Unknown",
                StudentEmail = sb.Student != null ? sb.Student.Email : "Unknown",

                BookId = sb.BookId,
                BookTitle = sb.Book != null ? sb.Book.Title : "Unknown",
                BookAuthor = sb.Book != null ? sb.Book.Author : "Unknown",

                LibraryId = sb.Book != null ? sb.Book.LibraryId : 0,
                LibraryName = sb.Book != null && sb.Book.Library != null ? sb.Book.Library.Name : "Unknown",

                BorrowDate = sb.BorrowDate,
                ReturnDate = sb.ReturnDate
            })
            .ToListAsync();

        return Ok(list);
    }
}
//Bu dosya, kütüphane, öğrenci ve ödünç işlemlerine ait raporları üretir;
//öğrencinin kendi raporunu veya Admin’in tüm ödünç geçmişini yetki kontrolü yaparak
//listeleyen API controller’ıdır.

