using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.ProjectAggregate;

namespace TaskManager.Infrastructure.Data.Configurations;

public class MemberRoleEntityConfiguration : IEntityTypeConfiguration<MemberRole>
{
    public void Configure(EntityTypeBuilder<MemberRole> builder)
    {
        builder.HasKey(e => new { e.UserId, e.ProjectId });
        
        builder
            .Property(e => e.Role)
            .HasConversion<string>();
    }
}