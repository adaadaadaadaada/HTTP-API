using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<BooksDb>(opt => opt.UseInMemoryDatabase("BooksList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/books", async (BooksDb db) =>
    await db.Books.ToListAsync());

app.MapGet("/books/{id}", async (int id, BooksDb db) =>
    await db.Books.FindAsync(id)
        is Books books
            ? Results.Ok(books)
            : Results.NotFound());

app.MapPost("/books", async (Books books, BooksDb db) =>
{
    db.Books.Add(books);
    await db.SaveChangesAsync();

    return Results.Created($"/books/{books.Id}", books);
});

app.MapPut("/books/{id}", async (int id, Books inputBooks, BooksDb db) =>
{
    var books = await db.Books.FindAsync(id);

    if (books is null) return Results.NotFound();

    books.Name = inputBooks.Name;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/books/{id}", async (int id, BooksDb db) =>
{
    if (await db.Books.FindAsync(id) is Books books)
    {
        db.Books.Remove(books);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    return Results.NotFound();
});

app.Run();

public class Books
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

class BooksDb : DbContext
{
    public BooksDb(DbContextOptions<BooksDb> options)
        : base(options) { }

    public DbSet<Books> Books => Set<Books>();
}