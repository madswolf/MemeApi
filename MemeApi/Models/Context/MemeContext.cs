using System;
using MemeApi.library;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Challenges;
using MemeApi.Models.Entity.Dubloons;
using MemeApi.Models.Entity.Lottery;
using MemeApi.Models.Entity.Memes;
using MemeApi.Models.Entity.Places;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
    //public DbSet<MemeSound> Sounds { get; set; }
    public DbSet<MemeText> Texts { get; set; }
    public DbSet<Vote> Votes { get; set; }
    public DbSet<Votable> Votables { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<DubloonEvent> DubloonEvents { get; set; }
    public DbSet<MemePlace> MemePlaces { get; set; }
    public DbSet<PlaceSubmission> PlaceSubmissions { get; set; }
    
    public DbSet<PlacePixelPrice> PixelPrices { get; set; }
    public DbSet<Lottery> Lotteries { get; set; }
    
    public DbSet<LotteryBracket> LotteryBrackets { get; set; }
    
    public DbSet<LotteryItem> LotteryItems { get; set; }
    
    public DbSet<LotteryTicket> LotteryTickets { get; set; }
    
    public DbSet<TriviaQuestion> TriviaQuestions { get; set; }
    
    public DbSet<TriviaQuestion> TriviaAnswers { get; set; }
    
    public DbSet<Challenge> Challenges { get; set; }
    
    public DbSet<ChallengeAttempt> ChallengeAttempts { get; set; }
    
    
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

        modelBuilder.Entity<User>()
            .HasMany(u => u.Votables)
            .WithOne(v => v.Owner)
            .IsRequired(false)
            .HasForeignKey(v => v.OwnerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>()
            .HasMany(u => u.DubloonEvents)
            .WithOne(d => d.Owner)
            .HasForeignKey(u => u.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.PlaceSubmissions)
            .WithOne(p => p.Owner)
            .IsRequired()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MemePlace>()
            .ToTable("MemePlaces")
            .HasMany(m => m.PlaceSubmissions)
            .WithOne(p => p.Place)
            .IsRequired()
            .HasForeignKey(p => p.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MemePlace>()
            .Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<MemePlace>()
            .HasMany(m => m.PriceHistory)
            .WithOne(p => p.Place)
            .IsRequired()
            .HasForeignKey(p => p.PlaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlacePixelPrice>()
            .ToTable("PlacePixelPrices")
            .Property(p => p.PriceChangeTime)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<PlaceSubmission>()
            .ToTable("PlaceSubmissions")
            .Property(u => u.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<PlacePixelPurchase>()
            .HasOne(p => p.Submission)
            .WithOne()
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<DubloonEvent>()
            .Property(u => u.EventTimestamp)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<DubloonEvent>().UseTptMappingStrategy().HasKey(d => d.Id);

        modelBuilder.Entity<DailyVote>().ToTable("DailyVotes");

        modelBuilder.Entity<DailyVote>()
            .HasOne(d => d.Vote)
            .WithOne()
            .HasForeignKey<DailyVote>(d => d.VoteId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Transaction>()
            .HasOne(d => d.Other)
            .WithMany()
            .HasForeignKey(d => d.OtherUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<User>().Property(v => v.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<User>().Property(v => v.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<MemeVisual>().ToTable("MemeVisuals");

        modelBuilder.Entity<MemeText>().ToTable("MemeTexts");

        modelBuilder.Entity<Votable>().ToTable("Votables");

        modelBuilder.Entity<Votable>().UseTptMappingStrategy().HasKey(v => v.Id);

        modelBuilder.Entity<Votable>().Property(v => v.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        modelBuilder.Entity<Votable>().Property(v => v.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        modelBuilder.Entity<Meme>(entity =>
        {
            entity.HasOne(m => m.Visual)
                .WithMany()
                .HasForeignKey(m => m.VisualId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.TopText)
                .WithMany()
                .HasForeignKey(m => m.TopTextId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(m => m.BottomText)
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

            entity.HasOne(v => v.DubloonEvent)
                .WithOne()
                .HasForeignKey<Vote>(v => v.DubloonEventId)
                .IsRequired(false);

            entity.Property(v => v.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(v => v.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("Topics");

            entity.Property(v => v.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(v => v.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(t => t.Owner)
            .WithMany(u => u.Topics)
            .HasForeignKey(t => t.OwnerId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(t => t.Moderators)
            .WithMany()
            .UsingEntity<TopicModerator>(
                entity => entity.HasOne(tv => tv.Moderator).WithMany().HasForeignKey(tv => tv.ModeratorId), 
                entity => entity.HasOne(tv => tv.Topic).WithMany().HasForeignKey(tv => tv.TopicID),
                entity => entity.HasKey(tm => new {tm.ModeratorId, tm.TopicID}));

            entity.HasIndex(t => t.Name)
            .IsUnique();
            
            entity.HasMany(t => t.Votables)
                .WithMany(v => v.Topics)
                .UsingEntity<TopicVotable>(
                entity => entity.HasOne(tv => tv.Votable).WithMany().HasForeignKey(tv => tv.VotableId),
                entity => entity.HasOne(tv => tv.Topic).WithMany().HasForeignKey(tv => tv.TopicID),
                entity => entity.HasKey(tm => new { tm.VotableId, tm.TopicID }));
        });

        modelBuilder.Entity<Lottery>().ToTable("Lotteries");
        
        modelBuilder.Entity<LotteryBracket>()
            .ToTable("LotteryBrackets")
            .HasOne(bracket => bracket.Lottery)
            .WithMany(lottery => lottery.Brackets)
            .HasForeignKey(bracket => bracket.LotteryId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<LotteryItem>()
            .ToTable("LotteryItems")
            .HasOne(item => item.Bracket)
            .WithMany(bracket => bracket.Items)
            .HasForeignKey(item => item.BracketId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<LotteryTicket>()
            .ToTable("LotteryTickets")
            .HasOne(ticket => ticket.Item)
            .WithMany(item => item.Tickets)
            .HasForeignKey(ticket => ticket.ItemId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Challenge>()
            .ToTable("Challenges")
            .HasMany(challenge => challenge.Attempts)
            .WithOne(attempt => attempt.AttemptedChallenge)
            .HasForeignKey(attempt => attempt.ChallengeId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Challenge>().UseTptMappingStrategy().HasKey(v => v.Id);

        modelBuilder.Entity<ChallengeAttempt>()
            .ToTable("ChallengeAttempts")
            .HasOne(attempt => attempt.Owner)
            .WithMany(user => user.ChallengeAttempts)
            .HasForeignKey(attempt => attempt.OwnerId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<ChallengeAttempt>().UseTptMappingStrategy().HasKey(v => v.Id);

        modelBuilder.Entity<TriviaQuestion>()
            .ToTable("TriviaQuestions");
        modelBuilder.Entity<TriviaQuestion>()
            .OwnsMany(question => question.Options);
        modelBuilder.Entity<TriviaQuestion>()
            .OwnsOne(question => question.CorrectOption);
        
        modelBuilder.Entity<TriviaAnswer>()
            .ToTable("TriviaAnswers");
        
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

        var defaultTopic3 = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            OwnerId = admin.Id,
            Name = "Bean-den",
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
        modelBuilder.Entity<Topic>().HasData(defaultTopic, defaultTopic2, defaultTopic3, memeOfTheDayTopic);
    }
}
