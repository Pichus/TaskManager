using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.UserAggregate;

namespace TaskManager.Infrastructure.Data.Configurations;

public class ProjectEntityConfiguration : IEntityTypeConfiguration<ProjectEntity>
{
    public void Configure(EntityTypeBuilder<ProjectEntity> builder)
    {
        builder
            .HasOne<TaskManagerUser>()
            .WithMany(e => e.LedProjects)
            .HasForeignKey(e => e.LeadUserId)
            .IsRequired();
        
        builder
            .HasMany(e => e.Tasks)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId)
            .IsRequired();
    }
}