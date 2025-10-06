using Microsoft.EntityFrameworkCore;
using SEP490_BE.BLL.IServices;
using SEP490_BE.BLL.Services;
using SEP490_BE.DAL.IRepositories;
using SEP490_BE.DAL.Repositories;
using SEP490_BE.DAL.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext registration via connection string
builder.Services.AddDbContext<DiamondHealthContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn"))
);

// Users DI
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
