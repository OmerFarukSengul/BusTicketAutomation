using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("Rotalar")]
[Index("NeredenSehirId", Name = "IX_Rotalar_NeredenSehirId")]
[Index("NereyeSehirId", Name = "IX_Rotalar_NereyeSehirId")]
public partial class Rotalar
{
    [Key]
    public int Id { get; set; }

    public int NeredenSehirId { get; set; }

    public int NereyeSehirId { get; set; }

    public int? MesafeKm { get; set; }

    public int? TahminiSureDakika { get; set; }

    public bool AktifMi { get; set; }

    [ForeignKey("NeredenSehirId")]
    [InverseProperty("RotalarNeredenSehirs")]
    public virtual Sehirler NeredenSehir { get; set; } = null!;

    [ForeignKey("NereyeSehirId")]
    [InverseProperty("RotalarNereyeSehirs")]
    public virtual Sehirler NereyeSehir { get; set; } = null!;

    [InverseProperty("Rota")]
    public virtual ICollection<RotaDuraklari> RotaDuraklaris { get; set; } = new List<RotaDuraklari>();

    [InverseProperty("Rota")]
    public virtual ICollection<Seferler> Seferlers { get; set; } = new List<Seferler>();
}
