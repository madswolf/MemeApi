using MemeApi.library;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace MemeApi.Models.Context;

public class MemeContext : IdentityDbContext<User, IdentityRole<string>, string>
{
    private readonly MemeApiSettings _settings;
    public MemeContext(DbContextOptions<MemeContext> options, MemeApiSettings settings) : base(options)
    {
        _settings = settings;
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
            .ToTable("Users")
            .HasMany(m => m.Votes)
            .WithOne(v => v.User)
            .HasForeignKey(v => v.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MemeVisual>().ToTable("MemeVisuals");

        modelBuilder.Entity<MemeText>().ToTable("MemeTexts");

        modelBuilder.Entity<Votable>().ToTable("Votables");

        modelBuilder.Entity<Votable>().UseTptMappingStrategy().HasKey(v => v.Id);
        
        modelBuilder.Entity<Meme>(entity =>
        {
            entity.HasOne(m => m.Visual)
            .WithMany()
            .HasForeignKey(m => m.VisualId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<MemeText>(m => m.TopText)
            .WithMany()
            .HasForeignKey(m => m.TopTextId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<MemeText>(m => m.BottomText)
            .WithMany()
            .HasForeignKey(m => m.BottomTextId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Vote>(entity =>
        {
            entity.ToTable("Votes");
            entity.HasOne(v => v.User)
            .WithMany(u => u.Votes)
            .HasForeignKey(v => v.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Element)
            .WithMany(v => v.Votes)
            .HasForeignKey(v => v.ElementId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("Topics");
            entity.HasOne(t => t.Owner)
            .WithMany(u => u.Topics)
            .HasForeignKey(t => t.OwnerId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Moderators)
            .WithMany()
            .UsingEntity<TopicModerators>(
                entity => entity.HasOne(tv => tv.Moderator).WithMany().HasForeignKey(tv => tv.ModeratorId), 
                entity => entity.HasOne(tv => tv.Topic).WithMany().HasForeignKey(tv => tv.TopicID));

            entity.HasIndex(t => t.Name)
            .IsUnique();

           // entity.HasMany(t => t.Votables)
           // .WithMany(v => v.Topics)
           // .UsingEntity<TopicVotable>(entity =>
           // {
           //     entity.ToTable("TopicVotable");
           //     entity.HasOne(tv => tv.Topic).WithMany().HasForeignKey(tv => tv.TopicID);
           //     entity.HasOne(tv => tv.Votable).WithMany().HasForeignKey(tv => tv.VotableId);
           //     entity.HasKey(vt => new { vt.VotableId, vt.TopicID });
           // });
            
            entity.HasMany(t => t.Votables)
                .WithMany(v => v.Topics)
                .UsingEntity("TopicVotable",
                entity => entity.HasOne(typeof(Votable)).WithMany().HasForeignKey("VotableId"),
                entity => entity.HasOne(typeof(Topic)).WithMany().HasForeignKey("TopicID"),
                entity => entity.HasKey("VotableId","TopicID"));
        });

        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = _settings.GetAdminUsername() ,
            Email = _settings.GetAdminPassword(),
            SecurityStamp = DateTime.UtcNow.ToString(),
            LastLoginAt = DateTime.UtcNow,
        };

        var defaultTopic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            OwnerId = admin.Id,
            Name = "Swu-legacy",
            Description = "Memes created 2020-2023",
        };

        var defaultTopic2 = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            OwnerId = admin.Id,
            Name = "Rotte-Grotte",
            Description = "Memes are back baby!",
        };

        var memeOfTheDayTopic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            OwnerId = admin.Id,
            Name = "MemeOfTheDay",
            Description = "Memes of the day",
        };

        modelBuilder.Entity<User>().HasData(admin);
        modelBuilder.Entity<Topic>().HasData(defaultTopic, defaultTopic2, memeOfTheDayTopic);
    }
}
