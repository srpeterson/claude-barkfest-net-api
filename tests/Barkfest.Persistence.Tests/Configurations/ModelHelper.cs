using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Barkfest.Persistence.Tests.Configurations;

/// <summary>
/// Builds the EF Core model once using the SQL Server provider and caches it.
/// Tests can inspect column names, max lengths, nullability, FK behaviour, etc.
/// without requiring a live database connection — OnModelCreating runs during
/// model construction regardless of whether a connection is available.
/// </summary>
internal static class ModelHelper
{
    private static readonly Lazy<IModel> _model = new(BuildModel);

    public static IModel Model => _model.Value;

    private static IModel BuildModel()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=.;Database=ModelInspection;")
            .Options;

        // The context is disposed immediately after the model is finalized.
        // No connection is ever opened.
        using var ctx = new AppDbContext(options);
        return ctx.Model.GetRelationalModel().Model;
    }
}
