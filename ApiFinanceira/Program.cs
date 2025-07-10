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

// Configuração do DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
});

// Registro dos Repositórios para Injeção de Dependência
builder.Services.AddScoped<IPessoaRepository, PessoaRepository>();
builder.Services.AddScoped<IContaRepository, ContaRepository>();
// A linha duplicada para IContaRepository foi removida.
builder.Services.AddScoped<ICartaoRepository, CartaoRepository>();
builder.Services.AddScoped<ITransacaoRepository, TransacaoRepository>();


// --- INÍCIO DA CONFIGURAÇÃO DE COMPLIANCE COM REFIT ---

// 1. Registre as configurações da API de Compliance (email/senha e URLs base)
// Certifique-se que esta seção "ExternalApis:Compliance" está presente no seu appsettings.json
builder.Services.Configure<ComplianceApiSettings>(builder.Configuration.GetSection("ExternalApis:Compliance"));

// 2. Registre o DelegatingHandler para autenticação
// Ele será injetado nos clientes Refit que o utilizam.
builder.Services.AddTransient<AuthHeaderHandler>();

// 3. Registre o cliente Refit para a API de Autenticação (IComplianceAuthClient)
builder.Services.AddRefitClient<IComplianceAuthClient>()
    .ConfigureHttpClient(c =>
    {
        // A URL base para os endpoints de autenticação (ex: /auth/code, /auth/token)
        // Certifique-se de que "ExternalApis:Compliance:AuthBaseUrl" está no appsettings.json
        c.BaseAddress = new Uri(builder.Configuration["ExternalApis:Compliance:AuthBaseUrl"]!);
    });

// 4. Registre o cliente Refit para a API de Validação (IComplianceValidationClient)
builder.Services.AddRefitClient<IComplianceValidationClient>()
    .ConfigureHttpClient(c =>
    {
        // A URL base para os endpoints de validação (ex: /v1/cpf/validate, /v1/cnpj/validate)
        // Certifique-se de que "ExternalApis:Compliance:ValidationBaseUrl" está no appsettings.json
        c.BaseAddress = new Uri(builder.Configuration["ExternalApis:Compliance:ValidationBaseUrl"]!);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>(); // Anexa o handler de autenticação a este cliente

// 5. Registre o seu ComplianceService
// Ele será injetado onde for necessário (por exemplo, em um Controller ou outro serviço),
// e o DI resolverá suas dependências (os clientes Refit e IOptions<ComplianceApiSettings>).
// Como ComplianceService implementa IComplianceService e IAuthTokenProvider,
// ele será resolvido para ambos os tipos quando solicitados.
builder.Services.AddScoped<IComplianceService, ComplianceService>();

// --- FIM DA CONFIGURAÇÃO DE COMPLIANCE COM REFIT ---


// Registro dos Serviços de Aplicação para Injeção de Dependência
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<ICartaoService, CartaoService>();
builder.Services.AddScoped<ITransacaoService, TransacaoService>();
builder.Services.AddScoped<IAuthService, AuthService>();


// Configuração JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada.");
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

    // Configuração para JWT no Swagger UI
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Token de autenticação JWT (Bearer Token). Ex: 'Bearer seu_token_aqui'"
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

// Configuração do pipeline de requisições HTTP
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