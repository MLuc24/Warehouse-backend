using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WarehouseManage.Data;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load .env file từ thư mục hiện tại
var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFilePath))
{
    Env.Load(envFilePath);
}

// Load environment variables
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Configure Entity Framework
builder.Services.AddDbContext<WarehouseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Register repositories
builder.Services.AddScoped<WarehouseManage.Interfaces.IUserRepository, WarehouseManage.Repositories.UserRepository>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IEmployeeRepository, WarehouseManage.Repositories.EmployeeRepository>();
builder.Services.AddScoped<WarehouseManage.Interfaces.ISupplierRepository, WarehouseManage.Repositories.SupplierRepository>();
builder.Services.AddScoped<WarehouseManage.Interfaces.ICustomerRepository, WarehouseManage.Repositories.CustomerRepository>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IProductRepository, WarehouseManage.Repositories.ProductRepository>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IGoodsReceiptRepository, WarehouseManage.Repositories.GoodsReceiptRepository>();

// Register services
builder.Services.AddScoped<WarehouseManage.Interfaces.IAuthService, WarehouseManage.Services.AuthService>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IEmployeeService, WarehouseManage.Services.EmployeeService>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IVerificationService, WarehouseManage.Services.VerificationService>();
builder.Services.AddScoped<WarehouseManage.Interfaces.ISupplierService, WarehouseManage.Services.SupplierService>();
builder.Services.AddScoped<WarehouseManage.Interfaces.ICustomerService, WarehouseManage.Services.CustomerService>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IProductService, WarehouseManage.Services.ProductService>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IProductStockService, WarehouseManage.Services.ProductStockService>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IProductPricingService, WarehouseManage.Services.ProductPricingService>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IProductExpiryService, WarehouseManage.Services.ProductExpiryService>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IGoodsReceiptService, WarehouseManage.Services.GoodsReceiptService>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IGoodsReceiptWorkflowService, WarehouseManage.Services.GoodsReceiptWorkflowService>();

// Register GoodsIssue services
builder.Services.AddScoped<WarehouseManage.Interfaces.IGoodsIssueRepository, WarehouseManage.Repositories.GoodsIssueRepository>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IGoodsIssueService, WarehouseManage.Services.GoodsIssueService>();
builder.Services.AddScoped<WarehouseManage.Interfaces.IGoodsIssueWorkflowService, WarehouseManage.Services.GoodsIssueWorkflowService>();

// PDF Service
builder.Services.AddScoped<WarehouseManage.Services.IPdfService, WarehouseManage.Services.PdfService>();

// Register Category service
builder.Services.AddScoped<WarehouseManage.Interfaces.ICategoryService, WarehouseManage.Services.CategoryService>();

// Register notification service
builder.Services.AddScoped<WarehouseManage.Interfaces.INotificationService, WarehouseManage.Services.Communication.NotificationService>();

// Register validation service
builder.Services.AddScoped<WarehouseManage.Services.IValidationService, WarehouseManage.Services.ValidationService>();

builder.Services.AddScoped<WarehouseManage.Helpers.JwtHelper>();

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

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "Warehouse Management API", 
        Version = "v1",
        Description = "API for Warehouse Management System with Authentication and Supplier Management"
    });

    // Configure JWT Authentication for Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
            new string[] {}
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse Management API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at root
    });
}

// Only use HTTPS redirection if HTTPS port is configured
if (app.Environment.IsProduction() || builder.Configuration["ASPNETCORE_URLS"]?.Contains("https") == true)
{
    app.UseHttpsRedirection();
}

// Use CORS
app.UseCors("AllowFrontend");

// Use Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
