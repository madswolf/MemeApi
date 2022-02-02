using MemeApi.Models.Entity;
using Microsoft.EntityFrameworkCore;

namespace MemeApi.Models.Context
{
    public class MemeContext : DbContext
    {
        public DbSet<Meme> Memes { get; set; }
        public DbSet<MemeVisual> Visuals { get; set; }
        public DbSet<MemeSound> Sounds { get; set; }
        public DbSet<MemeTopText> Toptexts { get; set; }
        public DbSet<MemeBottomText> BottomTexts { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Votable> Votables { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Topic> Topics { get; set; }


        public MemeContext(DbContextOptions<MemeContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(m => m.Votes)
                .WithOne(v => v.User)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Meme>()
                .HasOne(m => m.MemeVisual);

            modelBuilder.Entity<Meme>()
                .HasOne(m => m.MemeSound);

            modelBuilder.Entity<Meme>()
                .HasOne(m => m.MemeTopText);

            modelBuilder.Entity<Meme>()
                .HasOne(m => m.MemeBottomtext);

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder.Entity<Vote>()
                .HasOne(v => v.Element)
                .WithMany(u => u.Votes)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            modelBuilder.Entity<Topic>()
                .HasOne(t => t.Owner);
            
            modelBuilder.Entity<Topic>()
                .HasMany(t => t.Moderators);
        
        }
    }
}
