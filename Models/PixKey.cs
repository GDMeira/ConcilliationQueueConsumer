using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Models;

[Table("PixKey")]
[Index("Value", Name = "AK_PixKey_Value", IsUnique = true)]
[Index("PaymentProviderAccountId", Name = "IX_PixKey_PaymentProviderAccountId")]
public partial class PixKey
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Type { get; set; } = null!;

    public string Value { get; set; } = null!;

    public int PaymentProviderAccountId { get; set; }

    [ForeignKey("PaymentProviderAccountId")]
    [InverseProperty("PixKeys")]
    public virtual PaymentProviderAccount PaymentProviderAccount { get; set; } = null!;

    [InverseProperty("PixKey")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
