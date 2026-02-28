using FluentValidation;
using LMS.Backend.DTOs;

namespace LMS.Backend.Validators
{
    public class StudentLoginDtoValidator : AbstractValidator<StudentLoginDto>
    {
        public StudentLoginDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-posta zorunludur.")
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Şifre zorunludur.");
        }
    }
}
//Bu dosya, giriş yapılırken gönderilen e-posta ve şifrenin boş olup olmadığını ve e-posta
//formatının doğru olup olmadığını kontrol eden doğrulama sınıfıdır.

