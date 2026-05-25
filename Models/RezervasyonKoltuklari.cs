using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("RezervasyonKoltuklari")]
[Index("RezervasyonId", "KoltukNo", Name = "UQ_RezervasyonKoltuklari", IsUnique = true)]
public partial class RezervasyonKoltuklari
{
    [Key]
    public int Id { get; set; }

    public int RezervasyonId { get; set; }

    public int KoltukNo { get; set; }

    [StringLength(10)]
    public string? Cinsiyet { get; set; }

    public int? BinisDurakId { get; set; }

    public int? InisDurakId { get; set; }

    [ForeignKey("BinisDurakId")]
    [InverseProperty("RezervasyonKoltuklariBinisDuraks")]
    public virtual SeferDuraklari? BinisDurak { get; set; }

    [ForeignKey("InisDurakId")]
    [InverseProperty("RezervasyonKoltuklariInisDuraks")]
    public virtual SeferDuraklari? InisDurak { get; set; }

    [ForeignKey("RezervasyonId")]
    [InverseProperty("RezervasyonKoltuklaris")]
    public virtual Rezervasyonlar Rezervasyon { get; set; } = null!;
}
