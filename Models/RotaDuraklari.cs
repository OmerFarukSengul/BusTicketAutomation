using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("RotaDuraklari")]
public partial class RotaDuraklari
{
    [Key]
    public int Id { get; set; }

    public int RotaId { get; set; }

    public int SehirId { get; set; }

    public int SiraNo { get; set; }

    public bool AktifMi { get; set; }

    [ForeignKey("RotaId")]
    [InverseProperty("RotaDuraklaris")]
    public virtual Rotalar Rota { get; set; } = null!;

    [ForeignKey("SehirId")]
    [InverseProperty("RotaDuraklaris")]
    public virtual Sehirler Sehir { get; set; } = null!;
}
