using CrudeApi.Data;
using CrudeApi.Models.DomainModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization();

builder.Services.AddIdentity<UsersModel, IdentityRole<int>>()
    .AddEntityFrameworkStores<PizzaContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata=false;
    options.SaveToken=true;
    options.TokenValidationParameters=new TokenValidationParameters()
    {
        ValidateIssuer=true,
        ValidateAudience=true,
        ValidAudience=builder.Configuration["Jwt:Audience"],
        ValidIssuer=builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});


builder.Services.AddScoped<UserManager<UsersModel>>();
builder.Services.AddControllers();
// Register the UserManager


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(Program));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextPool<PizzaContext>(opt =>
{
    opt.UseSqlServer(connectionString);
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy.WithOrigins("http://localhost:3000",
                                "http://www.contoso.com")
            .AllowAnyHeader()
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .WithExposedHeaders("Authorization");

        });
});

var app = builder.Build();



if ( app.Environment.IsDevelopment() )
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();
app.Run();

