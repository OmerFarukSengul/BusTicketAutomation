using System.ComponentModel.DataAnnotations;

namespace OtobusFinalProje.ViewModels
{
    public class KayitViewModel
    {
        [Required]
        public string Ad { get; set; } = "";

        [Required]
        public string Soyad { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public string? Telefon { get; set; }

        [Required]
        public string Sifre { get; set; } = "";

        [Required]
        [Compare("Sifre", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string SifreTekrar { get; set; } = "";
    }
}