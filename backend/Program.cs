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

// Database - Auto detect environment
var isRailway = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT"));

string? connectionString;
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    // Railway injects postgres://user:pass@host:port/db
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Prefer;Trust Server Certificate=true";
}
else if (isRailway)
{
    connectionString = builder.Configuration.GetConnectionString("RailwayConnection");
    Console.WriteLine("🚀 Running in RAILWAY mode (RailwayConnection)");
}
else
{
    connectionString = builder.Configuration.GetConnectionString("LocalConnection");
    Console.WriteLine("💻 Running in LOCAL mode");
}

Console.WriteLine("📡 Connecting to database...");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        npgsqlOptions.CommandTimeout(60);
    }));

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
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

builder.Services.AddHttpClient();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dawaee API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.MapGet("/health", () => Results.Ok(new { 
    status = "healthy", 
    environment = isRailway ? "railway" : "local",
    timestamp = DateTime.UtcNow 
}));

// Migrate and seed database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        Console.WriteLine("🔄 Ensuring database is ready...");
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
                db.Users.AddRange(new User
                {
                    FirstName = "Admin", LastName = "Dawaee", Email = "admin@dawaee.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = "Admin", CreatedAt = DateTime.UtcNow
                }, new User
                {
                    FirstName = "Test", LastName = "Patient", Email = "patient@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient123!"),
                    Role = "Patient", CreatedAt = DateTime.UtcNow
                });
                db.SaveChanges();
            }
            Console.WriteLine($"✅ Seeded {meds.Count} medications");
        }
        else
        {
            Console.WriteLine($"✅ DB ready: {db.Medications.Count()} medications, {db.Users.Count()} users");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ DB error: {ex.Message}");
        Console.WriteLine($"   Inner: {ex.InnerException?.Message}");
        if (!isRailway)
            Console.WriteLine("💡 Start PostgreSQL: sudo service postgresql start");
    }
}

Console.WriteLine("\n✅ DAWAEE BACKEND RUNNING");
Console.WriteLine($"   Environment: {(isRailway ? "RAILWAY" : "LOCAL")}");
Console.WriteLine("   Admin: admin@dawaee.com / Admin123!");
Console.WriteLine("   Patient: patient@test.com / Patient123!\n");

// ✅ CRITICAL: Do NOT pass a URL to app.Run()
// Railway uses ASPNETCORE_URLS env var to set the port
app.Run();

static string GetMedicationNameAr(int i)
{
    string[] names = { "بانادول", "بروفين", "أوجمنتين", "سيبروفلوكساسين", "أموكسيسيلين", 
                       "ديكلوفيناك", "أوميبرازول", "لوراتادين", "بيتاهايستين", "فنتولين" };
    return names[i % names.Length];
}
static string GetMedicationNameEn(int i)
{
    string[] names = { "Panadol", "Brufen", "Augmentin", "Ciprofloxacin", "Amoxicillin",
                       "Diclofenac", "Omeprazole", "Loratadine", "Betahistine", "Ventolin" };
    return names[i % names.Length];
}
static string GetActiveIngredientAr(int i)
{
    string[] ingredients = { "باراسيتامول", "ايبوبروفين", "أموكسيسيلين+حمض كلافولانيك", "سيبروفلوكساسين", 
                              "أموكسيسيلين", "ديكلوفيناك صوديوم", "أوميبرازول", "لوراتادين", "بيتاهايستين", "سالبوتامول" };
    return ingredients[i % ingredients.Length];
}
static string GetActiveIngredientEn(int i)
{
    string[] ingredients = { "Paracetamol", "Ibuprofen", "Amoxicillin+Clavulanic acid", "Ciprofloxacin",
                              "Amoxicillin", "Diclofenac Sodium", "Omeprazole", "Loratadine", "Betahistine", "Salbutamol" };
    return ingredients[i % ingredients.Length];
}
static string GetDescriptionAr(int i)
{
    string[] d = { "مسكن للآلام وخافض للحرارة يستخدم لعلاج الصداع وآلام الأسنان والحمى.",
                   "مضاد للالتهابات غير ستيرويدي يستخدم لتسكين الآلام وتخفيف الالتهاب.",
                   "مضاد حيوي واسع المجال لعلاج العدوى البكتيرية.",
                   "مضاد حيوي من مجموعة الفلوروكينولونات لعلاج الالتهابات البكتيرية." };
    return d[i % d.Length];
}
static string GetDescriptionEn(int i)
{
    string[] d = { "Pain reliever and fever reducer used to treat headaches, toothaches, and fever.",
                   "Non-steroidal anti-inflammatory drug used for pain relief and inflammation reduction.",
                   "Broad-spectrum antibiotic for treating bacterial infections.",
                   "Fluoroquinolone antibiotic for treating bacterial infections." };
    return d[i % d.Length];
}
static string GetWarningsAr(int i)
{
    string[] w = { "لا تتجاوز الجرعة الموصى بها لتجنب تسمم الكبد.",
                   "قد يسبب تهيج في المعدة، يفضل تناوله مع الطعام.",
                   "يمنع استخدامه لمن لديهم حساسية من البنسلين.",
                   "تجنب التعرض للشمس أثناء العلاج بهذا الدواء." };
    return w[i % w.Length];
}
static string GetWarningsEn(int i)
{
    string[] w = { "Do not exceed recommended dose to avoid liver toxicity.",
                   "May cause stomach irritation, take with food.",
                   "Contraindicated for those with penicillin allergy.",
                   "Avoid sun exposure while taking this medication." };
    return w[i % w.Length];
}
static string GetDangerLevel(int i)
{
    if (i % 5 == 0) return "high";
    if (i % 3 == 0) return "medium";
    return "low";
}
