using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Models;

[Table("PaymentProvider")]
[Index("Token", Name = "IX_PaymentProviderAccount_Token", IsUnique = true)]
public partial class PaymentProvider
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Token { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string PatchPaymentUrl { get; set; } = null!;

    public string PostPaymentUrl { get; set; } = null!;

    [InverseProperty("PaymentProvider")]
    public virtual ICollection<Concilliation> Concilliations { get; set; } = new List<Concilliation>();

    [InverseProperty("PaymentProvider")]
    public virtual ICollection<PaymentProviderAccount> PaymentProviderAccounts { get; set; } = new List<PaymentProviderAccount>();
}
