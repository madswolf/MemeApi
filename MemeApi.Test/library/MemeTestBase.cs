using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using MemeApi.Controllers;
using MemeApi.library;
using MemeApi.library.Authentication;
using MemeApi.library.Extensions;
using MemeApi.library.repositories;
using MemeApi.library.Services.Files;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Memes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
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
        _votableRepository = new VotableRepository(_context, _settings, new WebFileLoader(_settings));

        _userManager = new FakeUserManager();
        _signInManager = new FakeSignInManager();
    }

    public static HttpContext GetMockedHttpContext(MemeApiSettings? settings = null)
    {
        var context = new DefaultHttpContext();
        var mockSession = new Mock<ISession>();
        context.Session = mockSession.Object;

        var mockAuthService = new Mock<IAuthenticationService>();
        mockAuthService
            .Setup(s => s.AuthenticateAsync(It.IsAny<HttpContext>(), SystemServiceAuthenticationHandler.SchemeName))
            .Returns<HttpContext, string>((ctx, scheme) =>
            {
                if (settings != null &&
                    ctx.Request.Headers.TryGetValue("Bot_Secret", out var secret) &&
                    secret == settings.GetBotSecret())
                {
                    var claims = new[] { new Claim("client_type", "discord_bot") };
                    var identity = new ClaimsIdentity(claims, scheme);
                    var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), scheme);
                    return Task.FromResult(AuthenticateResult.Success(ticket));
                }
                return Task.FromResult(AuthenticateResult.NoResult());
            });

        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider
            .Setup(s => s.GetService(typeof(IAuthenticationService)))
            .Returns(mockAuthService.Object);

        context.RequestServices = mockServiceProvider.Object;
        return context;
    }

    protected static void SetUserNameIdentifier(UsersController controller, string userId)
    {
        var claims = new List<Claim>
        {
                new Claim(ClaimTypes.NameIdentifier, userId.ExternalUserIdToGuid()),
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
                    new Mock<IAuthenticationSchemeProvider>().Object
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
        _votableRepository = new VotableRepository(_context, _settings, new WebFileLoader(_settings));
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

    public static IFormFile CreateFormFile(int size, string filename)
    {
        var pngBytes = new byte[]
        {
            // PNG signature
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            // IHDR chunk (length=13)
            0x00, 0x00, 0x00, 0x0D,
            0x49, 0x48, 0x44, 0x52, // "IHDR"
            0x00, 0x00, 0x00, 0x01, // width=1
            0x00, 0x00, 0x00, 0x01, // height=1
            0x08, 0x02,             // bit depth=8, color type=2 (RGB)
            0x00, 0x00, 0x00,       // compression, filter, interlace
            0x90, 0x77, 0x53, 0xDE, // IHDR CRC
            // IDAT chunk
            0x00, 0x00, 0x00, 0x0C,
            0x49, 0x44, 0x41, 0x54, // "IDAT"
            0x08, 0xD7, 0x63, 0xF8, 0xCF, 0xC0, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01,
            0xE2, 0x21, 0xBC, 0x33, // IDAT CRC
            // IEND chunk
            0x00, 0x00, 0x00, 0x00,
            0x49, 0x45, 0x4E, 0x44, // "IEND"
            0xAE, 0x42, 0x60, 0x82  // IEND CRC
        };

        var totalSize = pngBytes.Length + size;
        var bytes = new byte[totalSize];
        Array.Copy(pngBytes, bytes, pngBytes.Length);

        var random = new Random();
        random.NextBytes(new Span<byte>(bytes, pngBytes.Length, size));

        var fileStream = new MemoryStream(bytes);

        var file = new FormFile(fileStream, 0, totalSize, "fileStream", filename)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        return file;
    }
}
