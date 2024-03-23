using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Models;

[Table("Payment")]
[Index("PaymentProviderAccountId", Name = "IX_Payment_PaymentProviderAccountId")]
[Index("PixKeyId", Name = "IX_Payment_PixKeyId")]
public partial class Payment
{
    [Key]
    public int Id { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? Amount { get; set; }

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public int? PaymentProviderAccountId { get; set; }

    public int? PixKeyId { get; set; }

    [ForeignKey("PaymentProviderAccountId")]
    [InverseProperty("Payments")]
    public virtual PaymentProviderAccount PaymentProviderAccount { get; set; } = null!;

    [ForeignKey("PixKeyId")]
    [InverseProperty("Payments")]
    public virtual PixKey PixKey { get; set; } = null!;
}
