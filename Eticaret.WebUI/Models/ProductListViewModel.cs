using Eticaret.Core.Entities;

namespace Eticaret.WebUI.Models
{
    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new();  // non-nullable için başlangıç değeri
    }
}
