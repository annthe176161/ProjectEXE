using System;
using System.Collections.Generic;

namespace ProjectEXE.Models;

public partial class PaymentStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<PackagePayment> PackagePayments { get; set; } = new List<PackagePayment>();
}
