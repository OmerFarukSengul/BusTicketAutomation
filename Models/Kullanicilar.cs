using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("Kullanicilar")]
[Index("RolId", Name = "IX_Kullanicilar_RolId")]
[Index("Email", Name = "UQ__Kullanic__A9D105342D288F18", IsUnique = true)]
public partial class Kullanicilar
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Ad { get; set; } = null!;

    [StringLength(50)]
    public string Soyad { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(20)]
    public string? Telefon { get; set; }

    [StringLength(255)]
    public string SifreHash { get; set; } = null!;

    public int RolId { get; set; }

    public bool AktifMi { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime KayitTarihi { get; set; }

    [InverseProperty("Kullanici")]
    public virtual ICollection<Rezervasyonlar> Rezervasyonlars { get; set; } = new List<Rezervasyonlar>();

    [ForeignKey("RolId")]
    [InverseProperty("Kullanicilars")]
    public virtual Roller Rol { get; set; } = null!;
}
