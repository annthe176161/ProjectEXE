using System;
using System.Collections.Generic;

namespace ProjectEXE.Models;

public partial class PackageSubscription
{
    public int SubscriptionId { get; set; }

    public int ShopId { get; set; }

    public int PackageId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int StatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ServicePackage Package { get; set; } = null!;

    public virtual ICollection<PackagePayment> PackagePayments { get; set; } = new List<PackagePayment>();

    public virtual Shop Shop { get; set; } = null!;

    public virtual SubscriptionStatus Status { get; set; } = null!;
}
