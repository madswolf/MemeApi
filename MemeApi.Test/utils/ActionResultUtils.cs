using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MemeApi.Test.utils;

internal class ActionResultUtils
{
    public static T? ActionResultToValueAndAssertCreated<T>(ActionResult<T> actionResult) where T : class
    {
        var result = actionResult.Result;
        result.Should().NotBeNull();
        result.Should().BeOfType<CreatedAtActionResult>();
        return (result as CreatedAtActionResult)?.Value as T;
    }
    public static T? ActionResultToValueAndAssertOk<T>(ActionResult<T> actionResult) where T : class
    {
        var result = actionResult.Result;
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        return (result as OkObjectResult)?.Value as T;
    }

    public static async Task ActionResultAssertNoContent(Task<ActionResult> actionResult)

    {
        var result = (await actionResult);
        result.Should().NotBeNull();
        result.Should().BeOfType<NoContentResult>();
    }
}
