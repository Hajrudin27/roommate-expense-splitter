using Microsoft.OpenApi.Models;
using RoommateSplitter.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using RoommateSplitter.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<RoommateSplitterDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSingleton<IGroupsRepository, InMemoryGroupsRepository>();
builder.Services.AddSingleton<IExpensesRepository, InMemoryExpensesRepository>();
builder.Services.AddSingleton<IPaymentsRepository, InMemoryPaymentsRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Frontend");
    app.MapControllers();
}

app.UseHttpsRedirection();

app.Run();