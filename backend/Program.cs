using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DawaeeBackend.Data;
using DawaeeBackend.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Dawaee Medical API",
        Version = "v1",
        Description = "API for Dawaee Medical Platform"
    });
    
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database - PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};Port={uri.Port};SSL Mode=Require;Trust Server Certificate=true";
    Console.WriteLine("📦 Using Railway PostgreSQL");
}
else
{
    Console.WriteLine("📦 Using Local PostgreSQL");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// CORS - مهم للشات بوت
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JWT
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? 
             builder.Configuration["Jwt:Key"] ?? 
             "dawaee-super-secret-key-2024-for-jwt";
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// Add HttpClient for Chatbot
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure pipeline
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dawaee API V1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.EnsureCreated();
        
        if (!db.Medications.Any())
        {
            Console.WriteLine("🌱 Seeding medications...");
            var meds = new List<Medication>();
            for (int i = 1; i <= 100; i++)
            {
                meds.Add(new Medication
                {
                    NameAr = GetMedicationNameAr(i),
                    NameEn = GetMedicationNameEn(i),
                    ActiveIngredientAr = GetActiveIngredientAr(i),
                    ActiveIngredientEn = GetActiveIngredientEn(i),
                    DescriptionAr = GetDescriptionAr(i),
                    DescriptionEn = GetDescriptionEn(i),
                    WarningsAr = GetWarningsAr(i),
                    WarningsEn = GetWarningsEn(i),
                    DangerLevel = GetDangerLevel(i)
                });
            }
            db.Medications.AddRange(meds);
            db.SaveChanges();
            
            if (!db.Users.Any(u => u.Email == "admin@dawaee.com"))
            {
                var admin = new User
                {
                    FirstName = "Admin",
                    LastName = "Dawaee",
                    Email = "admin@dawaee.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };
                db.Users.Add(admin);
                
                var patient = new User
                {
                    FirstName = "Test",
                    LastName = "Patient",
                    Email = "patient@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient123!"),
                    Role = "Patient",
                    CreatedAt = DateTime.UtcNow
                };
                db.Users.Add(patient);
                db.SaveChanges();
            }
            Console.WriteLine($"✅ Seeded {meds.Count} medications");
        }
        else
        {
            Console.WriteLine($"✅ Database ready: {db.Medications.Count()} medications, {db.Users.Count()} users");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Database error: {ex.Message}");
    }
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
Console.WriteLine("\n╔════════════════════════════════════════════════════╗");
Console.WriteLine("║           🚀 DAWAEE BACKEND RUNNING              ║");
Console.WriteLine("╠════════════════════════════════════════════════════╣");
Console.WriteLine($"║  Port: {port}                                            ║");
Console.WriteLine($"║  Swagger UI: http://localhost:{port}/swagger            ║");
Console.WriteLine("╠════════════════════════════════════════════════════╣");
Console.WriteLine("║  👤 LOGIN CREDENTIALS:                             ║");
Console.WriteLine("║     Admin: admin@dawaee.com / Admin123!            ║");
Console.WriteLine("║     Patient: patient@test.com / Patient123!        ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

app.Run($"http://0.0.0.0:{port}");

// Helper methods
static string GetMedicationNameAr(int i)
{
    string[] names = { "بانادول", "بروفين", "أوجمنتين", "سيبروفلوكساسين", "أموكسيسيلين", 
                       "ديكلوفيناك", "أوميبرازول", "لوراتادين", "بيتاهايستين", "فنتولين" };
    return $"{names[i % names.Length]}";
}

static string GetMedicationNameEn(int i)
{
    string[] names = { "Panadol", "Brufen", "Augmentin", "Ciprofloxacin", "Amoxicillin",
                       "Diclofenac", "Omeprazole", "Loratadine", "Betahistine", "Ventolin" };
    return $"{names[i % names.Length]}";
}

static string GetActiveIngredientAr(int i)
{
    string[] ingredients = { "باراسيتامول", "ايبوبروفين", "أموكسيسيلين+حمض كلافولانيك", "سيبروفلوكساسين", 
                              "أموكسيسيلين", "ديكلوفيناك صوديوم", "أوميبرازول", "لوراتادين", "بيتاهايستين", 
                              "سالبوتامول" };
    return ingredients[i % ingredients.Length];
}

static string GetActiveIngredientEn(int i)
{
    string[] ingredients = { "Paracetamol", "Ibuprofen", "Amoxicillin+Clavulanic acid", "Ciprofloxacin",
                              "Amoxicillin", "Diclofenac Sodium", "Omeprazole", "Loratadine", "Betahistine",
                              "Salbutamol" };
    return ingredients[i % ingredients.Length];
}

static string GetDescriptionAr(int i)
{
    string[] descriptions = { 
        "مسكن للآلام وخافض للحرارة يستخدم لعلاج الصداع وآلام الأسنان والحمى.",
        "مضاد للالتهابات غير ستيرويدي يستخدم لتسكين الآلام وتخفيف الالتهاب.",
        "مضاد حيوي واسع المجال لعلاج العدوى البكتيرية.",
        "مضاد حيوي من مجموعة الفلوروكينولونات لعلاج الالتهابات البكتيرية."
    };
    return descriptions[i % descriptions.Length];
}

static string GetDescriptionEn(int i)
{
    string[] descriptions = {
        "Pain reliever and fever reducer used to treat headaches, toothaches, and fever.",
        "Non-steroidal anti-inflammatory drug used for pain relief and inflammation reduction.",
        "Broad-spectrum antibiotic for treating bacterial infections.",
        "Fluoroquinolone antibiotic for treating bacterial infections."
    };
    return descriptions[i % descriptions.Length];
}

static string GetWarningsAr(int i)
{
    string[] warnings = {
        "لا تتجاوز الجرعة الموصى بها لتجنب تسمم الكبد.",
        "قد يسبب تهيج في المعدة، يفضل تناوله مع الطعام.",
        "يمنع استخدامه لمن لديهم حساسية من البنسلين.",
        "تجنب التعرض للشمس أثناء العلاج بهذا الدواء."
    };
    return warnings[i % warnings.Length];
}

static string GetWarningsEn(int i)
{
    string[] warnings = {
        "Do not exceed recommended dose to avoid liver toxicity.",
        "May cause stomach irritation, take with food.",
        "Contraindicated for those with penicillin allergy.",
        "Avoid sun exposure while taking this medication."
    };
    return warnings[i % warnings.Length];
}

static string GetDangerLevel(int i)
{
    if (i % 5 == 0) return "high";
    if (i % 3 == 0) return "medium";
    return "low";
}
