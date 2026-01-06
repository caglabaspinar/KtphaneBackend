using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LMS.Backend.Data;   
using LMS.Backend.Models;
using LMS.Backend.DTOs;   

namespace LMS.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly LMSDbContext _context;

        public StudentsController(LMSDbContext context)
        {
            _context = context;
        }

        
        [HttpPost]
        public async Task<ActionResult<Student>> Register(StudentRegisterDto request)
        {
            
            var student = new Student
            {
                FullName = request.FullName,
                Email = request.Email,
                Password = request.Password 
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

           
            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
        }

       
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] StudentLoginDto loginDto)
        {
           
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == loginDto.Email && s.Password == loginDto.Password);

            
            if (student == null)
            {
                return Unauthorized("E-posta veya şifre hatalı.");
            }

           
            return Ok(new
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email
            });
        }

        
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null) return NotFound();

            student.Password = null; 
            return student;
        }
    }
}