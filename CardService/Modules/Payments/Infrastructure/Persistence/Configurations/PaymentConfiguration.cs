using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuyaCardFlow.Modules.Payments.Domain;

namespace TuyaCardFlow.Modules.Payments.Infrastructure.Persistence.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", table =>
        {
            table.HasCheckConstraint("CK_Payments_Amount", "\"Amount\" > 0");
            table.HasCheckConstraint("CK_Payments_Currency", "\"Currency\" ~ '^[A-Z]{3}$'");
        });
        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id).ValueGeneratedNever();
        builder.Property(payment => payment.UserId).IsRequired();
        builder.Property(payment => payment.CardId).IsRequired();
        builder.Property(payment => payment.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(payment => payment.Currency).HasMaxLength(3).IsRequired();
        builder.Property(payment => payment.Description).HasMaxLength(Payment.MaximumDescriptionLength);
        builder.Property(payment => payment.OccurredAtUtc).IsRequired();
        builder.HasIndex(payment => new { payment.UserId, payment.OccurredAtUtc, payment.Id });
        builder.HasIndex(payment => payment.CardId);
    }
}
