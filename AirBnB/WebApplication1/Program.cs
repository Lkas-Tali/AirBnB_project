using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>(options =>
        options.UseSqlite(builder.Configuration["ConnectionStrings:WebApiDatabase"]),
    ServiceLifetime.Scoped);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};



app.MapGet("/properties", async (DataContext db) =>
    await db.RentalProperties.ToListAsync());

app.MapPost("/properties", async (RentalProperty property, DataContext db) =>
{
    db.RentalProperties.Add(property);
    await db.SaveChangesAsync();

    return Results.Created($"/properties/{property.ID}", property);
});

app.MapGet("/users", async (DataContext db) =>
    await db.Users.ToListAsync());

app.MapPost("/users", async (User user, DataContext db) =>
{
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/users/{user.ID}", user);
});
app.Run();


record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}