using Microsoft.EntityFrameworkCore;

namespace PillsBot.Server.Persistence;

public sealed class PillsBotDbContext : DbContext
{
    public PillsBotDbContext()
    {
    }

    public PillsBotDbContext(DbContextOptions<PillsBotDbContext> options)
        : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>().ToTable("messages");
        modelBuilder.Entity<Message>().HasKey(e => e.Id);
        modelBuilder.Entity<Message>().Property(e => e.Created).HasDefaultValueSql("now()").ValueGeneratedOnAdd();
        modelBuilder.Entity<Message>().Property(e => e.Modified).HasDefaultValueSql("now()").ValueGeneratedOnAddOrUpdate();
        modelBuilder.Entity<Message>().Property(e => e.Language).HasMaxLength(3);
        modelBuilder.Entity<Message>().Property(e => e.Reminder).HasMaxLength(200).IsUnicode();
        modelBuilder.Entity<Message>().Property(e => e.Acknowledgement).HasMaxLength(100).IsUnicode();
        modelBuilder.Entity<Message>().Property(e => e.Appreciation).HasMaxLength(100).IsUnicode();
        modelBuilder.Entity<Message>().HasIndex(e => e.Language).HasMethod("hash");
    }
}
