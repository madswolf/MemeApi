using System.Threading.Tasks;
using FluentAssertions;
using MemeApi.Controllers;
using MemeApi.Models.Entity;
using MemeApi.Test.utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MemeApi.Test.Controllers
{
    public class MemeTextsControllerTest
    {
        [Theory]
        [InlineData(MemeTextPosition.TopText)]
        [InlineData(MemeTextPosition.BottomText)]
        public async Task GIVEN_DummyMemeText_WHEN_CreatingMemeBottomText_THEN_MemeBottomTextIsCreatedWithProperValues(MemeTextPosition memePosition)
        {
            using (var context = ContextUtils.CreateMemeTestContext())
            {
                var controller = new MemeTextsController(context);

                // given

                var memeText = "Test";

                // When

                var createResult = (await controller.CreateMemeBottomText(memeText, memePosition)).Result;


                // Then
                createResult.Should().NotBeNull();
                createResult.Should().BeOfType<CreatedAtActionResult>();
                var createdMemeText = ((CreatedAtActionResult)createResult).Value as MemeText;

                (await context.Texts.CountAsync()).Should().Be(1);

                createdMemeText.Text.Should().Be(memeText);
            }
        }

        [Fact]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_GettingMemeBottomText_THEN_MemeBottomTextHasProperValues()
        {
            using (var context = ContextUtils.CreateMemeTestContext())
            {
                var controller = new MemeTextsController(context);

                // given

                var memeText = "Test";
                var memePosition = MemeTextPosition.BottomText;

                // When

                var createResult = (await controller.CreateMemeBottomText(memeText, memePosition)).Result;
                var createdMemeBottomtext = ((CreatedAtActionResult)createResult).Value as MemeText;

                // Then
                var result = (await controller.GetMemeBottomText(createdMemeBottomtext.Id)).Result;

                result.Should().NotBeNull();
                result.Should().BeOfType<OkObjectResult>();
                var foundMemeBottomText = ((OkObjectResult)result).Value as MemeText;

                foundMemeBottomText.Text.Should().Be(memeText);
            }
        }

        [Theory]
        [InlineData(MemeTextPosition.TopText, MemeTextPosition.BottomText)]
        [InlineData(MemeTextPosition.BottomText, MemeTextPosition.TopText)]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_Updating_THEN_MemeBottomTextIsUpdatedWithGivenValues(MemeTextPosition memePosition, MemeTextPosition newMemePosition)
        {
            using (var context = ContextUtils.CreateMemeTestContext())
            {
                var controller = new MemeTextsController(context);

                // given

                var memeText = "Test";

                var newMemeText = "Test2";

                var createResult = (await controller.CreateMemeBottomText(memeText, memePosition)).Result;
                var createdMemeBottomText = ((CreatedAtActionResult)createResult).Value as MemeText;

                // When
                await controller.UpdateMemeText(createdMemeBottomText.Id, newMemeText, newMemePosition);

                // Then

                var result = (await controller.GetMemeBottomText(createdMemeBottomText.Id)).Result;
                result.Should().BeOfType<OkObjectResult>();
                var foundMemeBottomText = ((OkObjectResult)result).Value as MemeText;

                foundMemeBottomText.Text.Should().Be(newMemeText);
            }
        }

        [Fact]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_Deleting_THEN_MemeBottomTextIsDeleted()
        {
            using (var context = ContextUtils.CreateMemeTestContext())
            {
                var controller = new MemeTextsController(context);

                // given

                var memeText = "Test";
                var memePosition = MemeTextPosition.BottomText;


                var createResult = (await controller.CreateMemeBottomText(memeText, memePosition)).Result;
                var createdMemeBottomText = ((CreatedAtActionResult)createResult).Value as MemeText;

                // When
                var result = await controller.DeleteMemeBottomText(createdMemeBottomText.Id);

                // Then

                result.Should().NotBeNull();
                result.Should().BeOfType<NoContentResult>();
                (await context.Texts.CountAsync()).Should().Be(0);
            }
        }
    }
}
