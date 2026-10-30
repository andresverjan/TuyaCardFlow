using TuyaCardFlow.Modules.Cards.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TuyaCardFlow.Modules.Cards.Infrastructure.Persistence.Configurations;

public sealed class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("Cards", table =>
        {
            table.HasCheckConstraint("CK_Cards_ExpirationMonth", "\"ExpirationMonth\" BETWEEN 1 AND 12");
            table.HasCheckConstraint("CK_Cards_ExpirationYear", "\"ExpirationYear\" BETWEEN 2000 AND 9999");
            table.HasCheckConstraint("CK_Cards_Last4", "\"Last4\" ~ '^[0-9]{4}$'");
        });

        builder.HasKey(card => card.Id);
        builder.Property(card => card.Id).ValueGeneratedNever();
        builder.Property(card => card.UserId).IsRequired();
        builder.HasIndex(card => card.UserId);
        builder.Property(card => card.HolderName).HasMaxLength(150).IsRequired();
        builder.Property(card => card.Brand).HasMaxLength(50).IsRequired();
        builder.Property(card => card.Last4).HasMaxLength(4).IsRequired();
        builder.Property(card => card.ExpirationMonth).IsRequired();
        builder.Property(card => card.ExpirationYear).IsRequired();
    }
}
