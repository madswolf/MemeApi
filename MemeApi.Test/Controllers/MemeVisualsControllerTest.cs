﻿using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.library.Extensions;
using MemeApi.Models.DTO;
using MemeApi.Models.DTO.Memes;
using MemeApi.Models.Entity;
using MemeApi.Models.Entity.Memes;
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

public class MemeVisualsControllerTest : MemeTestBase
{
    public MemeVisualsControllerTest(IntegrationTestFactory databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task GIVEN_DummyFile_WHEN_CreatingMemeVisual_THEN_MemeVisualIsCreatedWithProperValues()
    {
        var controller = new VisualsController(_visualRepository, _settings);
        controller.ControllerContext.HttpContext = GetMockedHttpContext();

        // given
        var visualCreationDTO = new VisualCreationDTO
        {
            File = new FormFile(new MemoryStream(5), 0, 5, "fileStream", "test"),
        };

        // When
        var createTask = await controller.PostMemeVisual(visualCreationDTO);

        // Then
        var createdMemeVisual = ActionResultUtils.ActionResultToValueAndAssertCreated(createTask);

        (await _context.Visuals.CountAsync()).Should().Be(1);
        createdMemeVisual?.Filename.Should().Be(visualCreationDTO.File.FileName);
    }

    [Fact]
    public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_GettingMemeBottomText_THEN_MemeBottomTextHasProperValues()
    {
        var controller = new VisualsController(_visualRepository, _settings);

        // given
        var visual = new MemeVisual()
        {
            Id = Guid.NewGuid().ToString(),
            Filename = "Test"
        };
        _context.Visuals.Add(visual);
        await _context.SaveChangesAsync();

        // When
        var expected = visual.ToRandomComponentDTO("test");
        var result = (await controller.GetMemeVisual(visual.Id)).Result;

        // Then

        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var foundMemeText = (result as OkObjectResult)?.Value as RandomComponentDTO;

        foundMemeText?.data.Should().Be(expected.data);
        foundMemeText?.id.Should().Be(expected.id);
        foundMemeText?.votes.Should().Be(expected.votes);
    }

    [Fact]
    public async Task GIVEN_TwoDummyFilesWithSameName_WHEN_CreatingMemeVisuals_THEN_SecondMemeVisualIsHasDifferentName()
    {
        var controller = new VisualsController(_visualRepository, _settings);
        controller.ControllerContext.HttpContext = GetMockedHttpContext();

        // given
        var visualCreationDTO = new VisualCreationDTO
        {
            File = new FormFile(new MemoryStream(5), 0, 5, "fileStream", "filename"),
        };

        // When
        var createTask = await controller.PostMemeVisual(visualCreationDTO);
        var createTask2 = await controller.PostMemeVisual(visualCreationDTO);

        // Then
        var createdMemeVisual = ActionResultUtils.ActionResultToValueAndAssertCreated(createTask);
        var createdMemeVisual2 = ActionResultUtils.ActionResultToValueAndAssertCreated(createTask2);

        (await _context.Visuals.CountAsync()).Should().Be(2);
        createdMemeVisual?.Filename.Should().Be(visualCreationDTO.File.FileName);
        createdMemeVisual2?.Filename.Should().NotBe(visualCreationDTO.File.FileName);
    }

    [Fact]
    public async Task GIVEN_LargeDummyFile_WHEN_CreatingMemeVisual_THEN_MemeVisualIsNotCreated()
    {
        var controller = new VisualsController(_visualRepository, _settings);

        // given
        var visualCreationDTO = new VisualCreationDTO
        {
            File = new FormFile(new MemoryStream(5000001), 0, 5000001, "fileStream", "filename"),
        };

        // When
        var createResult = (await controller.PostMemeVisual(visualCreationDTO)).Result;

        // Then
        createResult.Should().NotBeNull();
        createResult.Should().BeOfType<StatusCodeResult>();
        (createResult as StatusCodeResult)?.StatusCode.Should().Be(413);

        (await _context.Visuals.CountAsync()).Should().Be(0);
    }



    [Fact]
    public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_Deleting_THEN_MemeVisualIsDeleted()
    {
        var controller = new VisualsController(_visualRepository, _settings);

        // given
        var memeVisual = new MemeVisual()
        {
            Id = Guid.NewGuid().ToString(),
            Filename = "Test"
        };
        _context.Visuals.Add(memeVisual);


        // When
        var result = await controller.DeleteMemeVisual(memeVisual.Id);

        // Then
        result.Should().NotBeNull();
        result.Should().BeOfType<NoContentResult>();
        (await _context.Visuals.CountAsync()).Should().Be(0);
    }
}
