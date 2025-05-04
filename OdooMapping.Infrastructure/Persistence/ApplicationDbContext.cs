using Microsoft.EntityFrameworkCore;
using OdooMapping.Domain.Models;

namespace OdooMapping.Infrastructure.Persistence
{
    /// <summary>
    /// Application database context for Entity Framework Core
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<MappingDefinition> MappingDefinitions { get; set; }
        public DbSet<FieldMapping> FieldMappings { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure MappingDefinition entity
            modelBuilder.Entity<MappingDefinition>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.SourceConnectionString).IsRequired();
                entity.Property(e => e.SourceQuery).IsRequired();
                entity.Property(e => e.TargetConnectionString).IsRequired();
                entity.Property(e => e.TargetTable).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastExecutionLog).HasMaxLength(4000);
                
                // Configure relationship to FieldMappings
                entity.HasMany(e => e.FieldMappings)
                      .WithOne(e => e.MappingDefinition)
                      .HasForeignKey(e => e.MappingDefinitionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            
            // Configure FieldMapping entity
            modelBuilder.Entity<FieldMapping>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SourceField).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SourceFieldType).HasMaxLength(50);
                entity.Property(e => e.TargetField).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TargetFieldType).HasMaxLength(50);
                entity.Property(e => e.TransformationExpression).HasMaxLength(500);
                entity.Property(e => e.DefaultValue).HasMaxLength(500);
            });
        }
    }
} 