using Microsoft.EntityFrameworkCore;
using LMS.Backend.Models;

namespace LMS.Backend.Data
{
    public class LMSDbContext : DbContext
    {
        public LMSDbContext(DbContextOptions<LMSDbContext> options) : base(options)
        {
        }
        public DbSet<Library> Library { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<Student> Students { get; set; } = null!;
        public DbSet<StudentBook> StudentBooks { get; set; } = null!;

        


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StudentBook>()
                .HasOne(sb => sb.Student)
                .WithMany(s => s.StudentBooks)
                .HasForeignKey(sb => sb.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentBook>()
                .HasOne(sb => sb.Book)
                .WithMany()
                .HasForeignKey(sb => sb.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.Email)
                .IsUnique();

            modelBuilder.Entity<Student>()
                .Property(s => s.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Student")
                .IsRequired();
            

        }

    }
}

//Bu dosya, veritabanı tablolarını ve ilişkilerini tanımlayan EF Core DbContext sınıfıdır;
//`Program.cs`’teki `AddDbContext` ile bağlanır ve tüm controller’lar, servisler, `DbSeeder`
//ve modeller (`Library`, `Book`, `Student`, `StudentBook`) ile bağlantılı çalışır.
