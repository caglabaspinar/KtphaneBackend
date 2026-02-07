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
