using MemeApi.Controllers;
using MemeApi.library;
using MemeApi.library.repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Xunit;

namespace MemeApi.Test.library;

[Collection(nameof(DatabaseTestCollection))]
public class MemeTestBase : IAsyncLifetime
{
    protected MemeContext _context;
    protected VisualRepository _visualRepository;
    protected TextRepository _textRepository;
    protected MemeRepository _memeRepository;
    protected TopicRepository _topicRepository;
    protected UserRepository _userRepository;
    protected VotableRepository _votableRepository;
    protected MemeApiSettings _settings;
    protected IntegrationTestFactory _fixture;
    protected UserManager<User> _userManager;
    protected SignInManager<User> _signInManager;

    protected MemeRenderingService _memeRenderingService;

    private readonly Dictionary<string, string?> _config = new()
    {
                {"Topic_Default_TopicName", "Rotte-Grotte"},
                {"Topic_MemeOfTheDay_Topicname", "MemeOfTheDay"},
                {"Bot_Secret", "lol"},
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
        _visualRepository = new VisualRepository(_context, new FileSaverStub(), new FileRemoverStub(), _topicRepository, _userRepository);
        _textRepository = new TextRepository(_context, _topicRepository, _userRepository);
        _memeRepository = new MemeRepository(_context, _visualRepository, _textRepository, _topicRepository, _settings, _userRepository);
        _votableRepository = new VotableRepository(_context, _settings);

        _userManager = new FakeUserManager();
        _signInManager = new FakeSignInManager();
    }

    public static HttpContext GetMockedHttpContext()
    {
        var context = new DefaultHttpContext();
        var mockSession = new Mock<ISession>();
        context.RequestServices = new Mock<IServiceProvider>().Object;
        context.Session = mockSession.Object;
        return context;
    }

    protected static void SetUserNameIdentifier(UsersController controller, string userId)
    {
        var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
            };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockPrincipal = new Mock<IPrincipal>();
        mockPrincipal.Setup(x => x.Identity).Returns(identity);
        mockPrincipal.Setup(x => x.IsInRole(It.IsAny<string>())).Returns(true);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(m => m.User).Returns(claimsPrincipal);
        controller.ControllerContext.HttpContext = mockHttpContext.Object;
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
        var pwdValidators = new List<PasswordValidator<TUser>> { new() };
        var userManager = new UserManager<TUser>(store, options.Object, new PasswordHasher<TUser>(),
            userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(), new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<TUser>>>().Object);
        validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
            .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
        return userManager;
    }

    public static SignInManager<TUser> TestSignInManager<TUser>(UserManager<TUser> userManager) where TUser : class
    {
        var signInManager = new Mock<SignInManager<TUser>>(
                    userManager,
                    new HttpContextAccessor(),
                    new Mock<IUserClaimsPrincipalFactory<TUser>>().Object,
                    new Mock<IOptions<IdentityOptions>>().Object,
                    new Mock<ILogger<SignInManager<TUser>>>().Object,
                    new Mock<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>().Object
                  );

        return signInManager.Object;
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

        var memeOfTheDayTopic = new Topic
        {
            Id = Guid.NewGuid().ToString(),
            OwnerId = admin.Id,
            Name = _config["Topic_MemeOfTheDay_Topicname"],
            Description = "test",
            CreatedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow
        };
#pragma warning restore CS8601 // Possible null reference assignment.
        _context.Users.Add(admin);
        _context.Topics.Add(defaultTopic);
        _context.Topics.Add(memeOfTheDayTopic);
        await _context.SaveChangesAsync();
    }

    public void ResetConnection()
    {
        _fixture.ResetConnection();
        _memeRenderingService = new MemeRenderingService(_settings, new WebFileLoader(_settings));
        _context = _fixture.Db;

        _userRepository = new UserRepository(_context, TestUserManager<User>(), new FileSaverStub());
        _topicRepository = new TopicRepository(_context, _userRepository, _settings);
        _visualRepository = new VisualRepository(_context, new FileSaverStub(), new FileRemoverStub(), _topicRepository, _userRepository);
        _textRepository = new TextRepository(_context, _topicRepository, _userRepository);
        _memeRepository = new MemeRepository(_context, _visualRepository, _textRepository, _topicRepository, _settings, _userRepository);
        _votableRepository = new VotableRepository(_context, _settings);
    }

    public async Task DisposeAsync() => await _fixture.ResetDatabase();

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabase();
        await SeedDB();
    }

    public class FakeUserManager : UserManager<User>
    {
        public FakeUserManager()
            : base(new Mock<IUserStore<User>>().Object,
              new Mock<IOptions<IdentityOptions>>().Object,
              new Mock<IPasswordHasher<User>>().Object,
              new IUserValidator<User>[0],
              new IPasswordValidator<User>[0],
              new Mock<ILookupNormalizer>().Object,
              new Mock<IdentityErrorDescriber>().Object,
              new Mock<IServiceProvider>().Object,
              new Mock<ILogger<UserManager<User>>>().Object)
        { }

        public override Task<IdentityResult> CreateAsync(User user, string password)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<IdentityResult> AddToRoleAsync(User user, string role)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }
    public class FakeSignInManager : SignInManager<User>
    {
        public FakeSignInManager()
                : base(new FakeUserManager(),
                     new Mock<IHttpContextAccessor>().Object,
                     new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                     new Mock<IOptions<IdentityOptions>>().Object,
                     new Mock<ILogger<SignInManager<User>>>().Object,
                     new Mock<IAuthenticationSchemeProvider>().Object,
                     new Mock<IUserConfirmation<User>>().Object)
        { }
    }
}
