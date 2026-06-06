using ChangeRequestAnalyzer.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChangeRequestAnalyzer.Infrastructure.Data
{
    public sealed class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChangeRequest> ChangeRequests => Set<ChangeRequest>();
        public DbSet<UserStory> UserStories => Set<UserStory>();
        public DbSet<AnalysisResult> AnalysisResults => Set<AnalysisResult>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ChangeRequest>(entity =>
            {
                entity.ToTable("ChangeRequests");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(4000);

                entity.Property(e => e.DocumentFileName)
                    .HasMaxLength(260);

                entity.Property(e => e.DocumentContent)
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.Status)
                    .IsRequired();

                entity.HasMany(e => e.UserStories)
                    .WithOne(e => e.ChangeRequest)
                    .HasForeignKey(e => e.ChangeRequestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.AnalysisResults)
                    .WithOne(e => e.ChangeRequest)
                    .HasForeignKey(e => e.ChangeRequestId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserStory>(entity =>
            {
                entity.ToTable("UserStories");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.StoryIdentifier)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.RequirementText)
                    .IsRequired()
                    .HasMaxLength(2000);
            });

            builder.Entity<AnalysisResult>(entity =>
            {
                entity.ToTable("AnalysisResults");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.TargetFiles)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.ConflictingFiles)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.GeneratedPrompt)
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                entity.Property(e => e.AnalyzedAt)
                    .IsRequired();
            });
        }
    }
}
