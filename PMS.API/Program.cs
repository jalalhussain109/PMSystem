using LMKR.Shared.Logging.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PMS.API.Extensions;
using PMS.API.Middleware;
using PMS.Application.Interfaces;
using PMS.Application.Mapping;
using PMS.Application.Services;
using PMS.Domain.Constants;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;
using PMS.Infrastructure;
using PMS.Infrastructure.Data;
using PMS.Infrastructure.Identity;
using Serilog;
using Serilog.Formatting.Json;
using System.IO;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for structured (JSON) file logging in PMS.API/Logs
var logsFolder = Path.Combine(builder.Environment.ContentRootPath, "Logs");
Directory.CreateDirectory(logsFolder);
var logFile = Path.Combine(logsFolder, "pms-.json"); // rolling daily files

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(new JsonFormatter(renderMessage: true), logFile, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 31)
    .CreateLogger();

builder.Host.UseSerilog();

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
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IEffectivePermissionService, IdentityPermissionService>();
builder.Services.AddScoped<IProjectMemberService, ProjectMemberService>();
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
builder.AddSerilogExtention();

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


app.UseHttpsRedirection();

app.UseCors("Client");

app.UseRouting();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseApiLogsMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed roles and permissions
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;

//    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<long>>>();

//    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

//    await SeedRolesAsync(roleManager);
//    await SeedAdminUserAsync(userManager);
//}

app.Run();


#region Seed Admin User and Roles
static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
{
    const string adminUserName = "admin";
    const string adminEmail = "admin@pms.local";
    const string adminPassword = "Admin@123";

    var adminUser = await userManager.FindByNameAsync(adminUserName);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminUserName,
            Email = adminEmail,
            DisplayName = "Jalal Hussain",
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(adminUser, adminPassword);
    }

    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        var result = await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
static async Task SeedRolesAsync(RoleManager<IdentityRole<long>> roleManager)
{
    var roles = new Dictionary<string, string[]>
    {
        { "Admin", new[] { "Projects.View", "Projects.Create", "Projects.Edit", "Projects.Delete", "Projects.AssignTeam", "Users.Manage" } },
        { "ProjectManager", new[] { "Projects.View", "Projects.Create", "Projects.Edit", "Tasks.View","Tasks.Create","Tasks.Edit", "Projects.AssignTeam" } },
        { "Member", new[] { "Projects.View", "Tasks.View", "Tasks.Create", "Tasks.Edit" } },
        { "User", new[] { "Projects.View" } }
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
            if (!existing.Any(c => c.Type == PermissionConstants.ClaimType && c.Value == perm))
            {
                await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim(PermissionConstants.ClaimType, perm));
            }
        }
    }
}
#endregion