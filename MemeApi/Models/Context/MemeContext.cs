using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MemeApi.Models.Context
{
    public class MemeContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        private readonly IConfiguration _configuration;
        public MemeContext(DbContextOptions<MemeContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }
        public DbSet<Meme> Memes { get; set; }
        public DbSet<MemeVisual> Visuals { get; set; }
        public DbSet<MemeSound> Sounds { get; set; }
        public DbSet<MemeText> Texts { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Votable> Votables { get; set; }
        public DbSet<Topic> Topics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();

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
                .HasOne(m => m.Toptext);

            modelBuilder.Entity<Meme>()
                .HasOne(m => m.BottomText);

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

            var admin = new User
            {
                UserName = _configuration["Admin.UserName"],
                Email = _configuration["Admin.Email"],
            };

            var defaultTopic = new Topic
            {
                Name = "Swu-legacy",
                Description = "Memes created 2020-2023",
                Id = 0,
                Owner = admin,
                CreatedAt = System.DateTime.Now,
                UpdatedAt = System.DateTime.Now
            };

            var defaultTopic2 = new Topic
            {
                Name = "Swu-reloaded",
                Description = "Memes are back baby!",
                Id = 1,
                Owner = admin,
                CreatedAt = System.DateTime.Now,
                UpdatedAt = System.DateTime.Now
            };

            modelBuilder.Entity<User>().HasData(admin);
            modelBuilder.Entity<Topic>().HasData(defaultTopic, defaultTopic2);
        }
    }
}
