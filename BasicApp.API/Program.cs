using BasicApp.Data.Context;
using BasicApp.Data.UOW;
using BasicApp.Helpers.JWT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using BasicApp.Helpers.Token;
using Microsoft.AspNetCore.Builder;
using BasicApp.Helpers;
using BasicApp.Helpers.Session;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<BasicAppDbContext>(options =>
    options.UseSqlite("Data Source=BasicAppDb.sqlite"));

builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

var JwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

builder.Services.AddSingleton<JwtHelper>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ISessionService, SessionService>();

builder.Services.AddAuthorization();

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

builder.Services.AddControllers();

builder.Services.AddMvc();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Configure Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VkApi Api Management", Version = "v1.0" });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "OrderAutomation Management",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                { securityScheme, new string[] { } }
            });
});

// JWT configuration
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = true;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = JwtConfig.Issuer,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtConfig.Secret)),
        ValidAudience = JwtConfig.Audience,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAllHeaders",
    builder =>
    {
        builder.AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BasicAppApi v1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseRouting();

app.UseCors("AllowAllHeaders");

app.UseAuthorization();

app.MapControllers();

app.Run();
