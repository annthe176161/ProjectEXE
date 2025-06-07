using System;
using System.Collections.Generic;

namespace ProjectEXE.Models;

public partial class Voucher
{
    public string VourcherId { get; set; } = null!;

    public string Code { get; set; } = null!;

    public int Discount { get; set; }

    public DateOnly CreateAt { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public decimal? MinOrderValue { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public int Quantity { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
