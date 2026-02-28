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
        private readonly IValidator<ResetPasswordDto> _resetPasswordValidator;
        private readonly IEmailService _emailService;


        public StudentsController(
            LMSDbContext context,
            IValidator<StudentRegisterDto> registerValidator,
            IValidator<StudentLoginDto> loginValidator,
            IPasswordService passwordService,
            IJwtTokenService jwtTokenService,
            ILogger<StudentsController> logger,
            IValidator<ResetPasswordDto> resetPasswordValidator,
            IEmailService emailService)

        {
            _context = context;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _passwordService = passwordService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
            _resetPasswordValidator = resetPasswordValidator;
            _emailService = emailService;
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

            _logger.LogInformation("Register başarılı. StudentId: {StudentId}", student.Id);


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
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new { message = "Email zorunludur." });

            var email = dto.Email.Trim().ToLower();
            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);

            
            if (student == null)
            {
                _logger.LogWarning("Forgot password denemesi (kullanıcı yok). Email: {Email}", email);
                return BadRequest(new { message = "Bu mail adresi kayıtlı değil. Lütfen kayıt olun." });
            }

            
            var code = Random.Shared.Next(100000, 999999).ToString();
            student.PasswordResetCode = code;
            student.PasswordResetExpiresAt = DateTime.Now.AddMinutes(15);

            await _context.SaveChangesAsync();

            var subject = "Şifre Sıfırlama Kodu";
            var body =
        $@"Merhaba {student.FullName},

Şifre sıfırlama kodunuz: {code}

Bu kod 15 dakika geçerlidir.

Kütüphane Mobil";

            await _emailService.SendAsync(student.Email, subject, body);

            _logger.LogInformation(
                "Forgot password code üretildi ve mail atıldı. StudentId: {StudentId}, Email: {Email}",
                student.Id, email
            );

            return Ok(new { message = "Şifre sıfırlama kodu e-postana gönderildi." });
        }


        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { message = "Email, kod ve yeni şifre zorunludur." });

            var email = dto.Email.Trim().ToLower();
            _logger.LogError("RESET-PASSWORD HIT ✅ (new-same-check should be active) Email: {Email}", email);
            dto.NewPassword = dto.NewPassword.Trim();
            dto.Code = dto.Code.Trim();

            var validationResult = await _resetPasswordValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));

            var student = await _context.Students.FirstOrDefaultAsync(s => s.Email == email);
            if (student == null)
            {
                _logger.LogWarning("Reset password başarısız (kullanıcı yok). Email: {Email}", email);
                return BadRequest(new { message = "Kod veya e-posta hatalı." });
            }

            if (student.PasswordResetCode == null || student.PasswordResetExpiresAt == null)
            {
                _logger.LogWarning("Reset password başarısız (kod yok). StudentId: {StudentId}, Email: {Email}", student.Id, email);
                return BadRequest(new { message = "Kod veya e-posta hatalı." });
            }

            if (student.PasswordResetExpiresAt < DateTime.Now)
            {
                _logger.LogWarning("Reset password başarısız (kod süresi doldu). StudentId: {StudentId}, Email: {Email}", student.Id, email);
                return BadRequest(new { message = "Kodun süresi doldu." });
            }

            if (student.PasswordResetCode != dto.Code.Trim())
            {
                _logger.LogWarning("Reset password başarısız (kod yanlış). StudentId: {StudentId}, Email: {Email}", student.Id, email);
                return BadRequest(new { message = "Kod veya e-posta hatalı." });
            }


           
            var isSameAsOld = _passwordService.VerifyPassword(student.PasswordHash, dto.NewPassword);
            if (isSameAsOld)
            {
                _logger.LogWarning("Reset password başarısız (aynı şifre). StudentId: {StudentId}, Email: {Email}", student.Id, email);
                return BadRequest(new { message = "Yeni şifre eski şifreyle aynı olamaz." });
            }

            student.PasswordHash = _passwordService.HashPassword(dto.NewPassword);
            student.PasswordResetCode = null;
            student.PasswordResetExpiresAt = null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Reset password başarılı. StudentId: {StudentId}, Email: {Email}", student.Id, email);

            return Ok(new { message = "Şifre başarıyla güncellendi." });
        }

        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail([FromServices] IEmailService emailService)
        {
            await emailService.SendAsync(
                "test@recipient.com",
                "Mailtrap Test",
                "Merhaba! Bu bir test mailidir."
            );

            return Ok("Mail gönderildi (Mailtrap inbox'a düşmeli).");
        }



    }
}

//Bu dosya, öğrenci kayıt ve giriş işlemlerini yapıp JWT token üretir, öğrenci bilgisi getirir
//ve “şifremi unuttum/şifre sıfırla” akışında kod üretip mail göndererek şifreyi günceller.

