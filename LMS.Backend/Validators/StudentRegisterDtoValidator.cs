using FluentValidation;
using LMS.Backend.Data;
using LMS.Backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LMS.Backend.Validators
{
    public class StudentRegisterDtoValidator : AbstractValidator<StudentRegisterDto>
    {
        public StudentRegisterDtoValidator(LMSDbContext context)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta zorunludur.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                
                .DependentRules(() =>
                {
                    RuleFor(x => x.Email)
                        .MustAsync(async (email, ct) =>
                        {
                            var normalized = email.Trim().ToLower();
                            return !await context.Students.AnyAsync(s => s.Email == normalized, ct);
                        })
                        .WithMessage("Bu e-posta adresi zaten kullanılıyor.");
                });

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre zorunludur.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .Matches("[A-Z]").WithMessage("Şifre en az bir büyük harf içermelidir.")
                .Matches("[a-z]").WithMessage("Şifre en az bir küçük harf içermelidir.")
                .Matches("[0-9]").WithMessage("Şifre en az bir rakam içermelidir.");


        }
    }
}
