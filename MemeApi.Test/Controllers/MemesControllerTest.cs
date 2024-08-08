using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.Models.DTO;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace MemeApi.Test.Controllers;

public class MemesControllerTest : MemeTestBase
{
    public MemesControllerTest(IntegrationTestFactory databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task GIVEN_Visual_WHEN_CreatingMeme_THEN_MemeIsCreated()
    {
        var controller = new MemesController(_memeRepository, _memeRenderingService);

        // given
        var filename = "test.png";

        var memeCreationDTO = new MemeCreationDTO()
        {
            VisualFile = CreateFormFile(5, filename),
        };

        // When
        var creationTask = await controller.PostMeme(memeCreationDTO);

        // Then
        var createdMeme = ActionResultUtils.ActionResultToValueAndAssertCreated(creationTask);

        (await _context.Memes.CountAsync()).Should().Be(1);
        (await _context.Visuals.CountAsync()).Should().Be(1);

        createdMeme?.MemeVisual.Should().Be(filename);
    }

    [Fact]
    public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_GettingMemeBottomText_THEN_MemeBottomTextHasProperValues()
    {
        var controller = new MemesController(_memeRepository, _memeRenderingService);

        // given
        var visual = new MemeVisual()
        {
            Id = Guid.NewGuid().ToString(),
            Filename = "Test",
        };
        
        var meme = new Meme
        {
            Id = Guid.NewGuid().ToString(),
            Visual = visual,
            VisualId = visual.Id,
        };

        _context.Visuals.Add(visual);
        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();

        // When
        var response = await controller.GetMeme(meme.Id);
        var result = response.Result;

        // Then

        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var foundMemeText = (result as OkObjectResult)?.Value as MemeDTO;

        foundMemeText?.MemeVisual.Should().Be(visual.Filename);
        foundMemeText?.Id.Should().Be(meme.Id);
    }

    [Fact]
    public async Task Test_Votable_Deletion_With_Connection_Reset()
    {
        var controller = new MemesController(_memeRepository, _memeRenderingService);
        var controller2 = new TextsController(_textRepository);

        // given
        var filename = "test.png";

        var memeCreationDTO = new MemeCreationDTO()
        {
            VisualFile = CreateFormFile(5, filename),
            TopText = "lol",
            BottomText = "lol",
        };

        // When
        var creationTask = await controller.PostMeme(memeCreationDTO);

        // Then
        var createdMeme = ActionResultUtils.ActionResultToValueAndAssertCreated(creationTask);

        (await _context.Memes.CountAsync()).Should().Be(1);
        (await _context.Visuals.CountAsync()).Should().Be(1);

        createdMeme?.MemeVisual.Should().Be(filename);

        ResetConnection();
        controller = new MemesController(_memeRepository, _memeRenderingService);
        controller2 = new TextsController(_textRepository);

        //with the connection reset, this fails
        var deleteTask = await controller2.DeleteMemeText(createdMeme?.BottomText?.Id);
        deleteTask.Should().NotBeNull();
        deleteTask.Should().BeOfType<NoContentResult>();

        ResetConnection();
        controller = new MemesController(_memeRepository, _memeRenderingService);
        controller2 = new TextsController(_textRepository);

        //with the connection reset, this fails
        var deleteTask2 = await controller2.DeleteMemeText(createdMeme?.Toptext?.Id);
        deleteTask2.Should().NotBeNull();
        deleteTask2.Should().BeOfType<NoContentResult>();
        await controller.GetMeme(createdMeme.Id);
    }

    [Fact]
    public async Task Test_Votable_Deletion_Without_Connection_Reset()
    {
        var controller = new MemesController(_memeRepository, _memeRenderingService);
        var controller2 = new TextsController(_textRepository);

        // given
        var filename = "test.png";

        var memeCreationDTO = new MemeCreationDTO()
        {
            VisualFile = CreateFormFile(5, filename),
            TopText = "lol",
            BottomText = "lol",
        };

        // When
        var creationTask = await controller.PostMeme(memeCreationDTO);

        // Then
        var createdMeme = ActionResultUtils.ActionResultToValueAndAssertCreated(creationTask);

        (await _context.Memes.CountAsync()).Should().Be(1);
        (await _context.Visuals.CountAsync()).Should().Be(1);

        createdMeme?.MemeVisual.Should().Be(filename);

        //ResetConnection();
        //controller = new MemesController(_memeRepository, _memeRenderingService);
        //controller2 = new TextsController(_textRepository);

        //without resetting the connection, this succeeds
        var deleteTask = await controller2.DeleteMemeText(createdMeme?.BottomText?.Id);
        deleteTask.Should().NotBeNull();
        deleteTask.Should().BeOfType<NoContentResult>();

        //ResetConnection();
        //controller = new MemesController(_memeRepository, _memeRenderingService);
        //controller2 = new TextsController(_textRepository);

        //without resetting the connection, this succeeds
        var deleteTask2 = await controller2.DeleteMemeText(createdMeme?.Toptext?.Id);
        deleteTask2.Should().NotBeNull();
        deleteTask2.Should().BeOfType<NoContentResult>();
    }


    public static IFormFile CreateFormFile(int size, string filename)
    {
        var fileStream = new MemoryStream(size);
        var file = new FormFile(fileStream, 0, size, "fileStream", filename);
        return file;
    }
}
