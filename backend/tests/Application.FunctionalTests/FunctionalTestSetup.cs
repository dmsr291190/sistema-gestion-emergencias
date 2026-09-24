using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sige.Infrastructure.Data;

namespace Sige.Application.FunctionalTests;

[SetUpFixture]
public class FunctionalTestSetup
{
    internal static IServiceScopeFactory ScopeFactory { get; private set; } = null!;
    internal static DatabaseResetter? DbResetter { get; private set; }

    private static WebApiFactory? _factory;

    // Se conecta al MySQL que ya corre en Docker (mismo servidor que Desarrollo),
    // usando una base de datos separada para no mezclar datos de pruebas con datos demo.
    // Sobrescribible con la variable de entorno ConnectionStrings__SigeDbTest.
    private const string DefaultTestConnectionString =
        "Server=localhost;Port=3306;Database=SigeDb_Test;User=sige_app;Password=SigeApp123!;TreatTinyAsBoolean=true;";

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SigeDbTest")
            ?? DefaultTestConnectionString;

        _factory = new WebApiFactory(connectionString);
        ScopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        using (var scope = ScopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();
        }

        DbResetter = await DatabaseResetter.CreateAsync(connectionString);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (DbResetter is not null) await DbResetter.DisposeAsync();
        if (_factory is not null) await _factory.DisposeAsync();
    }
}
