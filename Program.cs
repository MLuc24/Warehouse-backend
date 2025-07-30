using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WarehouseManage.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework
builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register repositories (Product only)
builder.Services.AddScoped<WarehouseManage.Interfaces.ISupplierRepository, WarehouseManage.Repositories.SupplierRepository>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IProductRepository, WarehouseManage.Repositories.ProductRepository>();

// Register services (Product only)  
builder.Services.AddScoped<WarehouseManage.Interfaces.IProductService, WarehouseManage.Services.ProductService>();

// Register HttpClient với cấu hình optimized cho API validation
builder.Services.AddHttpClient("ApiValidation", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15); // Timeout ngắn hơn cho API validation
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // Bypass SSL cho development
});

// Register HttpClient mặc định cho các service khác
builder.Services.AddHttpClient();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured"));

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
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000") // React/Vite default ports
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Use CORS
app.UseCors("AllowFrontend");

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
