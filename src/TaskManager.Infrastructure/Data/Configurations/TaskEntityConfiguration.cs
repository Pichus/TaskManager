using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.TaskAggregate;
using TaskManager.Infrastructure.Identity.User;

namespace TaskManager.Infrastructure.Data.Configurations;

public class TaskEntityConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder
            .HasOne<TaskManagerUser>()
            .WithMany(e => e.AssignedTasks)
            .HasForeignKey(e => e.AssigneeUserId)
            .IsRequired(false);

        builder
            .HasOne<TaskManagerUser>()
            .WithMany(e => e.CreatedTasks)
            .HasForeignKey(e => e.CreatedByUserId)
            .IsRequired();

        builder
            .Property(e => e.Status)
            .HasConversion<string>();
    }
}