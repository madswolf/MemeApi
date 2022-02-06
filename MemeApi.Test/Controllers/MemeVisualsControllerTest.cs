using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.library;
using MemeApi.Models.Entity;
using MemeApi.Test.library;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MemeApi.Test.Controllers
{
    public class MemeVisualsControllerTest
    {
        [Fact]
        public async Task GIVEN_DummyFile_WHEN_CreatingMemeVisual_THEN_MemeVisualIsCreatedWithProperValues()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var controller = new MemeVisualsController(context, new FileSaverStub(), new FileRemover());

            // given
            var fileStream = new MemoryStream(5);
            var file = new FormFile(fileStream, 0, 5, "filestream", "test");

            // When
            var createResult = (await controller.PostMemeVisual(file)).Result;

            // Then
            createResult.Should().NotBeNull();
            createResult.Should().BeOfType<CreatedAtActionResult>();
            var createdMemeVisual = ((CreatedAtActionResult)createResult).Value as MemeVisual;

            (await context.Visuals.CountAsync()).Should().Be(1);

            createdMemeVisual.Filename.Should().Be(file.FileName);
        }

        [Fact]
        public async Task GIVEN_TwoDummyFilesWithSameName_WHEN_CreatingMemeVisuals_THEN_SecondMemeVisualIsHasDifferentName()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var controller = new MemeVisualsController(context, new FileSaverStub(), new FileRemover());

            // given
            var fileStream = new MemoryStream(5);
            var file = new FormFile(fileStream, 0, 5, "filestream", "test");
            var file2 = new FormFile(fileStream, 0, 5, "filestream", "test");

            // When
            var createResult = (await controller.PostMemeVisual(file)).Result;
            var createResult2 = (await controller.PostMemeVisual(file2)).Result;

            // Then

            var createdMemeVisual = ((CreatedAtActionResult)createResult).Value as MemeVisual;
            var createdMemeVisual2 = ((CreatedAtActionResult)createResult2).Value as MemeVisual;

            (await context.Visuals.CountAsync()).Should().Be(2);

            createdMemeVisual.Filename.Should().NotBe(createdMemeVisual2.Filename);
            createdMemeVisual2.Filename.Should().NotBe(file2.FileName);
        }

        [Fact]
        public async Task GIVEN_LargeDummyFile_WHEN_CreatingMemeVisual_THEN_MemeVisualIsNotCreated()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var controller = new MemeVisualsController(context, new FileSaverStub(), new FileRemover());

            // given
            var fileStream = new MemoryStream(5001);
            var file = new FormFile(fileStream, 0, 5001, "filestream", "test");

            // When
            var createResult = (await controller.PostMemeVisual(file)).Result;

            // Then
            createResult.Should().NotBeNull();
            createResult.Should().BeOfType<StatusCodeResult>();
            ((StatusCodeResult)createResult).StatusCode.Should().Be(413);

            (await context.Visuals.CountAsync()).Should().Be(0);
        }



        [Fact]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_Deleting_THEN_MemeVisualIsDeleted()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var controller = new MemeVisualsController(context, new FileSaver(), new FileRemoverStub());

            // given
            var fileStream = new MemoryStream(5);
            var file = new FormFile(fileStream, 0, 5, "filestream", "test");

            var createResult = (await controller.PostMemeVisual(file)).Result;
            var createdMemeVisual = ((CreatedAtActionResult)createResult).Value as MemeVisual;
            // When
            var result = await controller.DeleteMemeVisual(createdMemeVisual.Id);

            // Then
            result.Should().NotBeNull();
            result.Should().BeOfType<NoContentResult>();
            (await context.Users.CountAsync()).Should().Be(0);
        }
    }
}
