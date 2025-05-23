﻿using System;
using System.Collections.Generic;

namespace ProjectEXE.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int BuyerId { get; set; }

    public int SellerId { get; set; }

    public int ProductId { get; set; }

    public int StatusId { get; set; }

    public string? CancelReason { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual User Buyer { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual User Seller { get; set; } = null!;

    public virtual OrderStatus Status { get; set; } = null!;
}
