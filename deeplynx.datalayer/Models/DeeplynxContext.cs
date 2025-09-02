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

    
    public virtual DbSet<Action> Actions { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<DataSource> DataSources { get; set; }

    public virtual DbSet<Edge> Edges { get; set; }
    
    public virtual DbSet<EdgeMapping> EdgeMappings { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<HistoricalEdge> HistoricalEdges { get; set; }

    public virtual DbSet<HistoricalRecord> HistoricalRecords { get; set; }

    public virtual DbSet<ObjectStorage> ObjectStorages { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<OrganizationUser> OrganizationUsers { get; set; }
    
    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Project> Projects { get; set; }
    
    public virtual DbSet<ProjectMember> ProjectMembers { get; set; }

    public virtual DbSet<Record> Records { get; set; }

    public virtual DbSet<RecordMapping> RecordMappings { get; set; }

    public virtual DbSet<Relationship> Relationships { get; set; }
    
    public virtual DbSet<Role> Roles { get; set; }
    
    public virtual DbSet<SensitivityLabel> SensitivityLabels { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Action>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("actions_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Project)
                .WithMany(p => p.Actions)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("actions_project_id_fkey");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("classes_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Project).WithMany(p => p.Classes).HasConstraintName("classes_project_id_fkey");
        });

        modelBuilder.Entity<DataSource>(entity =>
        {
            entity.Property(e => e.Id).HasIdentityOptions(startValue: 1);
            entity.HasKey(e => e.Id).HasName("data_sources_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Project).WithMany(p => p.DataSources).HasConstraintName("data_sources_project_id_fkey");
        });

        modelBuilder.Entity<Edge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("edges_pkey");

            entity.Property(e => e.ProjectId).HasDefaultValue(0L);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.DataSource).WithMany(p => p.Edges).HasConstraintName("edges_data_source_id_fkey");

            entity.HasOne(d => d.Destination).WithMany(p => p.EdgeDestinations).HasConstraintName("edges_destination_id_fkey");

            entity.HasOne(d => d.EdgeMapping).WithMany(p => p.Edges)
                .HasForeignKey(d => d.MappingId).IsRequired(false).HasConstraintName("edges_mapping_id_fkey");

            entity.HasOne(d => d.Origin).WithMany(p => p.EdgeOrigins).HasConstraintName("edges_origin_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Edges).HasConstraintName("edges_project_id_fkey");

            entity.HasOne(d => d.Relationship).WithMany(p => p.Edges)
                .HasForeignKey(d => d.RelationshipId).IsRequired(false).HasConstraintName("edges_relationship_id_fkey");
        });

        modelBuilder.Entity<EdgeMapping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("edge_mappings_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Destination).WithMany(p => p.EdgeMappingDestinations).HasConstraintName("edge_mappings_destination_id_fkey");

            entity.HasOne(d => d.Origin).WithMany(p => p.EdgeMappingOrigins).HasConstraintName("edge_mappings_origin_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.EdgeMappings).HasConstraintName("edge_mappings_project_id_fkey");

            entity.HasOne(d => d.Relationship).WithMany(p => p.EdgeMappings).HasConstraintName("edge_mappings_relationship_id_fkey");

            entity.HasOne(r => r.DataSource).WithMany(d => d.EdgeMappings).HasConstraintName("edge_mappings_data_source_id_fkey");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("events_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Project)
                .WithMany(p => p.Events)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("events_project_id_fkey");

            entity.HasOne(d => d.DataSource)
                .WithMany(p => p.Events)
                .HasForeignKey(d => d.DataSourceId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("events_dataSource_id_fkey");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("groups_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(g => g.Organization).WithMany(p => p.Groups)
                .HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("groups_organization_id_fkey");

            entity.HasMany(g => g.Users).WithMany(u => u.Groups)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupUser",
                    gu => gu.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("group_users_group_id_fkey"),
                    gu => gu.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .HasConstraintName("group_users_user_id_fkey"),
                    gu =>
                    {
                        gu.HasKey("GroupId", "UserId").HasName("group_users_pkey");
                        gu.ToTable("group_users", "deeplynx");
                        gu.HasIndex(new[] {"GroupId"}, "idx_group_users_group_id");
                        gu.HasIndex(new[] {"UserId"}, "idx_group_users_user_id");
                        gu.IndexerProperty<long>("GroupId").HasColumnName("group_id");
                        gu.IndexerProperty<long>("UserId").HasColumnName("user_id");
                    });
        });

        modelBuilder.Entity<HistoricalEdge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("historical_edges_pkey");

            entity.Property(e => e.ProjectId).HasDefaultValue(0L);

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Edge).WithMany(p => p.HistoricalEdges)
                .HasForeignKey(d => d.EdgeId).IsRequired(true).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("historical_edges_edge_id_fkey");
        });

        modelBuilder.Entity<HistoricalRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("historical_records_pkey");

            entity.HasIndex(e => e.Properties, "idx_historical_records_properties").HasMethod("gin");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Record).WithMany(p => p.HistoricalRecords)
                .HasForeignKey(d => d.RecordId).IsRequired(true).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("historical_records_record_id_fkey");
        });

        modelBuilder.Entity<ObjectStorage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("object_storage_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Project).WithMany(p => p.ObjectStorages).HasConstraintName("object_storage_project_id_fkey");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(o => o.Id).HasName("organization_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<OrganizationUser>(entity =>
        {
            entity.HasKey(ou => new { ou.OrganizationId, ou.UserId }).HasName("organization_user_pkey");

            entity.HasOne(ou => ou.Organization).WithMany(o => o.OrganizationUsers)
                .HasForeignKey(ou => ou.OrganizationId).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("organization_users_organization_id_fkey");

            entity.HasOne(ou => ou.User).WithMany(u => u.OrganizationUsers)
                .HasForeignKey(ou => ou.UserId).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("organization_users_user_id_fkey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("permission_pkey");

            entity.Property(p => p.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(p => p.SensitivityLabel).WithMany(s => s.Permissions)
                .HasForeignKey(p => p.LabelId).IsRequired(false).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("permissions_label_id_fkey");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("projects_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ConfigJson)
                .HasColumnType("jsonb")
                .HasDefaultValueSql("'{\"edgeRecordsMutable\":false,\"ontologyMutable\":false,\"tagsMutable\":false}'::jsonb")
                .IsRequired();

            entity.HasOne(p => p.Organization).WithMany(p => p.Projects)
                .HasForeignKey(p => p.OrganizationId).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("projects_organization_id_fkey");
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(pm => pm.Id).HasName("project_members_pkey");
            
            entity.HasOne(pm => pm.Project).WithMany(p => p.ProjectMembers)
                .HasForeignKey(pm => pm.ProjectId).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("project_members_project_id_fkey");
            
            entity.HasOne(pm => pm.Role).WithMany(r => r.ProjectMembers)
                .HasForeignKey(pm => pm.RoleId).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("project_members_role_id_fkey");
            
            entity.HasOne(pm => pm.Group).WithMany(g => g.ProjectMembers)
                .HasForeignKey(pm => pm.GroupId).OnDelete(DeleteBehavior.Cascade).IsRequired(false)
                .HasConstraintName("project_members_group_id_fkey");
            
            entity.HasOne(pm => pm.User).WithMany(u => u.ProjectMembers)
                .HasForeignKey(pm => pm.UserId).OnDelete(DeleteBehavior.Cascade).IsRequired(false)
                .HasConstraintName("project_members_user_id_fkey");
        });

        modelBuilder.Entity<Record>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("records_pkey");

            entity.HasIndex(e => e.Properties, "idx_records_properties").HasMethod("gin");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Class).WithMany(p => p.Records)
                .HasForeignKey(d => d.ClassId).IsRequired(false).HasConstraintName("records_class_id_fkey");

            entity.HasOne(d => d.RecordMapping).WithMany(p => p.Records)
                .HasForeignKey(d => d.MappingId).IsRequired(false).HasConstraintName("records_mapping_id_fkey");

            entity.HasOne(d => d.DataSource).WithMany(p => p.Records).HasConstraintName("records_data_source_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Records).HasConstraintName("records_project_id_fkey");

            entity.HasOne(d => d.ObjectStorage).WithMany(p => p.Records).HasConstraintName("records_object_storage_id_fkey");

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

            entity.HasMany(r => r.SensitivityLabels).WithMany(p => p.Records)
                .UsingEntity<Dictionary<string, object>>(
                    "RecordLabel",
                    rl => rl.HasOne<SensitivityLabel>().WithMany()
                        .HasForeignKey("LabelId")
                        .HasConstraintName("record_labels_label_id_fkey"),
                    rl => rl.HasOne<Record>().WithMany()
                        .HasForeignKey("RecordId")
                        .HasConstraintName("record_labels_record_id_fkey"),
                    rl =>
                    {
                        rl.HasKey("RecordId", "LabelId").HasName("record_labels_pkey");
                        rl.ToTable("record_labels", "deeplynx");
                        rl.HasIndex(new[] { "RecordId" }, "idx_record_labels_record_id");
                        rl.HasIndex(new[] { "LabelId" }, "idx_record_labels_label_id");
                        rl.IndexerProperty<long>("LabelId").HasColumnName("label_id");
                        rl.IndexerProperty<long>("RecordId").HasColumnName("record_id");
                    });
        });

        modelBuilder.Entity<RecordMapping>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("record_mappings_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ProjectId).HasDefaultValue(0L);

            entity.HasOne(d => d.Class).WithMany(p => p.RecordMappings).HasConstraintName("record_mappings_class_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.RecordMappings).HasConstraintName("record_mappings_project_id_fkey");

            entity.HasOne(r => r.DataSource).WithMany(d => d.RecordMappings).HasConstraintName("record_mapping_data_source_id_fkey");

            entity.HasMany(r => r.Tags).WithMany(t => t.RecordMappings)
                .UsingEntity<Dictionary<string, object>>(
                    "RecordMappingTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .HasConstraintName("record_mapping_tags_tag_id_fkey"),
                    l => l.HasOne<RecordMapping>().WithMany()
                        .HasForeignKey("RecordMappingId")
                        .HasConstraintName("record_mapping_tags_record_mapping_id_fkey"),
                    j =>
                    {
                        j.HasKey("RecordMappingId", "TagId").HasName("record_mapping_tags_pkey");
                        j.ToTable("record_mapping_tags", "deeplynx");
                        j.HasIndex(new[] { "RecordMappingId" }, "idx_record_mapping_tags_record_mapping_id");
                        j.HasIndex(new[] { "TagId" }, "idx_record_mapping_tags_tag_id");
                        j.IndexerProperty<long>("RecordId").HasColumnName("record_mapping_id");
                        j.IndexerProperty<long>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<Relationship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("relationships_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Destination).WithMany(p => p.RelationshipDestinations)
                .HasForeignKey(d => d.DestinationId).IsRequired(false).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("relationships_destination_id_fkey");

            entity.HasOne(d => d.Origin).WithMany(p => p.RelationshipOrigins)
                .HasForeignKey(d => d.OriginId).IsRequired(false).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("relationships_origin_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Relationships).HasConstraintName("relationships_project_id_fkey");
        });
        
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(g => g.Project).WithMany(p => p.Roles)
                .HasForeignKey(d => d.ProjectId).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("roles_project_id_fkey");
            
            entity.HasMany(g => g.Permissions).WithMany(u => u.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    gu => gu.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("role_permissions_permission_id_fkey"),
                    gu => gu.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("role_permissions_role_id_fkey"),
                    gu =>
                    {
                        gu.HasKey("RoleId", "PermissionId").HasName("role_permissions_pkey");
                        gu.ToTable("role_permissions", "deeplynx");
                        gu.HasIndex(new[] {"RoleId"}, "idx_role_permissions_role_id");
                        gu.HasIndex(new[] {"PermissionId"}, "idx_role_permissions_permission_id");
                        gu.IndexerProperty<long>("RoleId").HasColumnName("role_id");
                        gu.IndexerProperty<long>("PermissionId").HasColumnName("permission_id");
                    });
        });

        modelBuilder.Entity<SensitivityLabel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sensitivity_labels_pkey");

            entity.Property(s => s.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(g => g.Organization).WithMany(p => p.SensitivityLabels)
                .HasForeignKey(d => d.OrganizationId).OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("sensitivity_labels_organization_id_fkey");
        });

         modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscriptions_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("subscriptions_user_id_fkey");

            entity.HasOne(d => d.Action)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.ActionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("subscriptions_action_id_fkey");

            entity.HasOne(d => d.Project)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("subscriptions_project_id_fkey");

            entity.HasOne(d => d.DataSource)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.DataSourceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("subscriptions_dataSource_id_fkey");
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
            entity.HasMany(u => u.Projects).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserProject",
                    u => u.HasOne<Project>().WithMany()
                        .HasForeignKey("ProjectId")
                        .HasConstraintName("user_project_project_id_fkey"),
                    p => p.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("user_project_user_id_fkey"),
                    j =>
                    {
                        j.HasKey("UserId", "ProjectId").HasName("user_project_pkey");
                        j.ToTable("user_project", "deeplynx");
                        j.HasIndex(new[] { "UserId" }, "idx_user_project_user_id");
                        j.HasIndex(new[] { "ProjectId" }, "idx_user_project_project_id");
                        j.IndexerProperty<long>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<long>("ProjectId").HasColumnName("project_id");
                    });

        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
