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
            await using var context = ContextUtils.CreateMemeTestContext();
            var controller = new MemeTextsController(context);

            // given
            var memeText = "Test";

            // When
            var createTask = controller.CreateMemeBottomText(memeText, memePosition);

            // Then
            var createdMemeText = await ActionResultUtils.ActionResultToValueAndAssertCreated(createTask);
                
            (await context.Texts.CountAsync()).Should().Be(1);
            createdMemeText.Text.Should().Be(memeText);
        }

        [Fact]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_GettingMemeBottomText_THEN_MemeBottomTextHasProperValues()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var controller = new MemeTextsController(context);

            // given
            var memeText = "Test";
            var memePosition = MemeTextPosition.BottomText;

            // When
            var createdMemeBottomText =
                await ActionResultUtils.ActionResultToValueAndAssertCreated(
                    controller.CreateMemeBottomText(memeText, memePosition));

            // Then
            var result = (await controller.GetMemeBottomText(createdMemeBottomText.Id)).Result;

            result.Should().NotBeNull();
            result.Should().BeOfType<OkObjectResult>();
            var foundMemeBottomText = ((OkObjectResult)result).Value as MemeText;

            foundMemeBottomText.Text.Should().Be(memeText);
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

                var createdMemeBottomText =
                    await ActionResultUtils.ActionResultToValueAndAssertCreated(
                        controller.CreateMemeBottomText(memeText, memePosition));

                // When
                await controller.UpdateMemeText(createdMemeBottomText.Id, newMemeText, newMemePosition);

                // Then
                
                var foundMemeBottomText = await
                    ActionResultUtils.ActionResultToValueAndAssertOk(
                        controller.GetMemeBottomText(createdMemeBottomText.Id));

                foundMemeBottomText.Text.Should().Be(newMemeText);
            }
        }

        [Fact]
        public async Task GIVEN_CreatedDummyMemeBottomText_WHEN_Deleting_THEN_MemeBottomTextIsDeleted()
        {
            await using var context = ContextUtils.CreateMemeTestContext();
            var controller = new MemeTextsController(context);

            // given

            var memeText = "Test";
            var memePosition = MemeTextPosition.BottomText;


            var createdMemeBottomText =
                await ActionResultUtils.ActionResultToValueAndAssertCreated(
                    controller.CreateMemeBottomText(memeText, memePosition));

            // When
            var result = await controller.DeleteMemeBottomText(createdMemeBottomText.Id);

            // Then

            result.Should().NotBeNull();
            result.Should().BeOfType<NoContentResult>();
            (await context.Texts.CountAsync()).Should().Be(0);
        }
    }
}
