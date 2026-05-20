using Microsoft.EntityFrameworkCore;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

// --- שלב 1: הגדרת Services (לפני ה-Build!) ---

// א. הגדרת ה-Database
var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 0))));

// ב. הגדרת CORS - חייב להיות לפני ה-Build
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ג. הגדרת Swagger - חייב להיות לפני ה-Build
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---  - הופכים את ה-builder ל-app ---
var app = builder.Build();

// --- שלב 2: הגדרת Middleware (אחרי ה-Build!) ---

app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();

// --- שלב 3: מיפוי ה-Routes ---

app.MapGet("/items", async (ToDoDbContext db) =>
    await db.Items.ToListAsync());

app.MapPost("/items", async (ToDoDbContext db, Item item) =>
{
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

app.MapPut("/items/{id}", async (ToDoDbContext db, int id, Item inputItem) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    item.Name = inputItem.Name;
    item.IsComplete = inputItem.IsComplete;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/items/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
