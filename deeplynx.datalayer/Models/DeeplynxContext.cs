using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace deeplynx.datalayer.Models;

public partial class DeeplynxContext : DbContext
{
    public DeeplynxContext()
    {
    }

    public DeeplynxContext(DbContextOptions<DeeplynxContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<DataSource> DataSources { get; set; }

    public virtual DbSet<Edge> Edges { get; set; }

    public virtual DbSet<EdgeParameter> EdgeParameters { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Record> Records { get; set; }

    public virtual DbSet<RecordParameter> RecordParameters { get; set; }

    public virtual DbSet<Relationship> Relationships { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // if (!optionsBuilder.IsConfigured)
        // {
        //     // This fallback connection string is used only if no external configuration is provided.
        //     // It is recommended to supply the connection string via DI in production (from appsettings.json).
        //     string fallbackConnectionString = "Host=localhost;Port=5434;Database=mydb;Username=root;Password=root";
        //     optionsBuilder.UseNpgsql(fallbackConnectionString);
        // }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("classes_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Project).WithMany(p => p.Classes).HasConstraintName("classes_project_id_fkey");
        });

        modelBuilder.Entity<DataSource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("data_sources_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Project).WithMany(p => p.DataSources).HasConstraintName("data_sources_project_id_fkey");
        });

        modelBuilder.Entity<Edge>(entity =>
        {
            entity.HasKey(e => new { e.OriginId, e.DestinationId }).HasName("edges_pkey");

            entity.HasOne(d => d.Destination).WithMany(p => p.EdgeDestinations).HasConstraintName("edges_destination_id_fkey");

            entity.HasOne(d => d.Origin).WithMany(p => p.EdgeOrigins).HasConstraintName("edges_origin_id_fkey");

            entity.HasOne(d => d.Relationship).WithMany(p => p.Edges)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("edges_relationship_id_fkey");
        });

        modelBuilder.Entity<EdgeParameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("edge_parameters_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Destination).WithMany(p => p.EdgeParameterDestinations).HasConstraintName("edge_parameters_destination_id_fkey");

            entity.HasOne(d => d.Origin).WithMany(p => p.EdgeParameterOrigins).HasConstraintName("edge_parameters_origin_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.EdgeParameters).HasConstraintName("edge_parameters_project_id_fkey");

            entity.HasOne(d => d.Relationship).WithMany(p => p.EdgeParameters).HasConstraintName("edge_parameters_relationship_id_fkey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("permissions_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.DataSource).WithMany(p => p.Permissions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("permissions_data_source_id_fkey");

            entity.HasOne(d => d.Record).WithMany(p => p.Permissions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("permissions_record_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.Permissions).HasConstraintName("permissions_role_id_fkey");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("projects_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Record>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("records_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Class).WithMany(p => p.Records)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("records_class_id_fkey");

            entity.HasOne(d => d.DataSource).WithMany(p => p.Records).HasConstraintName("records_data_source_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Records).HasConstraintName("records_project_id_fkey");
        });

        modelBuilder.Entity<RecordParameter>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("record_parameters_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Class).WithMany(p => p.RecordParameters).HasConstraintName("record_parameters_class_id_fkey");
        });

        modelBuilder.Entity<Relationship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("relationships_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Destination).WithMany(p => p.RelationshipDestinations).HasConstraintName("relationships_destination_id_fkey");

            entity.HasOne(d => d.Origin).WithMany(p => p.RelationshipOrigins).HasConstraintName("relationships_origin_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Relationships).HasConstraintName("relationships_project_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Project).WithMany(p => p.Roles).HasConstraintName("roles_project_id_fkey");

            entity.HasMany(d => d.Users).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("user_roles_user_id_fkey"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("user_roles_role_id_fkey"),
                    j =>
                    {
                        j.HasKey("RoleId", "UserId").HasName("user_roles_pkey");
                        j.ToTable("user_roles", "deeplynx");
                        j.IndexerProperty<long>("RoleId").HasColumnName("role_id");
                        j.IndexerProperty<long>("UserId").HasColumnName("user_id");
                    });
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasMany(d => d.Projects).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserProject",
                    r => r.HasOne<Project>().WithMany()
                        .HasForeignKey("ProjectId")
                        .HasConstraintName("user_projects_project_id_fkey"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("user_projects_user_id_fkey"),
                    j =>
                    {
                        j.HasKey("UserId", "ProjectId").HasName("user_projects_pkey");
                        j.ToTable("user_projects", "deeplynx");
                        j.IndexerProperty<long>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<long>("ProjectId").HasColumnName("project_id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
