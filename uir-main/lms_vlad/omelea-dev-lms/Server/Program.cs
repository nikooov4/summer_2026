using System.Text;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Mephi.Cyber.AuthService.Core.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// var builder = WebApplication.CreateBuilder(args);
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // Explicitly set the path to appsettings.json if needed
    ApplicationName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name
});
builder.Configuration.AddJsonFile("Properties/appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Read connection string from configuration (appsettings.json or environment variables)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured. Please set it in appsettings.json or environment variables.");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseNpgsql(connectionString));

// Get JWT secret from configuration
var jwtSecret = builder.Configuration["JwtSettings:AdminSecret"] 
    ?? throw new InvalidOperationException("JwtSettings:AdminSecret is not configured. Please set it in appsettings.json or environment variables.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        };
    });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();



app.UseBlazorFrameworkFiles();
app.UseStaticFiles();


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

// Initialize default admin if configured
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    
    var defaultUsername = configuration["DefaultAdmin:Username"];
    var defaultPassword = configuration["DefaultAdmin:Password"];
    var defaultEmail = configuration["DefaultAdmin:Email"];
    
    if (!string.IsNullOrWhiteSpace(defaultUsername) && !string.IsNullOrWhiteSpace(defaultPassword))
    {
        if (!db.Admins.Any(a => a.Name == defaultUsername))
        {
            var authAdmin = new Mephi.Cyber.AuthService.Core.AuthAdmin(db);
            try
            {
                authAdmin.Create(defaultUsername, defaultPassword, defaultEmail ?? string.Empty);
                Console.WriteLine($"Default admin '{defaultUsername}' created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create default admin: {ex.Message}");
            }
        }
    }

    var lmsProjectName = configuration["SeedData:LmsProject:Name"];
    if (!string.IsNullOrWhiteSpace(lmsProjectName) && !db.Projects.Any(p => p.Name == lmsProjectName))
    {
        var authProject = new Mephi.Cyber.AuthService.Core.AuthProject(db);
        try
        {
            var secrets = authProject.Create(lmsProjectName);
            Console.WriteLine($"Seed project '{lmsProjectName}' created.");
            Console.WriteLine($"SharedSecret: {secrets["sharedSecret"]}");
            Console.WriteLine($"AuthSecret: {secrets["authSecret"]}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create seed project '{lmsProjectName}': {ex.Message}");
        }
    }
}

app.Run();

