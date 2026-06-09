using System.Threading.Tasks;
using MemeApi.library.Services;
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
    public const string SystemServiceOnly = "SystemServiceOnly";
}

public class SystemServiceRequirement : IAuthorizationRequirement { }

public class SystemServiceHandler : AuthorizationHandler<SystemServiceRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SystemServiceRequirement requirement)
    {
        if (context.User.HasClaim("scope", JwtTokenService.ScopeSystemService))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

public class ScopeOrSystemServiceHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ScopeRequirement requirement)
    {
        if (context.User.HasClaim("scope", requirement.Scope) ||
            context.User.HasClaim("scope", JwtTokenService.ScopeSystemService))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
