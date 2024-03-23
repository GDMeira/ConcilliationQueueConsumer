using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Consumer.Models;

[Table("User")]
[Index("Cpf", Name = "AK_User_Cpf", IsUnique = true)]
public partial class User
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Cpf { get; set; } = null!;

    public string Name { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<PaymentProviderAccount> PaymentProviderAccounts { get; set; } = new List<PaymentProviderAccount>();
}
