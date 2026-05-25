using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OtobusFinalProje.Models;

[Table("Roller")]
[Index("RolAdi", Name = "UQ__Roller__85F2635DE2FB1B9D", IsUnique = true)]
public partial class Roller
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string RolAdi { get; set; } = null!;

    [InverseProperty("Rol")]
    public virtual ICollection<Kullanicilar> Kullanicilars { get; set; } = new List<Kullanicilar>();
}
