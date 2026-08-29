using Microsoft.EntityFrameworkCore;
using Mephi.Cybernetics.Omelea.ReportsConstructor;
using Mephi.Cybernetics.Omelea.ReportsConstructor.Models;
using Mephi.Cybernetics.Race.ReportsDescriptionLanguage;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OmeleaReportsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IApprListRepository, ApprListRepository>();
builder.Services.AddScoped<IExpressionCompiler, ExpressionCompiler>();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

