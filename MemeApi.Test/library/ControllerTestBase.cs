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
using MemeApi.Test.utils;
using Microsoft.Extensions.Configuration;

namespace MemeApi.Test.library
{
    public class ControllerTestBase
    {
        protected readonly MemeContext _context;
        protected readonly VisualRepository _visualRepository;
        protected readonly TextRepository _textRepository;
        protected readonly MemeRepository _memeRepository;
        protected readonly IConfiguration _configuration;
        protected IMapper _mapper;
        public ControllerTestBase()
        {
            _context = ContextUtils.CreateMemeTestContext();
            _visualRepository = new VisualRepository(_context, new FileSaverStub(), new FileRemoverStub());
            _textRepository = new TextRepository(_context);
            _memeRepository = new MemeRepository(_context, _visualRepository, _textRepository);
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new VotableProfile());
            });
            _mapper = mappingConfig.CreateMapper();
            _configuration = new ConfigurationBuilder()
                .Build();
        }
    }
}
