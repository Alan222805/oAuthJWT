using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using oAuthJWT;
using oAuthJWT.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//Añadiendo Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

//Autenticacion con JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //La autenticacion por defecto es JWT
    options.DefaultSignInScheme = "CookieAuth";
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;

})
.AddCookie("CookieAuth")
.AddJwtBearer(options =>
{

    options.TokenValidationParameters = new TokenValidationParameters
    {
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = builder.Configuration["Jwt:Issuer"],
    ValidAudience = builder.Configuration["Jwt:Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
    ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            //Obteniendo el token enviado por el usuario en el header
            var token = context.SecurityToken as JwtSecurityToken;

            //Si el token no es nulo y el tiempo de expiracion es menor que el tiempo actual, se falla la autenticacion
            if(token != null && token.ValidTo < DateTime.UtcNow)
            {
                context.Fail("Token expirado");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {

            if(context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true")  ;
                
            }
            return Task.CompletedTask;
        }
    };
})

.AddGoogle(googleOptions => 
{

    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
});


builder.Services.AddAuthorization();

//Conectando con la SQL Server
builder.Services.AddSqlServer<oAuthJTWContext>(builder.Configuration.GetConnectionString("connexionDb"));

builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();