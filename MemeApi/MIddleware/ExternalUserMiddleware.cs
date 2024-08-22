using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

public class ExternalUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _headerName = "ExternalUserId";

    public ExternalUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(_headerName, out var userIdHeaderValue))
        {
            var first = userIdHeaderValue.FirstOrDefault();
            if(first != null)
            {
                var claims = new[] { new Claim(ClaimTypes.NameIdentifier, first) };
                var identity = new ClaimsIdentity(claims, "Custom");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await _next(context);
    }
}