using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace OtobusFinalProje.ViewModels
{
    public class RezervasyonOlusturViewModel
    {
        public int SeferId { get; set; }

        public string FirmaAdi { get; set; } = "";
        public string Nereden { get; set; } = "";
        public string Nereye { get; set; } = "";

        public DateTime KalkisTarihiSaat { get; set; }
        public DateTime VarisTarihiSaat { get; set; }

        public decimal Fiyat { get; set; }
        public int KoltukSayisi { get; set; }

        public List<int> DoluKoltuklar { get; set; } = new();

        public List<SelectListItem> Duraklar { get; set; } = new();

        [Required(ErrorMessage = "Biniş durağı seçiniz.")]
        public int BinisDurakId { get; set; }

        [Required(ErrorMessage = "İniş durağı seçiniz.")]
        public int InisDurakId { get; set; }

        [Required(ErrorMessage = "Koltuk seçiniz.")]
        public int SecilenKoltukNo { get; set; }
    }
}