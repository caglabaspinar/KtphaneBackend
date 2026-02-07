using Microsoft.Extensions.Logging;
using LMS.Backend.Services;
using FluentValidation;
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
        private readonly IValidator<StudentRegisterDto> _registerValidator;
        private readonly IValidator<StudentLoginDto> _loginValidator;
        private readonly IPasswordService _passwordService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(
            LMSDbContext context,
            IValidator<StudentRegisterDto> registerValidator,
            IValidator<StudentLoginDto> loginValidator,
            IPasswordService passwordService,
            IJwtTokenService jwtTokenService,
            ILogger<StudentsController> logger)
        {
            _context = context;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] StudentRegisterDto request)
        {
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var normalizedEmail = request.Email.Trim().ToLower();

            var student = new Student
            {
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = _passwordService.HashPassword(request.Password),
                Role = "Student"
            };

            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, new
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                Role = student.Role
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] StudentLoginDto loginDto)
        {
            var validationResult = await _loginValidator.ValidateAsync(loginDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            var normalizedEmail = loginDto.Email.Trim().ToLower();
            _logger.LogInformation("Login denemesi. Email: {Email}", normalizedEmail);

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == normalizedEmail);

            if (student == null)
            {
                _logger.LogWarning("Login başarısız (kullanıcı yok). Email: {Email}", normalizedEmail);
                return Unauthorized("E-posta veya şifre hatalı.");
            }

            var ok = _passwordService.VerifyPassword(student.PasswordHash, loginDto.Password);
            if (!ok)
            {
                _logger.LogWarning("Login başarısız (şifre yanlış). StudentId: {StudentId}, Email: {Email}", student.Id, normalizedEmail);
                return Unauthorized("E-posta veya şifre hatalı.");
            }

            var token = _jwtTokenService.GenerateToken(
                student.Id,
                student.Email,
                student.FullName,
                student.Role
            );

            _logger.LogInformation("Login başarılı. StudentId: {StudentId}, Role: {Role}", student.Id, student.Role);

            return Ok(new
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                Role = student.Role,
                Token = token
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            return Ok(new
            {
                Id = student.Id,
                FullName = student.FullName,
                Email = student.Email,
                Role = student.Role
            });
        }
    }
}
