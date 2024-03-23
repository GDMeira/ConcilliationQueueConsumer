using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Models;

[Table("Concilliation")]
[Index("PaymentProviderId", Name = "IX_Concilliation_PaymentProviderId")]
public partial class Concilliation
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string FileUrl { get; set; } = null!;

    public int PaymentProviderId { get; set; }

    public string Status { get; set; } = null!;

    public DateOnly Date { get; set; }

    public string Postback { get; set; } = null!;

    [ForeignKey("PaymentProviderId")]
    [InverseProperty("Concilliations")]
    public virtual PaymentProvider PaymentProvider { get; set; } = null!;
}
