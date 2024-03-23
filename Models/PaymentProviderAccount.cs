using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Models;

[Table("PaymentProviderAccount")]
[Index("PaymentProviderId", Name = "IX_PaymentProviderAccount_PaymentProviderId")]
[Index("UserId", Name = "IX_PaymentProviderAccount_UserId")]
public partial class PaymentProviderAccount
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Number { get; set; } = null!;

    public string Agency { get; set; } = null!;

    public int UserId { get; set; }

    public int PaymentProviderId { get; set; }

    [ForeignKey("PaymentProviderId")]
    [InverseProperty("PaymentProviderAccounts")]
    public virtual PaymentProvider PaymentProvider { get; set; } = null!;

    [InverseProperty("PaymentProviderAccount")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [InverseProperty("PaymentProviderAccount")]
    public virtual ICollection<PixKey> PixKeys { get; set; } = new List<PixKey>();

    [ForeignKey("UserId")]
    [InverseProperty("PaymentProviderAccounts")]
    public virtual User User { get; set; } = null!;
}
