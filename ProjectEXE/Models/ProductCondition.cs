using System;
using System.Collections.Generic;

namespace ProjectEXE.Models;

public partial class ProductCondition
{
    public int ConditionId { get; set; }

    public string ConditionName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
