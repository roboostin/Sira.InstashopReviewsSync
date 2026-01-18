using API.Domain.Entities;
using API.Domain.Entities.Client;
using API.Domain.Entities.Review;
using Microsoft.EntityFrameworkCore;
namespace API.Infrastructure.Persistence.DbContexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<API.Domain.Entities.Location> Locations { get; set; }

    public DbSet<Domain.Entities.Client.Channel> Channels { get; set; }
    public DbSet<Domain.Entities.Customer.Customer> Customers { get; set; }
 
    public DbSet<Review> Reviews { get; set; }
 
    public DbSet<LocationChannel> LocationChannelQRCodes { get; set; }
 
    public DbSet<SourceReviewSummary> SourceReviewSummaries { get; set; }
 

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set precision for all decimal properties
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal) && property.GetColumnType() == null)
                {
                    property.SetColumnType("numeric(18,2)"); // PostgreSQL uses numeric instead of decimal
                }

                if (property.ClrType == typeof(string) && property.GetMaxLength() == null)
                {
                    property.SetMaxLength(250);
                }
            }

            var idProperty = entityType.GetProperties()
                .FirstOrDefault(p => p.Name == "ID" && p.ClrType == typeof(long));


            SetDefaultValue(modelBuilder, entityType, "CreatedAt", typeof(DateTime), "NOW()");
            SetDefaultValue(modelBuilder, entityType, "CreatedBy", typeof(Guid), "'00000000-0000-0000-0000-000000000000'");
            SetDefaultValue(modelBuilder, entityType, "IsDeleted", typeof(bool), "false");
            SetDefaultValue(modelBuilder, entityType, "IsActive", typeof(bool), "false");

            
        }

        // Configure relationships to prevent delete cascade
        foreach (var foreignKey in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var singularName = entityType.ClrType.Name;
            var snakeCaseName = ToSnakeCase(singularName);
            entityType.SetTableName(snakeCaseName);

            // Convert schema name to snake_case if it exists
            var schema = entityType.GetSchema();
            if (!string.IsNullOrEmpty(schema))
            {
                var snakeCaseSchema = ToSnakeCase(schema);
                entityType.SetSchema(snakeCaseSchema);
            }
        }

        // Convert all property (column) names to snake_case
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var propertyName = property.Name;
                var snakeCasePropertyName = ToSnakeCase(propertyName);
                property.SetColumnName(snakeCasePropertyName);
            }
        }




        base.OnModelCreating(modelBuilder);
    }

    private void SetDefaultValue(ModelBuilder modelBuilder, Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType, string propertyName, Type propertyType, string defaultValueSql)
    {
        var property = entityType.GetProperties().FirstOrDefault(p => p.Name == propertyName && p.ClrType == propertyType);
        if (property != null)
            modelBuilder.Entity(entityType.ClrType).Property(propertyName).HasDefaultValueSql(defaultValueSql);
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var buffer = new System.Text.StringBuilder(input.Length + 10);
        var prevLower = false;
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0 && (prevLower || (i + 1 < input.Length && char.IsLower(input[i + 1]))))
                {
                    buffer.Append('_');
                }
                buffer.Append(char.ToLowerInvariant(c));
                prevLower = false;
            }
            else
            {
                buffer.Append(c);
                prevLower = char.IsLetter(c) && char.IsLower(c);
            }
        }
        return buffer.ToString();
    }
}