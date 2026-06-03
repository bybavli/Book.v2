using Book.v2.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Book.v2.Data;

public class ContextDb : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Models.Entities.Book> Books => Set<Models.Entities.Book>();
    public DbSet<BookPage> BookPages => Set<BookPage>();
    public DbSet<ReadingListEntry> ReadingListEntries => Set<ReadingListEntry>();
    public DbSet<ReadingProgress> ReadingProgresses => Set<ReadingProgress>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();

    public ContextDb(DbContextOptions<ContextDb> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(u => u.Email).IsUnique();

            entity.HasMany(u => u.ReadingList)
                  .WithOne(r => r.User)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(u => u.ReadingList)
                  .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasMany(u => u.ReadingProgresses)
                  .WithOne(rp => rp.User)
                  .HasForeignKey(rp => rp.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(u => u.ReadingProgresses)
                  .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasOne(u => u.Preference)
                  .WithOne(p => p.User)
                  .HasForeignKey<UserPreference>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Models.Entities.Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Title).IsRequired().HasMaxLength(300);
            entity.Property(b => b.Author).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Genre).IsRequired().HasMaxLength(100);
            entity.Property(b => b.Description).HasMaxLength(2000);
            entity.Property(b => b.CoverImageUrl).HasMaxLength(500);
            entity.Property(b => b.ContentFileUrl).HasMaxLength(500);
            entity.Property(b => b.Tags).HasMaxLength(500);

            entity.HasMany(b => b.InReadingLists)
                  .WithOne(r => r.Book)
                  .HasForeignKey(r => r.BookId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(b => b.InReadingLists)
                  .UsePropertyAccessMode(PropertyAccessMode.Field);

            entity.HasMany(b => b.Pages)
                  .WithOne(p => p.Book)
                  .HasForeignKey(p => p.BookId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Navigation(b => b.Pages)
                  .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<BookPage>(entity =>
        {
            entity.HasKey(bp => bp.Id);
            entity.Property(bp => bp.Content).IsRequired();
            entity.HasIndex(bp => new { bp.BookId, bp.PageNumber }).IsUnique();
        });

        modelBuilder.Entity<ReadingListEntry>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => new { r.UserId, r.BookId }).IsUnique();
        });

        modelBuilder.Entity<ReadingProgress>(entity =>
        {
            entity.HasKey(rp => rp.Id);
            entity.HasIndex(rp => new { rp.UserId, rp.BookId }).IsUnique();
        });

        modelBuilder.Entity<UserPreference>(entity =>
        {
            entity.HasKey(up => up.Id);
            entity.HasIndex(up => up.UserId).IsUnique();
            entity.Property(up => up.FavoriteGenres).IsRequired().HasMaxLength(1000);
            entity.Property(up => up.FavoriteTags).IsRequired().HasMaxLength(1000);
        });
    }
}
