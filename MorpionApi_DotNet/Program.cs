using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MorpionApi_DotNet.Auth;
using MorpionApi_DotNet.Data;
using MorpionApi_DotNet.Endpoints;
using MorpionApi_DotNet.Players;
using MorpionApi_DotNet.Services;

var builder = WebApplication.CreateBuilder(args);

var jwtAuthOptions = builder.Configuration
    .GetSection(JwtAuthOptions.SectionName)
    .Get<JwtAuthOptions>()
    ?? throw new InvalidOperationException("La configuration JwtAuth est manquante.");

var connectionString = builder.Configuration.GetConnectionString("MorpionDb")
    ?? throw new InvalidOperationException("La chaine de connexion MorpionDb est manquante.");

builder.Services.AddOpenApi();
builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection(JwtAuthOptions.SectionName));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services
    .AddAuthentication(options =>
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
            ValidIssuer = jwtAuthOptions.Issuer,
            ValidAudience = jwtAuthOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IBotPlayer, RandomBotPlayer>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<MorpionGameService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapMorpionEndpoints();

app.Run();
