using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.UserAggregate;

namespace TaskManager.Infrastructure.Data.Configurations;

public class TaskManagerUserEntityConfiguration : IEntityTypeConfiguration<TaskManagerUser>
{
    public void Configure(EntityTypeBuilder<TaskManagerUser> builder)
    {
        builder
            .HasMany(e => e.Projects)
            .WithOne(e => e.LeadUser)
            .HasForeignKey(e => e.LeadUserId)
            .IsRequired();
    }
}