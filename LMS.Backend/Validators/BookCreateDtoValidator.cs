using FluentValidation;
using LMS.Backend.Data;
using LMS.Backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace LMS.Backend.Validators
{
    public class BookCreateDtoValidator : AbstractValidator<BookCreateDto>
    {
        private readonly LMSDbContext _context;

        public BookCreateDtoValidator(LMSDbContext context)
        {
            _context = context;

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Kitap adı boş olamaz.")
                .MaximumLength(150).WithMessage("Kitap adı en fazla 150 karakter olmalı.");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Yazar adı boş olamaz.")
                .MaximumLength(120).WithMessage("Yazar adı en fazla 120 karakter olmalı.");

            RuleFor(x => x.LibraryId)
                .GreaterThan(0).WithMessage("Geçerli bir kütüphane seçilmelidir.");

            RuleFor(x => x.PageCount)
                .GreaterThan(0).WithMessage("Sayfa sayısı 0'dan büyük olmalı.")
                .When(x => x.PageCount.HasValue);

            RuleFor(x => x.Isbn)
                .NotEmpty().WithMessage("ISBN boş olamaz.")
                .Must(BeValidIsbn13Format).WithMessage("ISBN 13 haneli olmalı ve 978 veya 979 ile başlamalı.")
                .MustAsync(IsIsbnUniqueNormalized)
                .WithMessage("Bu ISBN numarası kayıtlı. Farklı bir numara girin.");
        }

        private static string NormalizeIsbn(string? isbn)
        {
            if (string.IsNullOrWhiteSpace(isbn)) return "";
            
            return new string(isbn.Where(char.IsDigit).ToArray());
        }

        private static bool BeValidIsbn13Format(string isbn)
        {
            var n = NormalizeIsbn(isbn);
            if (n.Length != 13) return false;
            return n.StartsWith("978") || n.StartsWith("979");
        }

        private async Task<bool> IsIsbnUniqueNormalized(string isbn, CancellationToken cancellationToken)
        {
            var n = NormalizeIsbn(isbn);
            return !await _context.Books.AnyAsync(b => b.Isbn == n, cancellationToken);
        }
    }
}
//Bu dosya, yeni kitap eklenirken gelen bilgilerin doğru olup olmadığını (boş alan, uzunluk,
//ISBN formatı ve ISBN tekrar kontrolü gibi) kontrol eden doğrulama sınıfıdır.
