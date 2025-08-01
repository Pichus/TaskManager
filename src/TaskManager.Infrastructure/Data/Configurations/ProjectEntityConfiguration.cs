using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.ProjectAggregate;

namespace TaskManager.Infrastructure.Data.Configurations;

public class ProjectEntityConfiguration : IEntityTypeConfiguration<ProjectEntity>
{
    public void Configure(EntityTypeBuilder<ProjectEntity> builder)
    {
        builder
            .HasMany(e => e.Tasks)
            .WithOne(e => e.Project)
            .HasForeignKey(e => e.ProjectId)
            .IsRequired();

        builder
            .HasMany(e => e.Members)
            .WithMany(e => e.MemberProjects);
    }
}