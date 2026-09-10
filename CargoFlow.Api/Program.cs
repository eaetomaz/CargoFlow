using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CargoFlow.Application.DependencyInjection;
using CargoFlow.Infrastructure.DependencyInjection;
using CargoFlow.Infrastructure.Persistence;
using CargoFlow.Infrastructure.Seeding;
using CargoFlow.Infrastructure.Simulation;

// Carrega o .env da raiz do repo (procurando subindo a partir do diretório
// de trabalho) para variáveis de ambiente reais — nunca para appsettings.json.
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<SimulationHostedService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "CargoFlow - API",
        Version = "v1"
    });
});

// JWT_SECRET vem só do .env (nunca de appsettings.json) -- mesmo padrão dos
// segredos do product-hunter (MELI_CLIENT_SECRET etc).
var jwtSecret = builder.Configuration["JWT_SECRET"]
    ?? throw new InvalidOperationException("JWT_SECRET não configurado. Copie .env.example para .env.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Tudo protegido por padrão -- cada controller opta pelos perfis que aceita
// via [Authorize(Roles = "...")]; só api/auth/login fica [AllowAnonymous].
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

// Frontend Vite roda em localhost:5180 (ver CargoFlow.frontend/vite.config.ts).
const string FrontendCorsPolicy = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5180", "https://localhost:5180")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        if (feature?.Error is not null)
            logger.LogError(feature.Error, "Erro não tratado em {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message = "Erro interno. Veja os logs para detalhes." });
    });
});

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CargoFlowDbContext>();
    await context.Database.MigrateAsync();
    await UserSeeder.SeedAsync(context, CancellationToken.None);

    var expiringSoonThresholdDays = builder.Configuration.GetValue("Documents:ExpiringSoonThresholdDays", 30);
    await DemoDataSeederService.SeedAsync(context, expiringSoonThresholdDays, CancellationToken.None);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.Run();
