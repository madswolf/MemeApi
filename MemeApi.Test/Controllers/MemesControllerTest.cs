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
    public class MemesControllerTest : MemeTestBase
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

            createdMeme.MemeVisual.Should().Be(filename);
        }

        [Fact]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_GettingMemeBottomText_THEN_MemeBottomTextHasProperValues()
        {
            var controller = new MemesController(_memeRepository);

            // given
            var visual = new MemeVisual()
            {
                Filename = "Test"
            };
            var meme = new Meme
            {
                MemeVisual = visual,
            };
            _context.Memes.Add(meme);
            await _context.SaveChangesAsync();

            // When
            var response = await controller.GetMeme(meme.Id);
            var result = response.Result;

            // Then

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var foundMemeText = ((OkObjectResult)result).Value as Meme;

            foundMemeText.MemeVisual.Should().Be(visual);
            foundMemeText.Id.Should().Be(meme.Id);
            foundMemeText.Votes.Should().BeNull();
        }

        public static IFormFile CreateFormFile(int size, string filename)
        {
            var fileStream = new MemoryStream(size);
            var file = new FormFile(fileStream, 0, size, "filestream", filename);
            return file;
        }
    }
}
