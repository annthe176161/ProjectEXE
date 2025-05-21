using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProjectEXE.Models;

public partial class RevaContext : DbContext
{
    public RevaContext()
    {
    }

    public RevaContext(DbContextOptions<RevaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<PackagePayment> PackagePayments { get; set; }

    public virtual DbSet<PackageSubscription> PackageSubscriptions { get; set; }

    public virtual DbSet<PaymentStatus> PaymentStatuses { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCondition> ProductConditions { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ServicePackage> ServicePackages { get; set; }

    public virtual DbSet<Shop> Shops { get; set; }

    public virtual DbSet<SubscriptionStatus> SubscriptionStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetConnectionString("MyCnn");
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B4D5ECC7C");

            entity.Property(e => e.CategoryName).HasMaxLength(100);

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_Categories_ParentCategory");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BCF546500EE");

            entity.HasIndex(e => e.BuyerId, "IX_Orders_BuyerId");

            entity.HasIndex(e => e.SellerId, "IX_Orders_SellerId");

            entity.HasIndex(e => e.StatusId, "IX_Orders_StatusId");

            entity.Property(e => e.CancelReason).HasMaxLength(255);
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Buyer).WithMany(p => p.OrderBuyers)
                .HasForeignKey(d => d.BuyerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Buyers");

            entity.HasOne(d => d.Product).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Products");

            entity.HasOne(d => d.Seller).WithMany(p => p.OrderSellers)
                .HasForeignKey(d => d.SellerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Sellers");

            entity.HasOne(d => d.Status).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Orders_Statuses");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__OrderSta__C8EE206343091DA2");

            entity.HasIndex(e => e.StatusName, "UQ__OrderSta__05E7698A526003C2").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<PackagePayment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__PackageP__9B556A3846272873");

            entity.HasIndex(e => e.TransactionCode, "UQ__PackageP__D85E70261DEF2C25").IsUnique();

            entity.Property(e => e.Amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionCode).HasMaxLength(50);

            entity.HasOne(d => d.ConfirmedByUser).WithMany(p => p.PackagePaymentConfirmedByUsers)
                .HasForeignKey(d => d.ConfirmedByUserId)
                .HasConstraintName("FK_PackagePayments_ConfirmedBy");

            entity.HasOne(d => d.Package).WithMany(p => p.PackagePayments)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackagePayments_ServicePackages");

            entity.HasOne(d => d.Status).WithMany(p => p.PackagePayments)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackagePayments_Statuses");

            entity.HasOne(d => d.Subscription).WithMany(p => p.PackagePayments)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK_PackagePayments_Subscriptions");

            entity.HasOne(d => d.User).WithMany(p => p.PackagePaymentUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackagePayments_Users");
        });

        modelBuilder.Entity<PackageSubscription>(entity =>
        {
            entity.HasKey(e => e.SubscriptionId).HasName("PK__PackageS__9A2B249D57F51D45");

            entity.HasIndex(e => e.ShopId, "IX_PackageSubscriptions_ShopId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.Package).WithMany(p => p.PackageSubscriptions)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackageSubscriptions_ServicePackages");

            entity.HasOne(d => d.Shop).WithMany(p => p.PackageSubscriptions)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackageSubscriptions_Shops");

            entity.HasOne(d => d.Status).WithMany(p => p.PackageSubscriptions)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PackageSubscriptions_Statuses");
        });

        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__PaymentS__C8EE20630ED36391");

            entity.HasIndex(e => e.StatusName, "UQ__PaymentS__05E7698A1EF5901D").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CDA2653423");

            entity.HasIndex(e => e.CategoryId, "IX_Products_CategoryId");

            entity.HasIndex(e => e.IsVisible, "IX_Products_IsVisible");

            entity.HasIndex(e => e.ShopId, "IX_Products_ShopId");

            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.IsInStock).HasDefaultValue(true);
            entity.Property(e => e.IsVisible).HasDefaultValue(true);
            entity.Property(e => e.Material).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.Size).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");

            entity.HasOne(d => d.Condition).WithMany(p => p.Products)
                .HasForeignKey(d => d.ConditionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Conditions");

            entity.HasOne(d => d.Shop).WithMany(p => p.Products)
                .HasForeignKey(d => d.ShopId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Shops");
        });

        modelBuilder.Entity<ProductCondition>(entity =>
        {
            entity.HasKey(e => e.ConditionId).HasName("PK__ProductC__37F5C0CFD3849081");

            entity.HasIndex(e => e.ConditionName, "UQ__ProductC__CE7E60662426F0D6").IsUnique();

            entity.Property(e => e.ConditionName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__ProductI__7516F70C3170E4E4");

            entity.Property(e => e.ImageUrl).HasMaxLength(255);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductImages_Products");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1AD3AEDE34");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B616064EF443C").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<ServicePackage>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PK__ServiceP__322035CC195D46DC");

            entity.Property(e => e.DiscountedPrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.PackageName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
        });

        modelBuilder.Entity<Shop>(entity =>
        {
            entity.HasKey(e => e.ShopId).HasName("PK__Shops__67C557C9DCF86EA8");

            entity.HasIndex(e => e.ShopName, "UQ__Shops__649A7D9693342DE6").IsUnique();

            entity.Property(e => e.ContactInfo).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ProfileImage).HasMaxLength(255);
            entity.Property(e => e.ShopName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Shops)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Shops_Users");
        });

        modelBuilder.Entity<SubscriptionStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__Subscrip__C8EE2063BB77D4B3");

            entity.HasIndex(e => e.StatusName, "UQ__Subscrip__05E7698A2D3225DB").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C3DA04F6C");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105349D9262BE").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
