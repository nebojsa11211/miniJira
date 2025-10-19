using Microsoft.EntityFrameworkCore;
using MiniJira.Models;

namespace MiniJira.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Models.Task> Tasks { get; set; } = null!;
    public DbSet<Column> Columns { get; set; } = null!;
    public DbSet<ColumnTranslation> ColumnTranslations { get; set; } = null!;
    public DbSet<TaskColumnHistory> TaskColumnHistories { get; set; } = null!;
    public DbSet<TaskAttachment> TaskAttachments { get; set; } = null!;
    public DbSet<TaskComment> TaskComments { get; set; } = null!;
    public DbSet<TaskOwnerHistory> TaskOwnerHistories { get; set; } = null!;
    public DbSet<WorkOrderHeader> WorkOrderHeaders { get; set; } = null!;
    public DbSet<WorkOrderRow> WorkOrderRows { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.Task>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(5000);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(Models.TaskStatus.ToDo);

            entity.Property(e => e.Priority)
                .IsRequired()
                .HasDefaultValue(Models.TaskPriority.Medium);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ColumnId);

            // Configure relationship with Column
            entity.HasOne(e => e.Column)
                .WithMany(c => c.Tasks)
                .HasForeignKey(e => e.ColumnId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Column>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Order)
                .IsRequired();

            entity.Property(e => e.Color)
                .HasMaxLength(20)
                .HasDefaultValue("#DFE1E6");

            entity.Property(e => e.IsSystem)
                .IsRequired()
                .HasDefaultValue(false);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.HasIndex(e => e.Order);
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<ColumnTranslation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Culture)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            // Configure relationship with Column
            entity.HasOne(e => e.Column)
                .WithMany(c => c.Translations)
                .HasForeignKey(e => e.ColumnId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: one translation per culture per column
            entity.HasIndex(e => new { e.ColumnId, e.Culture })
                .IsUnique();
        });

        modelBuilder.Entity<TaskColumnHistory>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ChangedAt)
                .IsRequired();

            entity.Property(e => e.ChangedBy)
                .HasMaxLength(200);

            // Configure relationship with Task
            entity.HasOne(e => e.Task)
                .WithMany()
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with FromColumn (optional)
            entity.HasOne(e => e.FromColumn)
                .WithMany()
                .HasForeignKey(e => e.FromColumnId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure relationship with ToColumn
            entity.HasOne(e => e.ToColumn)
                .WithMany()
                .HasForeignKey(e => e.ToColumnId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for efficient querying by task
            entity.HasIndex(e => e.TaskId);
            entity.HasIndex(e => e.ChangedAt);
        });

        modelBuilder.Entity<TaskAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.StoredFileName)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.ContentType)
                .HasMaxLength(200);

            entity.Property(e => e.UploadedBy)
                .HasMaxLength(200);

            entity.Property(e => e.UploadedAt)
                .IsRequired();

            // Configure relationship with Task
            entity.HasOne(e => e.Task)
                .WithMany()
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with Column (phase)
            entity.HasOne(e => e.Column)
                .WithMany()
                .HasForeignKey(e => e.ColumnId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for efficient querying
            entity.HasIndex(e => e.TaskId);
            entity.HasIndex(e => e.ColumnId);
            entity.HasIndex(e => e.UploadedAt);
        });

        modelBuilder.Entity<TaskComment>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Content)
                .IsRequired()
                .HasMaxLength(5000);

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(200);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            // Configure relationship with Task
            entity.HasOne(e => e.Task)
                .WithMany()
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationship with Column (phase)
            entity.HasOne(e => e.Column)
                .WithMany()
                .HasForeignKey(e => e.ColumnId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for efficient querying
            entity.HasIndex(e => e.TaskId);
            entity.HasIndex(e => e.ColumnId);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<TaskOwnerHistory>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ChangedAt)
                .IsRequired();

            // Indexes for efficient querying
            entity.HasIndex(e => e.TaskId);
            entity.HasIndex(e => e.ChangedAt);
        });

        modelBuilder.Entity<WorkOrderHeader>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TaskId)
                .IsRequired();

            // Configure one-to-one relationship with Task
            entity.HasOne(e => e.Task)
                .WithOne(t => t.WorkOrder)
                .HasForeignKey<WorkOrderHeader>(e => e.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // String properties with max lengths
            entity.Property(e => e.Customer).HasMaxLength(200);
            entity.Property(e => e.Object).HasMaxLength(200);
            entity.Property(e => e.Product).HasMaxLength(200);
            entity.Property(e => e.WorkOrderNumber).HasMaxLength(50);
            entity.Property(e => e.OrderReference).HasMaxLength(100);
            entity.Property(e => e.DeliveryReference).HasMaxLength(100);
            entity.Property(e => e.PageNumber).HasMaxLength(20);
            entity.Property(e => e.Material).HasMaxLength(200);
            entity.Property(e => e.Surface).HasMaxLength(100);
            entity.Property(e => e.Cut).HasMaxLength(100);
            entity.Property(e => e.Processing).HasMaxLength(100);
            entity.Property(e => e.Packing).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            // Indexes
            entity.HasIndex(e => e.TaskId).IsUnique();
            entity.HasIndex(e => e.WorkOrderNumber);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.UpdatedAt);
        });

        modelBuilder.Entity<WorkOrderRow>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.WorkOrderHeaderId)
                .IsRequired();

            entity.Property(e => e.RowNumber)
                .IsRequired();

            // Configure relationship with WorkOrderHeader
            entity.HasOne(e => e.WorkOrderHeader)
                .WithMany(h => h.Rows)
                .HasForeignKey(e => e.WorkOrderHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            // String properties with max lengths
            entity.Property(e => e.PositionPZ).HasMaxLength(10);
            entity.Property(e => e.PositionP1).HasMaxLength(10);
            entity.Property(e => e.PositionR).HasMaxLength(10);
            entity.Property(e => e.PositionO).HasMaxLength(10);
            entity.Property(e => e.PositionP2).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ProcessingNotes).HasMaxLength(500);

            // Decimal properties with precision
            entity.Property(e => e.Length).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Width).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Thickness).HasColumnType("decimal(18,2)");
            entity.Property(e => e.SquareMeters).HasColumnType("decimal(18,4)");
            entity.Property(e => e.SquareMetersTot).HasColumnType("decimal(18,4)");
            entity.Property(e => e.LinearMeters).HasColumnType("decimal(18,4)");
            entity.Property(e => e.CubicMetersTot).HasColumnType("decimal(18,6)");
            entity.Property(e => e.WeightKg).HasColumnType("decimal(18,3)");
            entity.Property(e => e.MaterialDensity).HasColumnType("decimal(18,2)");

            // Indexes
            entity.HasIndex(e => e.WorkOrderHeaderId);
            entity.HasIndex(e => new { e.WorkOrderHeaderId, e.RowNumber }).IsUnique();
        });
    }
}
