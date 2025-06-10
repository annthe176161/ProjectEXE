using System;
using System.Collections.Generic;

namespace ProjectEXE.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public int RoleId { get; set; }

    public int IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ReferralCode { get; set; } = null!;

    public string? ReferredBy { get; set; }

    public virtual ICollection<Order> OrderBuyers { get; set; } = new List<Order>();

    public virtual ICollection<Order> OrderSellers { get; set; } = new List<Order>();

    public virtual ICollection<PackagePayment> PackagePayments { get; set; } = new List<PackagePayment>();

    public virtual Role Role { get; set; } = null!;

    public virtual ICollection<Shop> Shops { get; set; } = new List<Shop>();
}
