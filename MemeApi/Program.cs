using System;
using System.IO;
using System.Net;
using System.Reflection;
using MemeApi.library;
using MemeApi.library.repositories;
using MemeApi.library.Repositories;
using MemeApi.library.Services;
using MemeApi.library.Services.Files;
using MemeApi.MIddleware;
using MemeApi.Models.Context;
using MemeApi.Models.Entity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

var appBuilder = WebApplication.CreateBuilder(args);
var services = appBuilder.Services;

services.AddControllers();
services.AddDbContext<MemeContext>(options => {
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
    options.UseNpgsql(connectionString);
});
services.AddControllers().AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore
);

services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<MemeContext>();

services.Configure<IdentityOptions>(options =>
{
    options.Lockout.MaxFailedAccessAttempts = 1000;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
});

services.AddAutoMapper(typeof(Program));

services.AddCors();
services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Meme API",
        Description = "An ASP.NET Core Web API for creating memes",
        Contact = new OpenApiContact
        {
            Name = "Contact information",
            Email = "Contact@mads.monster"
        },
    });
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    services.AddScoped<IFileRemover, FileRemover>();
    services.AddScoped<IFileSaver, FileSaver>();
} else {
    services.AddScoped<IFileSaver, S3FileStorageClient>();
    services.AddScoped<IFileRemover, S3FileStorageClient>();
}

services.AddScoped<IFileLoader, WebFileLoader>();
services.AddScoped<IMailSender, MailSender>();
services.AddScoped<IMemeRenderingService, MemeRenderingService>();
services.AddScoped<MailSender>();
services.AddScoped<MemeRenderingService>();
services.AddScoped<DiscordWebhookSender>();

services.AddScoped<UserRepository>();
services.AddScoped<MemeRepository>();
services.AddScoped<VisualRepository>();
services.AddScoped<TextRepository>();
services.AddScoped<VotableRepository>();
services.AddScoped<TopicRepository>();
services.AddScoped<MemePlaceRepository>();
services.AddScoped<LotteryRepository>();

services.AddSingleton<MemeApiSettings>();


services.AddScoped<IMemeOfTheDayService, MemeOfTheDayService>();
services.AddHostedService<ConsumeScopedServiceHostedService>();

services.Configure<ForwardedHeadersOptions>(options =>
{
    options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
});

var app = appBuilder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

var factory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var serviceScope = factory.CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<MemeContext>();
    if (context.Database.EnsureCreated())
    {
        context.SaveChanges();
    }
}
app.UseRequestLocalization("en-US");
app.UseMiddleware<SwaggerAuthenticationMiddleware>();
app.UseMiddleware<ExternalUserMiddleware>();
app.UseSwaggerUI();
app.UseSwagger();
app.UseRouting();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseAuthentication();
app.UseCors(builder =>
{
    builder
        .SetIsOriginAllowed(_ => true)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
});
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
