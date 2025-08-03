using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Infrastructure.Identity.User;

namespace TaskManager.Infrastructure.Data.Configurations;

public class TaskManagerUserEntityConfiguration : IEntityTypeConfiguration<TaskManagerUser>
{
    public void Configure(EntityTypeBuilder<TaskManagerUser> builder)
    {

    }
}