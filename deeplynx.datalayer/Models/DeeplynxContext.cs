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
    
    public virtual DbSet<ApiKey> ApiKeys { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<DataSource> DataSources { get; set; }

    public virtual DbSet<Edge> Edges { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<HistoricalEdge> HistoricalEdges { get; set; }

    public virtual DbSet<HistoricalRecord> HistoricalRecords { get; set; }
    
    public virtual DbSet<OauthApplication> OauthApplications { get; set; }
    
    public virtual DbSet<OauthToken> OauthTokens { get; set; }

    public virtual DbSet<ObjectStorage> ObjectStorages { get; set; }

    public virtual DbSet<Organization> Organizations { get; set; }

    public virtual DbSet<OrganizationUser> OrganizationUsers { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectMember> ProjectMembers { get; set; }

    public virtual DbSet<Record> Records { get; set; }

    public virtual DbSet<Relationship> Relationships { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SensitivityLabel> SensitivityLabels { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }
    
    public virtual DbSet<SavedSearch> SavedSearches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Action>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("actions_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_actions_last_updated_by");
    
            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedActions)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);

            entity.HasOne(d => d.Project).WithMany(p => p.Actions).HasConstraintName("actions_project_id_fkey");
        });

        modelBuilder.Entity<ApiKey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("api_keys_pkey");

            entity.HasOne(d => d.User).WithMany(p => p.ApiKeys)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("api_keys_user_id_fkey");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("classes_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_classes_last_updated_by");
            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedClasses)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);
            entity.HasOne(d => d.Project).WithMany(p => p.Classes).HasConstraintName("classes_project_id_fkey");
        });

        modelBuilder.Entity<DataSource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("data_sources_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_data_sources_last_updated_by");
            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedDataSources)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);
            entity.HasOne(d => d.Project).WithMany(p => p.DataSources).HasConstraintName("data_sources_project_id_fkey");
        });

        modelBuilder.Entity<Edge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("edges_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.ToTable(e => e.HasCheckConstraint(
                "CK_edges_origin_destination_different",
                "origin_id <> destination_id"));

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_edges_last_updated_by");

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedEdges)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);

            entity.HasOne(d => d.DataSource).WithMany(p => p.Edges).HasConstraintName("edges_data_source_id_fkey");

            entity.HasOne(d => d.Destination).WithMany(p => p.EdgeDestinations).HasConstraintName("edges_destination_id_fkey");

            entity.HasOne(d => d.Origin).WithMany(p => p.EdgeOrigins).HasConstraintName("edges_origin_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Edges).HasConstraintName("edges_project_id_fkey");

            entity.HasOne(d => d.Relationship).WithMany(p => p.Edges)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("edges_relationship_id_fkey");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("events_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasIndex(e=>e.LastUpdatedBy).HasDatabaseName("idx_events_last_updated_by");
            
            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedEvents)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);
            
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("groups_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_groups_last_updated_by");
    
            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedGroups)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);

            entity.HasOne(d => d.Organization).WithMany(p => p.Groups).HasConstraintName("groups_organization_id_fkey");

            entity.HasMany(d => d.Users).WithMany(p => p.Groups)
                .UsingEntity<Dictionary<string, object>>(
                    "GroupUser",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("group_users_group_id_fkey"),
                    l => l.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .HasConstraintName("group_users_user_id_fkey"),
                    j =>
                    {
                        j.HasKey("GroupId", "UserId").HasName("group_users_pkey");
                        j.ToTable("group_users", "deeplynx");
                        j.HasIndex(new[] { "GroupId" }, "idx_group_users_group_id");
                        j.HasIndex(new[] { "UserId" }, "idx_group_users_user_id");
                        j.IndexerProperty<long>("GroupId").HasColumnName("group_id");
                        j.IndexerProperty<long>("UserId").HasColumnName("user_id");
                    });
        });

        modelBuilder.Entity<HistoricalEdge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("historical_edges_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.DataSourceName).HasDefaultValueSql("''::text");
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_historical_edges_last_updated_by");
    
            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedHistoricalEdges)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);
            entity.Property(e => e.ProjectName).HasDefaultValueSql("''::text");

            entity.HasOne(d => d.Edge).WithMany(p => p.HistoricalEdges).HasConstraintName("historical_edges_edge_id_fkey");
        });

        modelBuilder.Entity<HistoricalRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("historical_records_pkey");

            entity.HasIndex(e => e.Properties, "idx_historical_records_properties").HasMethod("gin");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);

            entity.HasOne(d => d.Record).WithMany(p => p.HistoricalRecords).HasConstraintName("historical_records_record_id_fkey");
        });

        modelBuilder.Entity<OauthApplication>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("oauth_applications_pkey");
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_oauth_applications_last_updated_by");
            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.UpdatedOauthApplications)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);
        });
        
        modelBuilder.Entity<OauthToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("oauth_tokens_pkey");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastUsedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Revoked).HasDefaultValue(false);
            
            entity.HasOne(d => d.OauthApplication).WithMany(p => p.OauthTokens)
                .HasConstraintName("oauth_tokens_application_id_fkey");
            
            entity.HasOne(d => d.User).WithMany(p => p.OauthTokens)
                .HasConstraintName("oauth_tokens_user_id_fkey");
        });

        modelBuilder.Entity<ObjectStorage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("object_storage_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_object_storages_last_updated_by");

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedObjectStorages)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);

            entity.HasOne(d => d.Project).WithMany(p => p.ObjectStorages).HasConstraintName("object_storage_project_id_fkey");
        });

        modelBuilder.Entity<Organization>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("organization_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_organizations_last_updated_by");

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedOrganizations)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);
        });

        modelBuilder.Entity<OrganizationUser>(entity =>
        {
            entity.HasKey(e => new { e.OrganizationId, e.UserId }).HasName("organization_user_pkey");
            entity.Property(e => e.IsOrgAdmin).HasDefaultValue(false);

            entity.HasOne(d => d.Organization).WithMany(p => p.OrganizationUsers).HasConstraintName("organization_users_organization_id_fkey");


            entity.HasOne(d => d.User).WithMany(p => p.OrganizationUsers).HasConstraintName("organization_users_user_id_fkey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("permission_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_permissions_last_updated_by");

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedPermissions)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);

            entity.HasOne(d => d.Label).WithMany(p => p.Permissions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("permissions_label_id_fkey");
            
            entity.HasOne(d => d.Project).WithMany(p => p.Permissions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("permissions_project_id_fkey");
            
            entity.HasOne(d => d.Organization).WithMany(p => p.Permissions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("permissions_organization_id_fkey");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("projects_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.Config).HasDefaultValueSql("'{\"tagsMutable\": false, \"ontologyMutable\": false, \"edgeRecordsMutable\": false}'::jsonb");
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_projects_last_updated_by");

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedProjects)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);

            entity.HasOne(d => d.Organization).WithMany(p => p.Projects)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("projects_organization_id_fkey");
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("project_members_pkey");

            entity.HasOne(d => d.Group).WithMany(p => p.ProjectMembers)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("project_members_group_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectMembers).HasConstraintName("project_members_project_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.ProjectMembers).HasConstraintName("project_members_role_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ProjectMembers)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("project_members_user_id_fkey");
        });

        modelBuilder.Entity<Record>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("records_pkey");

            entity.HasIndex(e => e.Properties, "idx_records_properties").HasMethod("gin");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
           
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_records_last_updated_by");

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedRecords)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);

            entity.HasOne(d => d.Class).WithMany(p => p.Records)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("records_class_id_fkey");

            entity.HasOne(d => d.DataSource).WithMany(p => p.Records).HasConstraintName("records_data_source_id_fkey");

            entity.HasOne(d => d.ObjectStorage).WithMany(p => p.Records).HasConstraintName("records_object_storage_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Records).HasConstraintName("records_project_id_fkey");

            entity.HasMany(d => d.Labels).WithMany(p => p.Records)
                .UsingEntity<Dictionary<string, object>>(
                    "RecordLabel",
                    r => r.HasOne<SensitivityLabel>().WithMany()
                        .HasForeignKey("LabelId")
                        .HasConstraintName("record_labels_label_id_fkey"),
                    l => l.HasOne<Record>().WithMany()
                        .HasForeignKey("RecordId")
                        .HasConstraintName("record_labels_record_id_fkey"),
                    j =>
                    {
                        j.HasKey("RecordId", "LabelId").HasName("record_labels_pkey");
                        j.ToTable("record_labels", "deeplynx");
                        j.HasIndex(new[] { "LabelId" }, "idx_record_labels_label_id");
                        j.HasIndex(new[] { "RecordId" }, "idx_record_labels_record_id");
                        j.IndexerProperty<long>("RecordId").HasColumnName("record_id");
                        j.IndexerProperty<long>("LabelId").HasColumnName("label_id");
                    });

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

        modelBuilder.Entity<Relationship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("relationships_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_relationships_last_updated_by");

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedRelationships)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);
            
            entity.HasOne(d => d.Destination).WithMany(p => p.RelationshipDestinations)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("relationships_destination_id_fkey");

            entity.HasOne(d => d.Origin).WithMany(p => p.RelationshipOrigins)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("relationships_origin_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Relationships).HasConstraintName("relationships_project_id_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_roles_last_updated_by");

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedRoles)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);
            
            entity.HasOne(d => d.Organization).WithMany(p => p.Roles)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("roles_organization_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Roles)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("roles_project_id_fkey");

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .HasConstraintName("role_permissions_permission_id_fkey"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("role_permissions_role_id_fkey"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId").HasName("role_permissions_pkey");
                        j.ToTable("role_permissions", "deeplynx");
                        j.HasIndex(new[] { "PermissionId" }, "idx_role_permissions_permission_id");
                        j.HasIndex(new[] { "RoleId" }, "idx_role_permissions_role_id");
                        j.IndexerProperty<long>("RoleId").HasColumnName("role_id");
                        j.IndexerProperty<long>("PermissionId").HasColumnName("permission_id");
                    });
        });

        modelBuilder.Entity<SensitivityLabel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sensitivity_labels_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_sensitivity_labels_last_updated_by");

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedSensitivityLabels)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);
            
            entity.HasOne(d => d.Organization).WithMany(p => p.SensitivityLabels)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("sensitivity_label_organization_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.SensitivityLabels)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("sensitivity_label_project_id_fkey");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subscriptions_pkey");

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_subscriptions_last_updated_by");

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedSubscriptions)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);

            entity.HasOne(d => d.Action).WithMany(p => p.Subscriptions).HasConstraintName("subscriptions_action_id_fkey");

            entity.HasOne(d => d.DataSource).WithMany(p => p.Subscriptions)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("subscriptions_dataSource_id_fkey");

            entity.HasOne(d => d.Project).WithMany(p => p.Subscriptions).HasConstraintName("subscriptions_project_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Subscriptions).HasConstraintName("subscriptions_user_id_fkey");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tags_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            
            entity.HasIndex(e => e.LastUpdatedBy).HasDatabaseName("idx_tags_last_updated_by");

            entity.HasOne(d => d.LastUpdatedByUser)
                .WithMany(p => p.LastUpdatedTags)
                .HasForeignKey(d => d.LastUpdatedBy)
                .OnDelete(DeleteBehavior.NoAction)
                .HasConstraintName(null);

            entity.HasOne(d => d.Project).WithMany(p => p.Tags).HasConstraintName("tags_project_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            entity.Property(e => e.IsArchived).HasDefaultValue(false);

            entity.Property(e => e.IsSysAdmin).HasDefaultValue(false);
            
            entity.Property(e => e.IsActive).HasDefaultValue(false);
        });
        
        modelBuilder.Entity<SavedSearch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("saved_searches_pkey");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();
            
            entity.Property(e => e.IsFavorite).HasDefaultValue(false);

            entity.Property(e => e.LastUpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.HasOne(d => d.User).WithMany(p => p.SavedSearches).HasConstraintName("saved_searches_user_id_fkey");
            
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
