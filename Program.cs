using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using WebApplication1.Services; // Ensure this namespace matches where AzureBlobStorageService is defined

var builder = WebApplication.CreateBuilder(args);

// ✅ Add services to the Amahle was here 2025
builder.Services.AddControllers();

// ✅ Configure CORS policy for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",  // ✅ Local Development
                "https://www.mybeautyshop.co.za"  // ✅ Production Domain (No trailing `/`)
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials(); //  Allow credentials (JWT, cookies)
    });
});

// ✅ Configure Entity Framework Core with SQL Server
builder.Services.AddDbContext<BeautyShopDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ✅ Register the AzureBlobStorageService for dependency injection
builder.Services.AddSingleton<AzureBlobStorageService>();
builder.Services.AddSingleton<LocalFileStorageService>();


// ✅ Configure JWT Authentication
var secretKey = builder.Configuration["Jwt:SecretKey"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new ArgumentNullException("Jwt:SecretKey", "JWT SecretKey is not configured.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Allow HTTP for local testing
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero, // Prevents delays in token expiration
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

// ✅ Configure Swagger for API documentation
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddSingleton<LocalFileStorageService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "BeautyShop API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by a space and your JWT token."
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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

var app = builder.Build();

// ✅ Configure middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles();

// ✅ Apply CORS before authentication/authorization
app.UseCors("AllowReactApp");

// ✅ Enforce HTTPS redirection
app.UseHttpsRedirection();

// ✅ Add Authentication and Authorization middleware (Order Matters)
app.UseAuthentication(); // Must be before UseAuthorization
app.UseAuthorization();

// ✅ Map controllers to routes
app.MapControllers();

// ✅ Start the application
app.Run();
