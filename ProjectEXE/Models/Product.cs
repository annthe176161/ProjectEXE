using System;
using System.Collections.Generic;

namespace ProjectEXE.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public int ShopId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public int ConditionId { get; set; }

    public string? Gender { get; set; }

    public string? Size { get; set; }

    public string? Color { get; set; }

    public string? Brand { get; set; }

    public string? Material { get; set; }

    public bool IsInStock { get; set; }

    public bool IsVisible { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ProductCondition Condition { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual Shop Shop { get; set; } = null!;
}
