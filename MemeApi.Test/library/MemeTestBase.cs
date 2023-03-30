using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MemeApi.library;
using MemeApi.library.Mappings;
using MemeApi.library.repositories;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MemeApi.Test.library
{
    public class MemeTestBase
    {
        protected readonly MemeContext _context;
        protected readonly VisualRepository _visualRepository;
        protected readonly TextRepository _textRepository;
        protected readonly MemeRepository _memeRepository;
        protected readonly TopicRepository _topicRepository;
        protected readonly UserRepository _userRepository;
        protected readonly VotableRepository _votableRepository;
        protected readonly IConfiguration _configuration;
        protected IMapper _mapper;
        public MemeTestBase()
        {
            _context = ContextUtils.CreateMemeTestContext();
            _userRepository = new UserRepository(_context, TestUserManager<User>(), new FileSaverStub());
            _visualRepository = new VisualRepository(_context, new FileSaverStub(), new FileRemoverStub());
            _textRepository = new TextRepository(_context);
            _topicRepository = new TopicRepository(_context, _userRepository);
            _memeRepository = new MemeRepository(_context, _visualRepository, _textRepository, _configuration, _topicRepository);
           _votableRepository = new VotableRepository(_context);
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new VotableProfile());
            });
            _mapper = mappingConfig.CreateMapper();
            _configuration = new ConfigurationBuilder()
                .Build();
        }
        public static UserManager<TUser> TestUserManager<TUser>(IUserStore<TUser> store = null) where TUser : class
        {
            store = store ?? new Mock<IUserStore<TUser>>().Object;
            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions();
            idOptions.Lockout.AllowedForNewUsers = false;
            options.Setup(o => o.Value).Returns(idOptions);
            var userValidators = new List<IUserValidator<TUser>>();
            var validator = new Mock<IUserValidator<TUser>>();
            userValidators.Add(validator.Object);
            var pwdValidators = new List<PasswordValidator<TUser>>();
            pwdValidators.Add(new PasswordValidator<TUser>());
            var userManager = new UserManager<TUser>(store, options.Object, new PasswordHasher<TUser>(),
                userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
                new IdentityErrorDescriber(), null,
                new Mock<ILogger<UserManager<TUser>>>().Object);
            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<TUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
            return userManager;
        }
    }
}
