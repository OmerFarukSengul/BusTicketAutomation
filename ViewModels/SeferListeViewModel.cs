namespace OtobusFinalProje.ViewModels
{
    public class SeferListeViewModel
    {
        public int SeferId { get; set; }

        public int BinisDurakId { get; set; }
        public int InisDurakId { get; set; }

        public string FirmaAdi { get; set; } = string.Empty;
        public string Nereden { get; set; } = string.Empty;
        public string Nereye { get; set; } = string.Empty;

        public DateTime KalkisTarihiSaat { get; set; }
        public DateTime VarisTarihiSaat { get; set; }

        public decimal Fiyat { get; set; }

        public int KoltukSayisi { get; set; }
        public int DoluKoltukSayisi { get; set; }
    }
}