using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using Eticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Eticaret.Data.Configuratons
{
    internal class CategoryConfiguration: IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
{
    builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
    builder.Property(x => x.Image).HasMaxLength(50);
    
    builder.HasData(
        new Category
        {
            Name = "Elektronik",
            Id =1 ,
            IsActive = true,
            IsTopMenu = true,
            ParentId = null,
            OrderNo=1,
            
        },

        new Category
        {
            Name = "Bilgisayar",
            Id =2 ,
            IsActive = true,
            IsTopMenu = true,
            ParentId = null,
            OrderNo=2,
            
        }
    );
}

    }
}

