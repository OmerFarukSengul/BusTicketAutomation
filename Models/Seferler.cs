using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("Seferler")]
[Index("FirmaId", Name = "IX_Seferler_FirmaId")]
[Index("KalkisTarihiSaat", Name = "IX_Seferler_KalkisTarihiSaat")]
[Index("OtobusId", Name = "IX_Seferler_OtobusId")]
[Index("RotaId", Name = "IX_Seferler_RotaId")]
public partial class Seferler
{
    [Key]
    public int Id { get; set; }

    public int RotaId { get; set; }

    public int FirmaId { get; set; }

    public int OtobusId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime KalkisTarihiSaat { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime VarisTarihiSaat { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Fiyat { get; set; }

    [StringLength(250)]
    public string? Aciklama { get; set; }

    public bool AktifMi { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime OlusturmaTarihi { get; set; }

    [ForeignKey("FirmaId")]
    [InverseProperty("Seferlers")]
    public virtual Firmalar Firma { get; set; } = null!;

    [ForeignKey("OtobusId")]
    [InverseProperty("Seferlers")]
    public virtual Otobusler Otobus { get; set; } = null!;

    [InverseProperty("Sefer")]
    public virtual ICollection<Rezervasyonlar> Rezervasyonlars { get; set; } = new List<Rezervasyonlar>();

    [ForeignKey("RotaId")]
    [InverseProperty("Seferlers")]
    public virtual Rotalar Rota { get; set; } = null!;

    [InverseProperty("Sefer")]
    public virtual ICollection<SeferDuraklari> SeferDuraklaris { get; set; } = new List<SeferDuraklari>();
}
