using MemeApi.Models;
using MemeApi.Models.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

public class DocumentAuthorizationHandler :
    AuthorizationHandler<SameAuthorRequirement, Votable>
{
    private readonly IServiceProvider _provider;

    public DocumentAuthorizationHandler(IServiceProvider provider)
    {
        _provider = provider;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   SameAuthorRequirement requirement,
                                                   Votable resource)
    {
        using(var scope = _provider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MemeContext>();
            var topic = dbContext.Topics.Single(topic => topic.Name == resource.Topic.Name);
            if (topic.Moderators.Exists(m => m.Username == context.User.Identity?.Name) 
                || topic.Owner.Username == context.User.Identity?.Name)
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}

public class SameAuthorRequirement : IAuthorizationRequirement { }