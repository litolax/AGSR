using HospitalAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Patient Management API",
        Description =
            "An ASP.NET Core 6 API for managing patient records, supporting CRUD operations and search functionality."
    });
});
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Patient Management API v1"); });

app.UseHttpsRedirection();

app.MapGet("/patients", async (ApplicationDbContext db) =>
        await db.Patients
            .Include(p => p.Name)
            .ToListAsync())
    .WithDisplayName("Get all patients")
    .WithName("GetAllPatients")
    .Produces<List<Patient>>(200)
    .Produces(404);

app.MapGet("/patients/{id}", async (int id, ApplicationDbContext db) =>
        await db.Patients
            .Include(p => p.Name)
            .FirstOrDefaultAsync(p => p.Id == id) is { } patient
            ? Results.Ok(patient)
            : Results.NotFound())
    .WithDisplayName("Get patient by id")
    .WithName("GetPatientById")
    .Produces<Patient>(200)
    .Produces(404);

app.MapPost("/patients", async (Patient patient, ApplicationDbContext db) =>
    {
        db.Patients.Add(patient);

        await db.SaveChangesAsync();
        return Results.Created($"/patients/{patient.Id}", patient);
    })
    .WithDisplayName("Create patient")
    .WithName("CreatePatient")
    .Produces<Patient>(201)
    .Produces(400);

app.MapPost("/patients/batch", async (List<Patient> patients, ApplicationDbContext db) =>
    {
        db.Patients.AddRange(patients);
        await db.SaveChangesAsync();
        return Results.Created("/patients", patients);
    })
    .WithDisplayName("Create patients batch")
    .WithName("CreatePatientsBatch")
    .Produces<List<Patient>>(201)
    .Produces(400);


app.MapPut("/patients/{id}", async (int id, Patient inputPatient, ApplicationDbContext db) =>
    {
        var patient = await db.Patients.Include(p => p.Name).FirstOrDefaultAsync(p => p.Id == id);
        if (patient is null) return Results.NotFound();

        patient.Name = inputPatient.Name;
        patient.Gender = inputPatient.Gender;
        patient.BirthDate = inputPatient.BirthDate;
        patient.Active = inputPatient.Active;

        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithDisplayName("Update patient")
    .WithName("UpdatePatient")
    .Produces(200)
    .Produces(404);

app.MapDelete("/patients/{id}", async (int id, ApplicationDbContext db) =>
    {
        var patient = await db.Patients.Include(p => p.Name).FirstOrDefaultAsync(p => p.Id == id);
        if (patient is null) return Results.NotFound();

        db.Remove(patient);
        await db.SaveChangesAsync();
        return Results.Ok();
    })
    .WithDisplayName("Delete patient")
    .WithName("DeletePatient")
    .Produces(200)
    .Produces(404);

app.MapGet("/patients/search", async (DateTime birthDate, string? @operator, ApplicationDbContext context) =>
    {
        IQueryable<Patient> query = context.Patients;

        @operator = @operator?.ToLower() ?? "eq";

        switch (@operator)
        {
            case "eq":
                query = query.Where(p => p.BirthDate.Date == birthDate.Date);
                break;
            case "ne":
                query = query.Where(p => p.BirthDate.Date != birthDate.Date);
                break;
            case "gt":
                query = query.Where(p => p.BirthDate.Date > birthDate.Date);
                break;
            case "lt":
                query = query.Where(p => p.BirthDate.Date < birthDate.Date);
                break;
            case "ge":
                query = query.Where(p => p.BirthDate.Date >= birthDate.Date);
                break;
            case "le":
                query = query.Where(p => p.BirthDate.Date <= birthDate.Date);
                break;
            default:
                return Results.BadRequest("Invalid operator");
        }

        var patients = await query
            .Include(p => p.Name)
            .ToListAsync();

        return patients.Count > 0 ? Results.Ok(patients) : Results.NotFound();
    })
    .WithDisplayName("Search patients by birthdate")
    .WithName("SearchPatientsByBirthDate")
    .Produces<List<Patient>>(200)
    .Produces(404)
    .Produces(400);

app.Run();

public class ApplicationDbContext : DbContext
{
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Name> Names { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Patient>()
            .HasOne(p => p.Name)
            .WithOne()
            .HasForeignKey<Name>(n => n.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}