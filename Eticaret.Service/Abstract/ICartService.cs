using Eticaret.Core.Entities;
using System.Linq.Expressions;

namespace Eticaret.Service.Abstract 
{
    public interface ICartService 
 {
    void AddProduct(Product product , int quantity);
    void UpdateProduct(Product product , int quantity);
    void RemoveProduct(Product product );
    decimal TotalPrice();
    void ClearAll(); // sepeti boşlat aksiyonu tek seferde temizleyebilmek için

 }

}
