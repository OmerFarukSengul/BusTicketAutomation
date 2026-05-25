using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("Otobusler")]
[Index("FirmaId", Name = "IX_Otobusler_FirmaId")]
[Index("Plaka", Name = "UQ__Otobusle__830E30F740084D00", IsUnique = true)]
public partial class Otobusler
{
    [Key]
    public int Id { get; set; }

    public int FirmaId { get; set; }

    [StringLength(20)]
    public string Plaka { get; set; } = null!;

    public int KoltukSayisi { get; set; }

    [StringLength(50)]
    public string? Model { get; set; }

    public bool AktifMi { get; set; }

    [ForeignKey("FirmaId")]
    [InverseProperty("Otobuslers")]
    public virtual Firmalar Firma { get; set; } = null!;

    [InverseProperty("Otobus")]
    public virtual ICollection<Seferler> Seferlers { get; set; } = new List<Seferler>();
}
