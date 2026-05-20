using FC26Competition.Components;
using FC26Competition.Data;
using FC26Competition.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure SQL Server database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<FC26CompetitionContext>(options =>
    options.UseSqlServer(connectionString));

// Register services
builder.Services.AddScoped<CompetitionService>();

var app = builder.Build();

// Ensure database is created (migrations will be used in production)
using (var scope = app.Services.CreateScope())
{
    var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<FC26CompetitionContext>>();
    await using var context = await contextFactory.CreateDbContextAsync();
    
    // For development: auto-create database
    // For production: use migrations
    if (app.Environment.IsDevelopment())
    {
        await context.Database.EnsureCreatedAsync();
    }
    else
    {
        // Apply any pending migrations automatically
        await context.Database.MigrateAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
