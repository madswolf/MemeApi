using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace MemeApi.library.Authorization;

public class ScopeRequirement : IAuthorizationRequirement
{
    public string Scope { get; }
    public ScopeRequirement(string scope) => Scope = scope;
}

public static class Policies
{
    public const string TransferDubloons = "TransferDubloons";
    public const string SubmitPlace = "SubmitPlace";
}

public class ScopeOrBotSecretHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
    {
        if (context.User.HasClaim("scope", requirement.Scope) ||
            context.User.HasClaim("client_type", "discord_bot"))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
