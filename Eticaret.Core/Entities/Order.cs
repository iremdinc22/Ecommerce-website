using System.ComponentModel.DataAnnotations;
using Eticaret.Core.Enums; // enum'un yeni yeri

namespace Eticaret.Core.Entities
{
    public class Order : IEntity
    {
        public int Id { get; set; }

        [Display(Name = "Sipariş No"), StringLength(50)]
        public string OrderNumber { get; set; }

        [Display(Name = "Sipariş Toplamı")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Müşteri No")]
        public int AppUserId { get; set; }

        [Display(Name = "Müşteri"), StringLength(200)]

        public AppUser? AppUser { get; set; }

        public string CustomerId { get; set; }

        [Display(Name = "Fatura Adresi"), StringLength(200)]
        public string BillingAddress { get; set; }

        [Display(Name = "Teslimat Adresi"), StringLength(200)]
        public string DeliveryAddress { get; set; }

        [Display(Name = "Sipariş Tarihi")]
        public DateTime OrderDate { get; set; }

        public ICollection<OrderLine> OrderLines { get; set; }
        public EnumOrderState? OrderState { get; set; }

        
    }
}
