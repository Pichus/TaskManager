using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.ProjectAggregate;

namespace TaskManager.Infrastructure.Data.Configurations;

public class MemberRoleEntityConfiguration : IEntityTypeConfiguration<MemberRole>
{
    public void Configure(EntityTypeBuilder<MemberRole> builder)
    {
        builder
            .HasIndex(e => e.ProjectMemberId)
            .IsUnique();
        
        builder
            .Property(e => e.Role)
            .HasConversion<string>();
    }
}