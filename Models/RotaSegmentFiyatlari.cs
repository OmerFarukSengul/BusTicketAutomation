using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OtobusFinalProje.Models
{
    [Table("RotaSegmentFiyatlari")]
    public class RotaSegmentFiyatlari
    {
        [Key]
        public int Id { get; set; }

        public int RotaId { get; set; }

        public int BinisSehirId { get; set; }

        public int InisSehirId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Fiyat { get; set; }

        public int? SureDakika { get; set; }

        public bool AktifMi { get; set; } = true;

        // RELATIONS
        [ForeignKey("RotaId")]
        public virtual Rotalar Rota { get; set; }

        [ForeignKey("BinisSehirId")]
        public virtual Sehirler BinisSehir { get; set; }

        [ForeignKey("InisSehirId")]
        public virtual Sehirler InisSehir { get; set; }
    }
}