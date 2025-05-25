using System;
using System.Collections.Generic;

namespace ProjectEXE.Models;

public partial class PackagePayment
{
    public int PaymentId { get; set; }

    public int? SubscriptionId { get; set; }

    public int UserId { get; set; }

    public int PackageId { get; set; }

    public string TransactionCode { get; set; } = null!;

    public decimal Amount { get; set; }

    public int StatusId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ServicePackage Package { get; set; } = null!;

    public virtual PaymentStatus Status { get; set; } = null!;

    public virtual PackageSubscription? Subscription { get; set; }

    public virtual User User { get; set; } = null!;
}
