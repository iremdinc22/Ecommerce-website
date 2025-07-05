using System.ComponentModel.DataAnnotations;

namespace Eticaret.Core.Enums // veya senin uygun gördüğün bir namespace
{
    public enum EnumOrderState
    {
        [Display(Name = "Onay Bekliyor")]
        Waiting,

        [Display(Name = "Onaylandı")]
        Approved,

        [Display(Name = "Kargoya Verildi")]
        Shipped,

        [Display(Name = "Tamamlandı")]
        Conpleted,

        [Display(Name = "İptal Edildi")]
        Cancelled,

        [Display(Name = "İade Edildi")]
        Returned
    }
}
