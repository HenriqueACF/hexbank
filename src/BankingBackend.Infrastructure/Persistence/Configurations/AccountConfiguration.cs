using BankingBackend.Core.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingBackend.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.UserId).HasColumnName("user_id").IsRequired();

        builder.Property(a => a.Number).HasColumnName("number").HasMaxLength(8).IsRequired();

        builder.Property(a => a.Balance).HasColumnName("balance")
            .HasConversion(money => money.Amount, value => Money.Create(value).Value).IsRequired();
        builder.Property(a => a.IsActive).HasColumnName("is_active").IsRequired();

        builder.Property(a => a.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        builder.Ignore(a => a.DomainEvents);
    }
}