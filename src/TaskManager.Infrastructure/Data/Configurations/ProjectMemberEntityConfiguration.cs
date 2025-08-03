using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.UserAggregate;

namespace TaskManager.Infrastructure.Data.Configurations;

public class ProjectMemberEntityConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.HasKey(e => new { e.ProjectId, e.MemberId });

        builder
            .HasOne(e => e.Project)
            .WithMany(e => e.Members)
            .HasForeignKey(e => e.ProjectId);

        builder
            .HasOne<TaskManagerUser>()
            .WithMany()
            .HasForeignKey(e => e.MemberId);
    }
}