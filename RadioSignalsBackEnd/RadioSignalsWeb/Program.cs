using Domain.Domain_Models;
using Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Repository.Implementation;
using Repository.Interface;
using Repository.Seed;
using Scalar.AspNetCore;
using Services.Implementation;
using Services.Interface;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Linq;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// --- Database ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions => npgsqlOptions.EnableRetryOnFailure() 
    ));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- Controllers / Views ---
builder.Services.AddControllersWithViews();

// --- Identity ---
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// --- Dependency Injection ---
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IMeasurementService, MeasurementService>();
builder.Services.AddScoped<IThresholdService, ThresholdService>();

// --- JWT Authentication ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
    };
});

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("vite-dev",
        p => p.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
   var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
      foreach (var name in Enum.GetNames(typeof(Role))) // "USER", "ADMIN"
         {
           if (!await roleManager.RoleExistsAsync(name))
            await roleManager.CreateAsync(new IdentityRole(name));
         }
}

app.UseCors("vite-dev");

// Seed municipalities & settlements from JSON on startup (runs once)
await app.Services.SeedMunicipalitiesAndSettlementsAsync(
    "SeedData\\north_macedonia_municipalities_settlements_seed.json");

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();

    app.MapOpenApi();
    app.MapScalarApiReference(); // browse at /scalar/v1
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();  
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
