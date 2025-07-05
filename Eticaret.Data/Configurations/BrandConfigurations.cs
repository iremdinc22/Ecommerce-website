using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using Eticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eticaret.Data.Configuratons
{
    internal class BrandConfiguration: IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
{
    builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
    builder.Property(x => x.Logo).HasMaxLength(50);

}

    }
}

