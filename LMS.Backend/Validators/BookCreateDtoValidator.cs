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
                .NotEmpty().WithMessage("Kitap adı boş olamaz.");

            RuleFor(x => x.Author)
                .NotEmpty().WithMessage("Yazar adı boş olamaz.");

            RuleFor(x => x.Isbn)
                .NotEmpty().WithMessage("ISBN boş olamaz.")
                .MustAsync(IsIsbnUnique)
                .WithMessage("Bu ISBN numarası kayıtlı. Farklı bir numara girin.");

            RuleFor(x => x.LibraryId)
                .GreaterThan(0).WithMessage("Geçerli bir kütüphane seçilmelidir.");
        }

        private async Task<bool> IsIsbnUnique(string isbn, CancellationToken cancellationToken)
        {
            return !await _context.Books.AnyAsync(b => b.Isbn == isbn, cancellationToken);
        }
    }
}
