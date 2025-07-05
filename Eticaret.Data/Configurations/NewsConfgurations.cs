using Eticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eticaret.Data.Configuratons
{
    internal class NewsConfiguration: IEntityTypeConfiguration<News>
    {
        public void Configure(EntityTypeBuilder<News> builder)
{
    builder.Property(x => x.Name).IsRequired().HasMaxLength(250);
    builder.Property(x => x.Description).HasMaxLength(750);
    builder.Property(x => x.Image).IsRequired().HasMaxLength(100);
    
    
}

    }
}

