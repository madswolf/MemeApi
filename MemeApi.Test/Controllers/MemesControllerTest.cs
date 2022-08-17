using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.library;
using MemeApi.Models.Context;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MemeApi.Test.Controllers
{
    public class MemesControllerTest : ControllerTestBase
    {
        [Fact]
        public async Task GIVEN_Visual_WHEN_CreatingMeme_THEN_MemeIsCreated()
        {
            var controller = new MemesController(_memeRepository);

            // given
            var filename = "test.png";

            var memeCreationDTO = new MemeCreationDTO()
            {
                VisualFile = CreateFormFile(5, filename),
            };

            // When
            var creationTask = controller.PostMeme(memeCreationDTO);

            // Then
            var createdMeme = await ActionResultUtils.ActionResultToValueAndAssertCreated(creationTask);

            (await _context.Memes.CountAsync()).Should().Be(1);
            (await _context.Visuals.CountAsync()).Should().Be(1);

            createdMeme.MemeVisual.Filename.Should().Be(filename);
        }

        public static IFormFile CreateFormFile(int size, string filename)
        {
            var fileStream = new MemoryStream(size);
            var file = new FormFile(fileStream, 0, size, "filestream", filename);
            return file;
        }
    }
}
