using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Infrastructure.Identity.User;

namespace TaskManager.Infrastructure.Data.Configurations;

public class ProjectMemberEntityConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder
            .HasIndex(e => new { e.ProjectId, e.MemberId })
            .IsUnique();

        builder
            .HasOne(e => e.Project)
            .WithMany(e => e.Members)
            .HasForeignKey(e => e.ProjectId)
            .IsRequired();

        builder
            .HasOne<TaskManagerUser>()
            .WithMany()
            .HasForeignKey(e => e.MemberId)
            .IsRequired();

        builder
            .HasOne(e => e.MemberRole)
            .WithOne(e => e.ProjectMember)
            .IsRequired();
    }
}