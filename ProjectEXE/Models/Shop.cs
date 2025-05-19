using System;
using System.Collections.Generic;

namespace ProjectEXE.Models;

public partial class Shop
{
    public int ShopId { get; set; }

    public int UserId { get; set; }

    public string ShopName { get; set; } = null!;

    public string? Description { get; set; }

    public string? ProfileImage { get; set; }

    public string? ContactInfo { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<PackageSubscription> PackageSubscriptions { get; set; } = new List<PackageSubscription>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual User User { get; set; } = null!;
}
