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

    public virtual DbSet<EdgeMapping> EdgeMappings { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<Record> Records { get; set; }

    public virtual DbSet<RecordMapping> RecordMappings { get; set; }

    public virtual DbSet<Relationship> Relationships { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<RoleResource> RoleResources { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProject> UserProjects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("classes_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Project).WithMany(p => p.Classes).HasConstraintName("classes_project_id_fkey");
        });

        modelBuilder.Entity<DataSource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("data_sources_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Project).WithMany(p => p.DataSources).HasConstraintName("data_sources_project_id_fkey");
        });

        modelBuilder.Entity<Edge>(entity =>
        {
            entity.HasKey(e => new { e.OriginId, e.DestinationId }).HasName("edges_pkey");

            entity.Property(e => e.ProjectId).HasDefaultValue(0L);

            entity.HasOne(d => d.DataSource).WithMany(p => p.Edges).HasConstraintName("edges_data_source_id_fkey");

            entity.HasOne(d => d.Destination).WithMany(p => p.EdgeDestinations).HasConstraintName("edges_destination_id_fkey");

            entity.HasOne(d => d.Origin).WithMany(p => p.EdgeOrigins).HasConstraintName("edges_origin_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Edges).HasConstraintName("edges_project_id_fkey");

            entity.HasOne(d => d.Relationship).WithMany(p => p.Edges)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("edges_relationship_id_fkey");
        });

        modelBuilder.Entity<EdgeMapping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("edge_mappings_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Destination).WithMany(p => p.EdgeMappingDestinations).HasConstraintName("edge_mappings_destination_id_fkey");

            entity.HasOne(d => d.Origin).WithMany(p => p.EdgeMappingOrigins).HasConstraintName("edge_mappings_origin_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.EdgeMappings).HasConstraintName("edge_mappings_project_id_fkey");

            entity.HasOne(d => d.Relationship).WithMany(p => p.EdgeMappings).HasConstraintName("edge_mappings_relationship_id_fkey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("permissions_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Name).HasDefaultValueSql("''::text");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("projects_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Record>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("records_pkey");

            entity.HasIndex(e => e.Properties, "idx_records_properties").HasMethod("gin");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Class).WithMany(p => p.Records)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("records_class_id_fkey");

            entity.HasOne(d => d.DataSource).WithMany(p => p.Records).HasConstraintName("records_data_source_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Records).HasConstraintName("records_project_id_fkey");

            entity.HasMany(d => d.Tags).WithMany(p => p.Records)
                .UsingEntity<Dictionary<string, object>>(
                    "RecordTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("record_tags_tag_id_fkey"),
                    l => l.HasOne<Record>().WithMany()
                        .HasForeignKey("RecordId")
                        .HasConstraintName("record_tags_record_id_fkey"),
                    j =>
                    {
                        j.HasKey("RecordId", "TagId").HasName("record_tags_pkey");
                        j.ToTable("record_tags", "deeplynx");
                        j.HasIndex(new[] { "RecordId" }, "idx_record_tags_record_id");
                        j.HasIndex(new[] { "TagId" }, "idx_record_tags_tag_id");
                        j.IndexerProperty<long>("RecordId").HasColumnName("record_id");
                        j.IndexerProperty<long>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<RecordMapping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("record_mappings_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ProjectId).HasDefaultValue(0L);

            entity.HasOne(d => d.Class).WithMany(p => p.RecordMappings).HasConstraintName("record_mappings_class_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.RecordMappings).HasConstraintName("record_mappings_project_id_fkey");

            entity.HasOne(d => d.Tag).WithMany(p => p.RecordMappings).HasConstraintName("record_mappings_tag_id_fkey");
        });

        modelBuilder.Entity<Relationship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("relationships_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

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
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("role_permissions_pkey");

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions).HasConstraintName("role_permissions_permission_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions).HasConstraintName("role_permissions_role_id_fkey");
        });

        modelBuilder.Entity<RoleResource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("role_resources_pkey");

            entity.Property(e => e.HasAccess).HasDefaultValue(true);

            entity.HasOne(d => d.DataSource).WithMany(p => p.RoleResources)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("role_resources_data_source_id_fkey");

            entity.HasOne(d => d.Record).WithMany(p => p.RoleResources)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("role_resources_record_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.RoleResources).HasConstraintName("role_resources_role_id_fkey");

            entity.HasOne(d => d.Tag).WithMany(p => p.RoleResources)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("role_resources_tag_id_fkey");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Project).WithMany(p => p.Tags).HasConstraintName("tags_project_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<UserProject>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ProjectId }).HasName("user_projects_pkey");

            entity.HasOne(d => d.Project).WithMany(p => p.UserProjects).HasConstraintName("user_projects_project_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.UserProjects)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_projects_role_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserProjects).HasConstraintName("user_projects_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
