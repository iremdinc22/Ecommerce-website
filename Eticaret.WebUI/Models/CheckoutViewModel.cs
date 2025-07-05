using System.ComponentModel.DataAnnotations;
using Eticaret.Core.Entities;

public class CheckoutViewModel
{
    // Sepet ve adres bilgileri
    public List<CartLine>? CartProducts { get; set; }
    public decimal TotalPrice { get; set; }
    public List<Address>? Addresses { get; set; }

    // Adres seçimleri
    [Required(ErrorMessage = "Teslimat adresi seçilmelidir.")]
    public string DeliveryAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fatura adresi seçilmelidir.")]
    public string BillingAddress { get; set; } = string.Empty;

    // Kart bilgileri
    [Required(ErrorMessage = "Kart sahibi adı zorunludur.")]
    [Display(Name = "Kart Sahibi")]
    public string CardHolderName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kart numarası zorunludur.")]
    [Display(Name = "Kart Numarası")]
    public string CardNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ay seçilmelidir.")]
    [Display(Name = "Ay")]
    public string CardMonth { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yıl seçilmelidir.")]
    [Display(Name = "Yıl")]
    public string CardYear { get; set; } = string.Empty;

    [Required(ErrorMessage = "CVV kodu zorunludur.")]
    [Display(Name = "CVV")]
    public string CVV { get; set; } = string.Empty;

    [Display(Name = "Ön bilgilendirme formunu ve mesafeli satış sözleşmesini okudum, onaylıyorum.")]
    [Required(ErrorMessage = "Sözleşmeyi onaylamanız gerekmektedir.")]
    public bool IsAgreementChecked { get; set; }

    public Address? BillingAddressObject =>
        Addresses?.FirstOrDefault(a => a.AddressGuid.ToString() == BillingAddress);

    public Address? DeliveryAddressObject =>
        Addresses?.FirstOrDefault(a => a.AddressGuid.ToString() == DeliveryAddress);
}
