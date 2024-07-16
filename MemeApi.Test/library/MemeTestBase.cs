using MemeApi.library;
using MemeApi.library.repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MemeApi.Test.library;

[Collection(nameof(DatabaseTestCollection))]
public class MemeTestBase : IAsyncLifetime
{
    protected readonly MemeContext _context;
    protected readonly VisualRepository _visualRepository;
    protected readonly TextRepository _textRepository;
    protected readonly MemeRepository _memeRepository;
    protected readonly TopicRepository _topicRepository;
    protected readonly UserRepository _userRepository;
    protected readonly VotableRepository _votableRepository;
    protected readonly MemeApiSettings _settings;
    protected readonly IntegrationTestFactory _fixture;

    protected readonly MemeRenderingService _memeRenderingService;

    private readonly Dictionary<string, string?> _config = new()
    {
                {"Topic_Default_TopicName", "Rotte-Grotte"},
                {"Admin_Username", "test"},
                {"Admin_Password", "test"},
                {"Media_Host", "test"}
            };
    public MemeTestBase(IntegrationTestFactory databaseFixture)
    {
        var config = new ConfigurationBuilder()
        .AddInMemoryCollection(_config)
        .Build();
        _settings = new MemeApiSettings(config);

        _memeRenderingService = new MemeRenderingService(_settings, new WebFileLoader(_settings));
        _context = databaseFixture.Db;
        _fixture = databaseFixture;

        _userRepository = new UserRepository(_context, TestUserManager<User>(), new FileSaverStub());
        _topicRepository = new TopicRepository(_context, _userRepository, _settings);
        _visualRepository = new VisualRepository(_context, new FileSaverStub(), new FileRemoverStub(), _topicRepository);
        _textRepository = new TextRepository(_context, _topicRepository);
        _memeRepository = new MemeRepository(_context, _visualRepository, _textRepository, _topicRepository, _settings);
        _votableRepository = new VotableRepository(_context);
    }

    public static HttpContext GetMockedHttpContext()
    {
        var context = new DefaultHttpContext();
        var mockSession = new Mock<ISession>();
        context.RequestServices = new Mock<IServiceProvider>().Object;
        context.Session = mockSession.Object;
        return context;
    }
    public static UserManager<TUser> TestUserManager<TUser>(IUserStore<TUser>? store = null) where TUser : class
    {
        store ??= new Mock<IUserStore<TUser>>().Object;
        var options = new Mock<IOptions<IdentityOptions>>();
        var idOptions = new IdentityOptions();
        idOptions.Lockout.AllowedForNewUsers = false;
        options.Setup(o => o.Value).Returns(idOptions);
        var userValidators = new List<IUserValidator<TUser>>();
        var validator = new Mock<IUserValidator<TUser>>();
        userValidators.Add(validator.Object);
        var pwdValidators = new List<PasswordValidator<TUser>>{new()};
        var userManager = new UserManager<TUser>(store, options.Object, new PasswordHasher<TUser>(),
            userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<TUser>>>().Object);
        validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
            .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
        return userManager;
    }

    private async Task SeedDB()
    {

#pragma warning disable CS8601 // Possible null reference assignment.
        var admin = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = _config["Admin_Username"],
            Email = _config["Admin_Password"],
            SecurityStamp = DateTime.UtcNow.ToString(),
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow,
        };

        var defaultTopic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            OwnerId = admin.Id,
            Name = _config["Topic_Default_TopicName"],
            Description = "test",
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
#pragma warning restore CS8601 // Possible null reference assignment.
        _context.Users.Add(admin);  
        _context.Topics.Add(defaultTopic);
        await _context.SaveChangesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    public async Task InitializeAsync(){
        await _fixture.ResetDatabase();
        await SeedDB();
    }
}
