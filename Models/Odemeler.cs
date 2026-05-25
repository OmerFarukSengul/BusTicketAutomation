using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("Odemeler")]
[Index("RezervasyonId", Name = "IX_Odemeler_RezervasyonId")]
public partial class Odemeler
{
    [Key]
    public int Id { get; set; }

    public int RezervasyonId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime OdemeTarihi { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Tutar { get; set; }

    [StringLength(30)]
    public string OdemeTipi { get; set; } = null!;

    [StringLength(30)]
    public string OdemeDurumu { get; set; } = null!;

    [StringLength(100)]
    public string? IslemNo { get; set; }

    [ForeignKey("RezervasyonId")]
    [InverseProperty("Odemelers")]
    public virtual Rezervasyonlar Rezervasyon { get; set; } = null!;
}
