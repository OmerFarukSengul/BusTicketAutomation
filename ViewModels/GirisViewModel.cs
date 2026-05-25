using System.ComponentModel.DataAnnotations;

namespace OtobusFinalProje.ViewModels
{
    public class GirisViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Sifre { get; set; } = "";
    }
}