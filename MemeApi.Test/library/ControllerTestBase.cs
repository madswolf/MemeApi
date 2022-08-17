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

namespace MemeApi.Test.library
{
    public class ControllerTestBase
    {
        protected readonly MemeContext _context;
        protected readonly VisualRepository _visualRepository;
        protected readonly TextRepository _textRepository;
        protected IMapper _mapper;
        public ControllerTestBase()
        {
            _context = ContextUtils.CreateMemeTestContext();
            _visualRepository = new VisualRepository(_context, new FileSaverStub(), new FileRemoverStub());
            _textRepository = new TextRepository(_context);

            var mappingConfig = new MapperConfiguration(mc =>
            { 
                mc.AddProfile(new VotableProfile());
            });
            _mapper = mappingConfig.CreateMapper();
        }
    }
}
