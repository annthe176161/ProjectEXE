using System;
using System.Collections.Generic;

namespace ProjectEXE.Models;

public partial class SubscriptionStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<PackageSubscription> PackageSubscriptions { get; set; } = new List<PackageSubscription>();
}
