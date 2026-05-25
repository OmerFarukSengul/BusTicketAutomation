using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("Sehirler")]
[Index("SehirAdi", Name = "UQ__Sehirler__7B189908B0DB8A3D", IsUnique = true)]
public partial class Sehirler
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string SehirAdi { get; set; } = null!;

    public bool AktifMi { get; set; }

    [InverseProperty("Sehir")]
    public virtual ICollection<RotaDuraklari> RotaDuraklaris { get; set; } = new List<RotaDuraklari>();

    [InverseProperty("NeredenSehir")]
    public virtual ICollection<Rotalar> RotalarNeredenSehirs { get; set; } = new List<Rotalar>();

    [InverseProperty("NereyeSehir")]
    public virtual ICollection<Rotalar> RotalarNereyeSehirs { get; set; } = new List<Rotalar>();

    [InverseProperty("Sehir")]
    public virtual ICollection<SeferDuraklari> SeferDuraklaris { get; set; } = new List<SeferDuraklari>();
}
