using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuyaCardFlow.Modules.Users.Domain;

namespace TuyaCardFlow.Modules.Users.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public const string EmailUniqueIndex = "IX_Users_NormalizedEmail";

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.FullName).HasMaxLength(UserProfile.MaximumNameLength).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(UserProfile.MaximumEmailLength).IsRequired();
        builder.Property(user => user.NormalizedEmail).HasMaxLength(UserProfile.MaximumEmailLength).IsRequired();
        builder.Property(user => user.IsActive).IsRequired();
        builder.Property(user => user.PasswordHash).HasMaxLength(1024);
        builder.Property(user => user.AuthenticationVersion).IsRequired();
        builder.Property(user => user.CreatedAtUtc).IsRequired();
        builder.Property(user => user.UpdatedAtUtc).IsRequired();
        builder.HasIndex(user => user.NormalizedEmail).IsUnique().HasDatabaseName(EmailUniqueIndex);
        builder.HasIndex(user => new { user.CreatedAtUtc, user.Id });
    }
}
