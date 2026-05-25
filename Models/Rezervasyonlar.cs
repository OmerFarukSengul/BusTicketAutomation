using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("Rezervasyonlar")]
[Index("KullaniciId", Name = "IX_Rezervasyonlar_KullaniciId")]
[Index("SeferId", Name = "IX_Rezervasyonlar_SeferId")]
[Index("PnrNo", Name = "UQ__Rezervas__0FA22DC4885ACD9C", IsUnique = true)]
public partial class Rezervasyonlar
{
    [Key]
    public int Id { get; set; }

    public int? KullaniciId { get; set; }

    public int SeferId { get; set; }

    [StringLength(20)]
    public string PnrNo { get; set; } = null!;

    [StringLength(50)]
    public string YolcuAd { get; set; } = null!;

    [StringLength(50)]
    public string YolcuSoyad { get; set; } = null!;

    [StringLength(20)]
    public string? YolcuTcNo { get; set; }

    [StringLength(20)]
    public string YolcuTelefon { get; set; } = null!;

    [StringLength(100)]
    public string? YolcuEmail { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal ToplamTutar { get; set; }

    [StringLength(30)]
    public string Durum { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime RezervasyonTarihi { get; set; }

    [ForeignKey("KullaniciId")]
    [InverseProperty("Rezervasyonlars")]
    public virtual Kullanicilar? Kullanici { get; set; }

    [InverseProperty("Rezervasyon")]
    public virtual ICollection<Odemeler> Odemelers { get; set; } = new List<Odemeler>();

    [InverseProperty("Rezervasyon")]
    public virtual ICollection<RezervasyonKoltuklari> RezervasyonKoltuklaris { get; set; } = new List<RezervasyonKoltuklari>();

    [ForeignKey("SeferId")]
    [InverseProperty("Rezervasyonlars")]
    public virtual Seferler Sefer { get; set; } = null!;
}
