using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Core.ProjectAggregate;
using TaskManager.Core.ProjectInviteAggregate;
using TaskManager.Infrastructure.Identity.User;

namespace TaskManager.Infrastructure.Data.Configurations;

public class ProjectInviteEntityConfiguration : IEntityTypeConfiguration<ProjectInvite>
{
    public void Configure(EntityTypeBuilder<ProjectInvite> builder)
    {
        builder
            .HasKey(e => e.Id);
        
        builder
            .HasAlternateKey(e => new { e.ProjectId, e.InvitedUserId });

        builder
            .HasOne(e => e.Project)
            .WithMany(e => e.Invites)
            .HasForeignKey(e => e.ProjectId)
            .IsRequired();

        builder
            .HasOne<TaskManagerUser>()
            .WithMany(e => e.Invites)
            .HasForeignKey(e => e.InvitedUserId)
            .IsRequired();

        builder
            .Property(e => e.Status)
            .HasConversion<string>();
    }
}