using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PMS.API.Middleware;
using PMS.Application.Interfaces;
using PMS.Application.Mapping;
using PMS.Application.Services;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;
using PMS.Infrastructure;
using PMS.Infrastructure.Data;
using PMS.Infrastructure.Identity;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ASP.NET Core Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<long>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddDataProtection();

// Application services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IUserService, UserService>();


// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Infrastructure and application DI

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<PMS.Application.Interfaces.IProjectMemberService, PMS.Application.Services.ProjectMemberService>();
builder.Services.AddInfrastructure(builder.Configuration);
// AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Client", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PMS API v1");
        c.RoutePrefix = string.Empty; // Serve UI at app root
    });
}

// Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("Client");

app.UseAuthentication();
app.UseAuthorization();


app.MapControllers();

// Seed roles and permissions
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<long>>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        // fire-and-forget seed
        await SeedRolesAsync(roleManager);
    }
    catch (Exception ex)
    {
        // log if logging is configured; swallow to not prevent app start
        Console.WriteLine($"Error seeding roles: {ex.Message}");
    }
}

app.Run();

static async Task SeedRolesAsync(RoleManager<IdentityRole<long>> roleManager)
{
    var roles = new Dictionary<string, string[]>
    {
        { "Admin", new[] { "projects.view", "projects.create", "projects.edit", "projects.delete", "users.manage" } },
        { "Manager", new[] { "projects.view", "projects.create", "projects.edit" } },
        { "User", new[] { "projects.view" } }
    };

    foreach (var kv in roles)
    {
        var roleName = kv.Key;
        var permissions = kv.Value;
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            role = new IdentityRole<long> { Name = roleName };
            var created = await roleManager.CreateAsync(role);
            if (!created.Succeeded) continue;
            role = await roleManager.FindByNameAsync(roleName);
        }

        var existing = await roleManager.GetClaimsAsync(role);
        foreach (var perm in permissions)
        {
            if (!existing.Any(c => c.Type == "permission" && c.Value == perm))
            {
                await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", perm));
            }
        }
    }
}
