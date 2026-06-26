using Empreendimentos.Api.Middlewares;
using Empreendimentos.Application.Common;
using Empreendimentos.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- Serviços ---

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serializa enums (Status) como string ("Ativo"/"Inativo") em vez de número.
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API de Gestão de Empreendimentos",
        Version = "v1",
        Description = "API para cadastro, listagem, edição e inativação de empreendimentos."
    });

    // Inclui os comentários XML (///) do código nos endpoints do Swagger.
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// CORS liberado para o frontend Angular rodando em desenvolvimento.
const string AngularCorsPolicy = "AngularCorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// --- Pipeline HTTP ---

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API de Empreendimentos v1");
        options.RoutePrefix = string.Empty; // Swagger disponível na raiz: http://localhost:5000/
    });
}

app.UseCors(AngularCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();

// Necessário para WebApplicationFactory em testes de integração, se desejado.
public partial class Program { }
