using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMS.Domain.Entities;
using PMS.Infrastructure.Interceptors;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser, IdentityRole<long>, long>
{
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, AuditSaveChangesInterceptor auditInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<ApiLog> ApiLogs => Set<ApiLog>();

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var ownsTransaction = Database.CurrentTransaction == null;

        await using var transaction = ownsTransaction ? await Database.BeginTransactionAsync(cancellationToken) : null;

        try
        {
            // 1. Save Project / Task / ProjectMember changes.
            // The interceptor captures changes before this save.
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            // 2. Generate audit records after saving,
            // so database-generated IDs are available.
            if (_auditInterceptor.HasPendingAudits)
            {
                var logs = _auditInterceptor.BuildAuditLogs();

                _auditInterceptor.BeginAuditSave();

                try
                {
                    ActivityLogs.AddRange(logs);

                    // 3. Save audit records.
                    await base.SaveChangesAsync(cancellationToken);
                }
                finally
                {
                    _auditInterceptor.EndAuditSave();
                }
            }

            // 4. Commit business and audit changes together.
            if (transaction != null)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            return result;
        }
        catch
        {
            if (transaction != null)
            {
                await transaction.RollbackAsync(cancellationToken);
            }

            _auditInterceptor.EndAuditSave();

            throw;
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("ActivityLogs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EntityName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.EntityId)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Action)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.FieldName)
                .HasMaxLength(150);

            entity.Property(x => x.OldValue)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.NewValue)
                .HasColumnType("nvarchar(max)");

            entity.HasIndex(x => new
            {
                x.EntityName,
                x.EntityId,
                x.CreatedAt
            });

            entity.HasIndex(x => x.ChangeSetId);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasOne(t => t.AssignedUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Role)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.HasOne(x => x.Project)
                  .WithMany()
                  .HasForeignKey(x => x.ProjectId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.User)
                  .WithMany()
                  .HasForeignKey(x => x.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => new { x.ProjectId, x.UserId })
                  .IsUnique();
        });
    }
}