using Book.v2.Data;
using Book.v2.Repositories.Implementations;
using Book.v2.Repositories.Interfaces;
using Book.v2.Services.External;
using Book.v2.Services.Implementations;
using Book.v2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

using System.Text.Json.Serialization; // Added for ReferenceHandler

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContextDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookPageRepository, BookPageRepository>();
builder.Services.AddScoped<IReadingListRepository, ReadingListRepository>();
builder.Services.AddScoped<IReadingProgressRepository, ReadingProgressRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IReadingListService, ReadingListService>();
builder.Services.AddScoped<IReadingProgressService, ReadingProgressService>();
builder.Services.AddScoped<IRecommendationService, ContentBasedRecommendationService>();

builder.Services.AddHttpClient<GoogleBooksService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ContextDb>();
    var googleBooksService = scope.ServiceProvider.GetRequiredService<GoogleBooksService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    await DbSeeder.SeedAsync(context, googleBooksService, logger);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

app.Use(async (context, next) =>
{
    context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
    context.Response.Headers.Append("Pragma", "no-cache");
    context.Response.Headers.Append("Expires", "0");
    await next();
});

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
        ctx.Context.Response.Headers.Append("Expires", "0");
    }
});

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
