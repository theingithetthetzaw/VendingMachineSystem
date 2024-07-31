using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;
using System.Transactions;

public class VendingMachineContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    public VendingMachineContext(DbContextOptions<VendingMachineContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().HasData(
            new Product { ID = 1, Name = "Coke", Price = 3.99m, QuantityAvailable = 100 },
            new Product { ID = 2, Name = "Pepsi", Price = 6.885m, QuantityAvailable = 100 },
            new Product { ID = 3, Name = "Water", Price = 0.5m, QuantityAvailable = 100 }
        );
    }

    public class Product
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Quantity Available is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity available cannot be negative.")]
        public int QuantityAvailable { get; set; }

    }

    public class User
    {
        public string ID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }

    public class Transaction
    {
        public int ID { get; set; }
        public int ProductID { get; set; }
        public string UserID { get; set; }
        public int QuantityPurchased { get; set; }
        public DateTime PurchaseDate { get; set; }

        public Product Product { get; set; }
        public User User { get; set; }
    }


}


