using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("Firmalar")]
public partial class Firmalar
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    public string FirmaAdi { get; set; } = null!;

    [StringLength(20)]
    public string? Telefon { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? LogoUrl { get; set; }

    public bool AktifMi { get; set; }

    [InverseProperty("Firma")]
    public virtual ICollection<Otobusler> Otobuslers { get; set; } = new List<Otobusler>();

    [InverseProperty("Firma")]
    public virtual ICollection<Seferler> Seferlers { get; set; } = new List<Seferler>();
}
