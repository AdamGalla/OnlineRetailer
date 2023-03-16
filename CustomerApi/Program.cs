using Microsoft.EntityFrameworkCore;
using CustomerApi.Data;
using CustomerApi.Models;
using CustomerApi.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

string cloudAMQPConnectionString =

builder.Services.AddDbContext<CustomerApiContext>(opt => opt.UseInMemoryDatabase("CustomerDb"));

// Register repositories for dependency injection
builder.Services.AddScoped<IRepository<Customer>, CustomerRepository>();

// Register database initializer for dependency injection
builder.Services.AddTransient<IDbInitializer, DbInitializer>();

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(options =>
{
    options.AllowAnyOrigin();
    options.AllowAnyMethod();
    options.AllowAnyHeader();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetService<CustomerApiContext>();
    var dbInitializer = services.GetService<IDbInitializer>();
    if(dbContext != null && dbInitializer != null)
    {
        dbInitializer.Initialize(dbContext);
    }
}

Task.Factory.StartNew(() =>
    new MessageListener(app.Services, cloudAMQPConnectionString).Start());

app.UseAuthorization();

app.MapControllers();

app.Run();
