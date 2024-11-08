using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BasicApp.Data.Domain;

namespace BasicApp.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // BaseModel configuration
        builder.Property(x => x.CreatedById).IsRequired();
        builder.Property(x => x.UpdatedById).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.InsertDate).IsRequired();
        builder.Property(x => x.UpdateDate).IsRequired(false);
        builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);

        // User Model Configuration;
        builder.Property(a => a.UserName).HasMaxLength(50).IsRequired();
        builder.Property(a => a.Role).IsRequired();
        builder.Property(a => a.Password).IsRequired();
        builder.Property(a => a.Email).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PasswordRetryCount).HasDefaultValue(0);
    }
}
