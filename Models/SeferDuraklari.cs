using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("SeferDuraklari")]
public partial class SeferDuraklari
{
    [Key]
    public int Id { get; set; }

    public int SeferId { get; set; }

    public int SehirId { get; set; }

    public int SiraNo { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? VarisSaati { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? KalkisSaati { get; set; }

    public bool AktifMi { get; set; }

    [InverseProperty("BinisDurak")]
    public virtual ICollection<RezervasyonKoltuklari> RezervasyonKoltuklariBinisDuraks { get; set; } = new List<RezervasyonKoltuklari>();

    [InverseProperty("InisDurak")]
    public virtual ICollection<RezervasyonKoltuklari> RezervasyonKoltuklariInisDuraks { get; set; } = new List<RezervasyonKoltuklari>();

    [ForeignKey("SeferId")]
    [InverseProperty("SeferDuraklaris")]
    public virtual Seferler Sefer { get; set; } = null!;

    [ForeignKey("SehirId")]
    [InverseProperty("SeferDuraklaris")]
    public virtual Sehirler Sehir { get; set; } = null!;
}
