using ApiFinanceira.Application.Clients;
using ApiFinanceira.Application.ExternalServices;
using ApiFinanceira.Application.Services;
using ApiFinanceira.Application.Services.ApiFinanceira.Application.Services;
using ApiFinanceira.Domain.Interfaces;
using ApiFinanceira.Infrastructure.Data;
using ApiFinanceira.Infrastructure.HttpHandlers; 
using ApiFinanceira.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Refit; 
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configura��o do DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
});

// Registro dos Reposit�rios para Inje��o de Depend�ncia
builder.Services.AddScoped<IPessoaRepository, PessoaRepository>();
builder.Services.AddScoped<IContaRepository, ContaRepository>();
// A linha duplicada para IContaRepository foi removida.
builder.Services.AddScoped<ICartaoRepository, CartaoRepository>();
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();


// --- IN�CIO DA CONFIGURA��O DE COMPLIANCE COM REFIT ---

// 1. Registre as configura��es da API de Compliance (email/senha e URLs base)
// Certifique-se que esta se��o "ExternalApis:Compliance" est� presente no seu appsettings.json
builder.Services.Configure<ComplianceApiSettings>(builder.Configuration.GetSection("ExternalApis:Compliance"));

// 2. Registre o DelegatingHandler para autentica��o
// Ele ser� injetado nos clientes Refit que o utilizam.
builder.Services.AddTransient<AuthHeaderHandler>();

// 3. Registre o cliente Refit para a API de Autentica��o (IComplianceAuthClient)
builder.Services.AddRefitClient<IComplianceAuthClient>()
    .ConfigureHttpClient(c =>
    {
        // A URL base para os endpoints de autentica��o (ex: /auth/code, /auth/token)
        // Certifique-se de que "ExternalApis:Compliance:AuthBaseUrl" est� no appsettings.json
        c.BaseAddress = new Uri(builder.Configuration["ExternalApis:Compliance:AuthBaseUrl"]!);
    });

// 4. Registre o cliente Refit para a API de Valida��o (IComplianceValidationClient)
builder.Services.AddRefitClient<IComplianceValidationClient>()
    .ConfigureHttpClient(c =>
    {
        // A URL base para os endpoints de valida��o (ex: /v1/cpf/validate, /v1/cnpj/validate)
        // Certifique-se de que "ExternalApis:Compliance:ValidationBaseUrl" est� no appsettings.json
        c.BaseAddress = new Uri(builder.Configuration["ExternalApis:Compliance:ValidationBaseUrl"]!);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>(); // Anexa o handler de autentica��o a este cliente

// 5. Registre o seu ComplianceService
// Ele ser� injetado onde for necess�rio (por exemplo, em um Controller ou outro servi�o),
// e o DI resolver� suas depend�ncias (os clientes Refit e IOptions<ComplianceApiSettings>).
// Como ComplianceService implementa IComplianceService e IAuthTokenProvider,
// ele ser� resolvido para ambos os tipos quando solicitados.
builder.Services.AddScoped<IComplianceService, ComplianceService>();

// --- FIM DA CONFIGURA��O DE COMPLIANCE COM REFIT ---


// Registro dos Servi�os de Aplica��o para Inje��o de Depend�ncia
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<ICartaoService, CartaoService>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();
builder.Services.AddScoped<IAuthService, AuthService>();


// Configura��o JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key n�o configurada.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

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

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "API Financeira", Version = "v1" });

    // Configura��o para JWT no Swagger UI
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Token de autentica��o JWT (Bearer Token). Ex: 'Bearer seu_token_aqui'"
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configura��o do pipeline de requisi��es HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();