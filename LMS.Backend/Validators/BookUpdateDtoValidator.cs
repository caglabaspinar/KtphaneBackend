using FluentValidation;
using LMS.Backend.DTOs;

namespace LMS.Backend.Validators
{
    public class BookUpdateDtoValidator : AbstractValidator<BookUpdateDto>
    {
        public BookUpdateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Kitap adı boş olamaz.");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Yazar adı boş olamaz.");

            RuleFor(x => x.Isbn)
                .NotEmpty().WithMessage("ISBN boş olamaz.")
                .Must(BeValidIsbn13Prefix).WithMessage("ISBN 13 haneli olmalı ve 978 veya 979 ile başlamalı.");

           
            RuleFor(x => x.PageCount)
                .GreaterThan(0).When(x => x.PageCount.HasValue)
                .WithMessage("Sayfa sayısı 0'dan büyük olmalı.");
        }

        private bool BeValidIsbn13Prefix(string isbnRaw)
        {
            if (string.IsNullOrWhiteSpace(isbnRaw)) return false;

            
            var digits = new string(isbnRaw.Where(char.IsDigit).ToArray());

            if (digits.Length != 13) return false;
            return digits.StartsWith("978") || digits.StartsWith("979");
        }
    }
}
//Bu dosya, kitap güncellenirken gönderilen bilgilerin boş olup olmadığını, ISBN formatının
//doğru olup olmadığını ve sayfa sayısının geçerli değer içerip içermediğini kontrol eden doğrulama sınıfıdır.
