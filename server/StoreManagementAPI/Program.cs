using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StoreManagementAPI.Configs;
using StoreManagementAPI.Middlewares;
using StoreManagementAPI.Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;


// Add services to the container.
builder.Services.Configure<StoreManagementDBSettings>(
    builder.Configuration.GetSection("StoreManagementDatabase"));

builder.Services.Configure<JWTSettings>(
       builder.Configuration.GetSection("JWTSettings"));

builder.Services.Configure<MailSettings>(
          builder.Configuration.GetSection("EmailSettings"));

builder.Services.Configure<AppSettings>(
             builder.Configuration.GetSection("AppSettings"));

builder.Services.AddSingleton<OrderProductsService>();

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<OrderService>();
builder.Services.AddSingleton<ProductService>();
builder.Services.AddSingleton<CustomerService>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddSingleton<AnalyticsService>();

builder.Services.AddSingleton<MailService>();
builder.Services.AddSingleton<JWTTokenService>();
builder.Services.AddSingleton<ResetPasswordTokenService>();

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JWTSettings").Get<JWTSettings>();

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(config["JWTSettings:Secret"])),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim("Role", "ADMIN"));
    options.AddPolicy("User", policy => policy.RequireClaim("Role", "User"));
});


builder.Services.AddControllers()
    .AddJsonOptions(opt => { opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseCors(x => x
       .AllowAnyOrigin()
       .AllowAnyMethod()
       .AllowAnyHeader());


app.MapControllers();

app.Run();
