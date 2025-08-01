using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.UserAggregate;

namespace TaskManager.Infrastructure.Data.Configurations;

public class TaskManagerUserEntityConfiguration : IEntityTypeConfiguration<TaskManagerUser>
{
    public void Configure(EntityTypeBuilder<TaskManagerUser> builder)
    {
        builder
            .HasMany(e => e.LedProjects)
            .WithOne(e => e.ProjectLead)
            .HasForeignKey(e => e.LeadUserId)
            .IsRequired();

        builder
            .HasMany(e => e.CreatedTasks)
            .WithOne(e => e.CreatedByUser)
            .HasForeignKey(e => e.CreatedByUserId)
            .IsRequired();

        builder
            .HasMany(e => e.AssignedTasks)
            .WithOne(e => e.Assignee)
            .HasForeignKey(e => e.AssigneeUserId)
            .IsRequired(false);
    }
}