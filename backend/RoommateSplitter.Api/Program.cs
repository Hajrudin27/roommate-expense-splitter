using Microsoft.OpenApi.Models;
using RoommateSplitter.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using RoommateSplitter.Infrastructure.Persistence;
using RoommateSplitter.Infrastructure.Repositories;


var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<RoommateSplitterDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddScoped<IGroupsRepository, EfGroupsRepository>();
builder.Services.AddScoped<IExpensesRepository, EfExpensesRepository>();
builder.Services.AddScoped<IPaymentsRepository, EfPaymentsRepository>();

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


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Frontend");
    app.MapControllers();
}

app.UseHttpsRedirection();

app.Run();