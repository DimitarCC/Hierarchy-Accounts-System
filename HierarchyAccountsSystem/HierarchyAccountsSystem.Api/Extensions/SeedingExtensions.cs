using HierarchyAccountsSystem.BusinessLogic.DataContext;
using HierarchyAccountsSystem.BusinessLogic.Models;
using Microsoft.EntityFrameworkCore;

namespace HierarchyAccountsSystem.Api.Extensions;

/// <summary>
/// Provides extension methods for configuring and executing database seeding operations during application startup.
/// </summary>
/// <remarks>These extensions are intended to be used with ASP.NET Core applications to ensure that the database
/// is created, migrated, and populated with initial data when the application starts. Typical usage involves calling
/// the seeding extension in the application's startup configuration pipeline.</remarks>
public static class SeedingExtensions {
  /// <summary>
  /// Initializes and seeds the application's database during startup.
  /// </summary>
  /// <remarks>This method ensures that the database schema is created and applies any pending migrations before
  /// seeding initial data. It is typically called at application startup to prepare the database for use. If migrations
  /// fail, the method attempts to create the database to ensure the application can start with a valid
  /// schema.</remarks>
  /// <param name="app">The <see cref="WebApplication"/> instance to configure and seed.</param>
  /// <returns>The same <see cref="WebApplication"/> instance, to enable method chaining.</returns>
  public static WebApplication UseSeeding(this WebApplication app) {
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetService<ILoggerFactory>()?.CreateLogger("Seed")!;
    var db = services.GetRequiredService<HASDbContext>();

    try {
      db.Database.EnsureCreated();
      db.Database.Migrate();
    } catch (Exception ex) {
      logger?.LogWarning(ex, "Something happened during initial database creation or migration execution.");
      // Rethrowing exception after logging to avoid silent failures
      throw ex;
    }

    SeedAsync(db, logger).GetAwaiter().GetResult();
    return app;
  }

  private static async Task SeedAsync(HASDbContext db, ILogger? logger = null) {
    if (await db.Accounts.AnyAsync()) {
      logger?.LogInformation("Seed skipped: Accounts already exist.");
      return;
    }

    logger?.LogInformation("Seeding initial data...");

    var globalAccount = new Account {
      AccountId = 0,
      Name = "Global Account",
      ParentAccountId = null
    };

    await db.Accounts.AddAsync(globalAccount);
    await db.SaveChangesAsync();
    logger?.LogInformation("Seeding complete.");
  }
}
