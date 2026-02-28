using FluentValidation;
using LMS.Backend.DTOs;

namespace LMS.Backend.Validators
{
    public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email zorunludur.")
                .EmailAddress().WithMessage("Email formatı geçersiz.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Kod zorunludur.")
                .Length(6).WithMessage("Kod 6 haneli olmalıdır.")
                .Matches("^[0-9]{6}$").WithMessage("Kod sadece rakamlardan oluşmalıdır.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Yeni şifre zorunludur.")
                .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalıdır.")
                .Matches("[A-Z]").WithMessage("Şifre en az 1 büyük harf içermelidir.")
                .Matches("[a-z]").WithMessage("Şifre en az 1 küçük harf içermelidir.")
                .Matches("[0-9]").WithMessage("Şifre en az 1 rakam içermelidir.");
        }
    }
}
//Bu dosya, şifre sıfırlama sırasında gönderilen email, doğrulama kodu ve yeni şifrenin
//kurallara uygun olup olmadığını kontrol eden doğrulama sınıfıdır.
