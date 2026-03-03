using Microsoft.EntityFrameworkCore;
using ExcelCompare.Domain.Entities;

namespace ExcelCompare.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UploadBatch> UploadBatches { get; set; }
    public DbSet<SentRecord> SentRecords { get; set; }
    public DbSet<ReceivedRecord> ReceivedRecords { get; set; }
    public DbSet<UploadDuplicate> UploadDuplicates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UploadBatch configuration
        modelBuilder.Entity<UploadBatch>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
            entity.Property(e => e.IsUpdate).IsRequired().HasDefaultValue(false);
            
            entity.HasOne(e => e.OriginalBatch)
                .WithMany()
                .HasForeignKey(e => e.OriginalBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.OriginalBatchId)
                .HasDatabaseName("IX_UploadBatches_OriginalBatchId");
        });

        // SentRecord configuration
        modelBuilder.Entity<SentRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.UploadBatch)
                .WithMany(b => b.SentRecords)
                .HasForeignKey(e => e.UploadBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite index on mem + nid
            entity.HasIndex(e => new { e.Mem, e.Nid })
                .HasDatabaseName("IX_SentRecords_Mem_Nid");

            // Individual indexes for duplicate detection
            entity.HasIndex(e => e.Mem)
                .HasDatabaseName("IX_SentRecords_Mem");
            
            entity.HasIndex(e => e.Nid)
                .HasDatabaseName("IX_SentRecords_Nid");

            // Index on UploadBatchId
            entity.HasIndex(e => e.UploadBatchId)
                .HasDatabaseName("IX_SentRecords_UploadBatchId");

            entity.Property(e => e.Mem).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Nid).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Sn).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.BatchNo).HasMaxLength(100);
            entity.Property(e => e.MemberRank).HasMaxLength(100);
            entity.Property(e => e.RefSn).HasMaxLength(100);
        });

        // ReceivedRecord configuration
        modelBuilder.Entity<ReceivedRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.UploadBatch)
                .WithMany(b => b.ReceivedRecords)
                .HasForeignKey(e => e.UploadBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite index on mem + nid
            entity.HasIndex(e => new { e.Mem, e.Nid })
                .HasDatabaseName("IX_ReceivedRecords_Mem_Nid");

            // Individual indexes for duplicate detection
            entity.HasIndex(e => e.Mem)
                .HasDatabaseName("IX_ReceivedRecords_Mem");
            
            entity.HasIndex(e => e.Nid)
                .HasDatabaseName("IX_ReceivedRecords_Nid");

            // Index on UploadBatchId
            entity.HasIndex(e => e.UploadBatchId)
                .HasDatabaseName("IX_ReceivedRecords_UploadBatchId");

            entity.Property(e => e.Mem).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Nid).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Sn).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.BatchNo).HasMaxLength(100);
            entity.Property(e => e.MemberRank).HasMaxLength(100);
            entity.Property(e => e.RefSn).HasMaxLength(100);
        });

        // UploadDuplicate configuration
        modelBuilder.Entity<UploadDuplicate>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.UploadBatch)
                .WithMany()
                .HasForeignKey(e => e.UploadBatchId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ConflictingBatch)
                .WithMany()
                .HasForeignKey(e => e.ConflictingBatchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.UploadBatchId)
                .HasDatabaseName("IX_UploadDuplicates_UploadBatchId");

            entity.HasIndex(e => new { e.Mem, e.Nid })
                .HasDatabaseName("IX_UploadDuplicates_Mem_Nid");

            entity.Property(e => e.DuplicateType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Mem).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Nid).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
        });
    }
}
