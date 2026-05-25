using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OtobusFinalProje.ViewModels
{
    public class SeferAraViewModel
    {
        [Required(ErrorMessage = "Nereden şehir seçiniz.")]
        public int NeredenSehirId { get; set; }

        [Required(ErrorMessage = "Nereye şehir seçiniz.")]
        public int NereyeSehirId { get; set; }

        [Required(ErrorMessage = "Tarih seçiniz.")]
        public DateTime Tarih { get; set; } = DateTime.Today;

        public List<SelectListItem> Sehirler { get; set; } = new();
    }
}