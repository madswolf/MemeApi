﻿// <auto-generated />
using System;
using MemeApi.Models.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MemeApi.Migrations
{
    [DbContext(typeof(MemeContext))]
    [Migration("20241006150737_AddPlacePixelPrices")]
    partial class AddPlacePixelPrices
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MemeApi.Models.Entity.Dubloons.DubloonEvent", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<double>("Dubloons")
                        .HasColumnType("double precision");

                    b.Property<DateTime>("EventTimestamp")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("DubloonEvents");

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.Topic", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("HasRestrictedPosting")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastUpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("OwnerId");

                    b.ToTable("Topics", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "a3b615bf-3978-4ddc-b156-f4abcf811c58",
                            CreatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Memes created 2020-2023",
                            HasRestrictedPosting = false,
                            LastUpdatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "Swu-legacy",
                            OwnerId = "92d78bf0-dfaf-4da8-be82-fa93e897ad1c"
                        },
                        new
                        {
                            Id = "fe1b12f1-c495-4473-aff5-0650a3300c3c",
                            CreatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Memes are back baby!",
                            HasRestrictedPosting = false,
                            LastUpdatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "Rotte-Grotte",
                            OwnerId = "92d78bf0-dfaf-4da8-be82-fa93e897ad1c"
                        },
                        new
                        {
                            Id = "a8b43158-361f-498f-a659-953edeba4fe3",
                            CreatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Memes are back baby!",
                            HasRestrictedPosting = false,
                            LastUpdatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "Bean-den",
                            OwnerId = "92d78bf0-dfaf-4da8-be82-fa93e897ad1c"
                        },
                        new
                        {
                            Id = "8ef13d87-8043-4c24-97d8-f3bd9788bf32",
                            CreatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Description = "Memes of the day",
                            HasRestrictedPosting = false,
                            LastUpdatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Name = "MemeOfTheDay",
                            OwnerId = "92d78bf0-dfaf-4da8-be82-fa93e897ad1c"
                        });
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.Votable", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime>("LastUpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("OwnerId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Votables", (string)null);

                    b.UseTptMappingStrategy();
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.Vote", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("DubloonEventId")
                        .HasColumnType("text");

                    b.Property<string>("ElementId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<bool>("Upvote")
                        .HasColumnType("boolean");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("VoteNumber")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("DubloonEventId")
                        .IsUnique();

                    b.HasIndex("ElementId");

                    b.HasIndex("UserId");

                    b.ToTable("Votes", (string)null);
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Places.MemePlace", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int>("Height")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Width")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("MemePlaces", (string)null);
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Places.PlacePixelPrice", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("PlaceId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("PriceChangeTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<double>("PricePerPixel")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.HasIndex("PlaceId");

                    b.ToTable("PlacePixelPrices", (string)null);
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Places.PlaceSubmission", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<string>("OwnerId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("PixelChangeCount")
                        .HasColumnType("integer");

                    b.Property<string>("PlaceId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.HasIndex("PlaceId");

                    b.ToTable("PlaceSubmissions", (string)null);
                });

            modelBuilder.Entity("MemeApi.Models.Entity.TopicModerator", b =>
                {
                    b.Property<string>("ModeratorId")
                        .HasColumnType("text");

                    b.Property<string>("TopicID")
                        .HasColumnType("text");

                    b.HasKey("ModeratorId", "TopicID");

                    b.HasIndex("TopicID");

                    b.ToTable("TopicModerator");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.TopicVotable", b =>
                {
                    b.Property<string>("VotableId")
                        .HasColumnType("text");

                    b.Property<string>("TopicID")
                        .HasColumnType("text");

                    b.HasKey("VotableId", "TopicID");

                    b.HasIndex("TopicID");

                    b.ToTable("TopicVotable");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastLoginAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("LastUpdatedAt")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("ProfilePicFile")
                        .HasColumnType("text");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("text");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.HasIndex("UserName")
                        .IsUnique();

                    b.ToTable("Users", (string)null);

                    b.HasData(
                        new
                        {
                            Id = "92d78bf0-dfaf-4da8-be82-fa93e897ad1c",
                            AccessFailedCount = 0,
                            ConcurrencyStamp = "83c8a805-da91-44e6-ab1d-60b423b30241",
                            CreatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "",
                            EmailConfirmed = false,
                            LastLoginAt = new DateTime(2024, 10, 6, 15, 7, 37, 512, DateTimeKind.Utc).AddTicks(8234),
                            LastUpdatedAt = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            LockoutEnabled = false,
                            PhoneNumberConfirmed = false,
                            SecurityStamp = "06-10-2024 15:07:37",
                            TwoFactorEnabled = false,
                            UserName = ""
                        });
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole<string>", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ClaimType")
                        .HasColumnType("text");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderKey")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("RoleId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("LoginProvider")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Value")
                        .HasColumnType("text");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Dubloons.DailyVote", b =>
                {
                    b.HasBaseType("MemeApi.Models.Entity.Dubloons.DubloonEvent");

                    b.Property<string>("VoteId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasIndex("VoteId")
                        .IsUnique();

                    b.ToTable("DailyVotes", (string)null);
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Dubloons.PlacePixelPurchase", b =>
                {
                    b.HasBaseType("MemeApi.Models.Entity.Dubloons.DubloonEvent");

                    b.Property<string>("SubmissionId")
                        .HasColumnType("text");

                    b.HasIndex("SubmissionId")
                        .IsUnique();

                    b.ToTable("PlacePixelPurchase");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Dubloons.Transaction", b =>
                {
                    b.HasBaseType("MemeApi.Models.Entity.Dubloons.DubloonEvent");

                    b.Property<string>("OtherUserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasIndex("OtherUserId");

                    b.ToTable("Transaction");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.Meme", b =>
                {
                    b.HasBaseType("MemeApi.Models.Entity.Memes.Votable");

                    b.Property<string>("BottomTextId")
                        .HasColumnType("text");

                    b.Property<string>("TopTextId")
                        .HasColumnType("text");

                    b.Property<string>("VisualId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasIndex("BottomTextId");

                    b.HasIndex("TopTextId");

                    b.HasIndex("VisualId");

                    b.ToTable("Memes");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.MemeText", b =>
                {
                    b.HasBaseType("MemeApi.Models.Entity.Memes.Votable");

                    b.Property<int>("Position")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasColumnType("text");

                    b.ToTable("MemeTexts", (string)null);
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.MemeVisual", b =>
                {
                    b.HasBaseType("MemeApi.Models.Entity.Memes.Votable");

                    b.Property<string>("Filename")
                        .IsRequired()
                        .HasColumnType("text");

                    b.ToTable("MemeVisuals", (string)null);
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Dubloons.DubloonEvent", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.User", "Owner")
                        .WithMany("DubloonEvents")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.Topic", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.User", "Owner")
                        .WithMany("Topics")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.Votable", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.User", "Owner")
                        .WithMany("Votables")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.Vote", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.Dubloons.DubloonEvent", "DubloonEvent")
                        .WithOne()
                        .HasForeignKey("MemeApi.Models.Entity.Memes.Vote", "DubloonEventId");

                    b.HasOne("MemeApi.Models.Entity.Memes.Votable", "Element")
                        .WithMany("Votes")
                        .HasForeignKey("ElementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemeApi.Models.Entity.User", "User")
                        .WithMany("Votes")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DubloonEvent");

                    b.Navigation("Element");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Places.PlacePixelPrice", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.Places.MemePlace", "Place")
                        .WithMany("PriceHistory")
                        .HasForeignKey("PlaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Place");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Places.PlaceSubmission", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.User", "Owner")
                        .WithMany("PlaceSubmissions")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemeApi.Models.Entity.Places.MemePlace", "Place")
                        .WithMany("PlaceSubmissions")
                        .HasForeignKey("PlaceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");

                    b.Navigation("Place");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.TopicModerator", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.User", "Moderator")
                        .WithMany()
                        .HasForeignKey("ModeratorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemeApi.Models.Entity.Memes.Topic", "Topic")
                        .WithMany()
                        .HasForeignKey("TopicID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Moderator");

                    b.Navigation("Topic");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.TopicVotable", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.Memes.Topic", "Topic")
                        .WithMany()
                        .HasForeignKey("TopicID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemeApi.Models.Entity.Memes.Votable", "Votable")
                        .WithMany()
                        .HasForeignKey("VotableId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Topic");

                    b.Navigation("Votable");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<string>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole<string>", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemeApi.Models.Entity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Dubloons.DailyVote", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.Dubloons.DubloonEvent", null)
                        .WithOne()
                        .HasForeignKey("MemeApi.Models.Entity.Dubloons.DailyVote", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemeApi.Models.Entity.Memes.Vote", "Vote")
                        .WithOne()
                        .HasForeignKey("MemeApi.Models.Entity.Dubloons.DailyVote", "VoteId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("Vote");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Dubloons.PlacePixelPurchase", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.Dubloons.DubloonEvent", null)
                        .WithOne()
                        .HasForeignKey("MemeApi.Models.Entity.Dubloons.PlacePixelPurchase", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemeApi.Models.Entity.Places.PlaceSubmission", "Submission")
                        .WithOne()
                        .HasForeignKey("MemeApi.Models.Entity.Dubloons.PlacePixelPurchase", "SubmissionId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Submission");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Dubloons.Transaction", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.Dubloons.DubloonEvent", null)
                        .WithOne()
                        .HasForeignKey("MemeApi.Models.Entity.Dubloons.Transaction", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemeApi.Models.Entity.User", "Other")
                        .WithMany()
                        .HasForeignKey("OtherUserId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("Other");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.Meme", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.Memes.MemeText", "BottomText")
                        .WithMany()
                        .HasForeignKey("BottomTextId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("MemeApi.Models.Entity.Memes.Votable", null)
                        .WithOne()
                        .HasForeignKey("MemeApi.Models.Entity.Memes.Meme", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MemeApi.Models.Entity.Memes.MemeText", "TopText")
                        .WithMany()
                        .HasForeignKey("TopTextId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.HasOne("MemeApi.Models.Entity.Memes.MemeVisual", "Visual")
                        .WithMany()
                        .HasForeignKey("VisualId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BottomText");

                    b.Navigation("TopText");

                    b.Navigation("Visual");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.MemeText", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.Memes.Votable", null)
                        .WithOne()
                        .HasForeignKey("MemeApi.Models.Entity.Memes.MemeText", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.MemeVisual", b =>
                {
                    b.HasOne("MemeApi.Models.Entity.Memes.Votable", null)
                        .WithOne()
                        .HasForeignKey("MemeApi.Models.Entity.Memes.MemeVisual", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Memes.Votable", b =>
                {
                    b.Navigation("Votes");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.Places.MemePlace", b =>
                {
                    b.Navigation("PlaceSubmissions");

                    b.Navigation("PriceHistory");
                });

            modelBuilder.Entity("MemeApi.Models.Entity.User", b =>
                {
                    b.Navigation("DubloonEvents");

                    b.Navigation("PlaceSubmissions");

                    b.Navigation("Topics");

                    b.Navigation("Votables");

                    b.Navigation("Votes");
                });
#pragma warning restore 612, 618
        }
    }
}
