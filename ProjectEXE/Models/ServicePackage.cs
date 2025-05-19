using System;
using System.Collections.Generic;

namespace ProjectEXE.Models;

public partial class ServicePackage
{
    public int PackageId { get; set; }

    public string PackageName { get; set; } = null!;

    public int ProductLimit { get; set; }

    public decimal Price { get; set; }

    public decimal? DiscountedPrice { get; set; }

    public virtual ICollection<PackagePayment> PackagePayments { get; set; } = new List<PackagePayment>();

    public virtual ICollection<PackageSubscription> PackageSubscriptions { get; set; } = new List<PackageSubscription>();
}
