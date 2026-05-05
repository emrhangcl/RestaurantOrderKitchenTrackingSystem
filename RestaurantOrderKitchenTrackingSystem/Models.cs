using System;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantOrderKitchenTrackingSystem
{
    [Serializable]
    public enum OrderStatus
    {
        New,
        Preparing,
        Ready,
        Served,
        Paid,
        Cancelled
    }

    [Serializable]
    public enum PaymentMethod
    {
        None,
        Cash,
        Card,
        Split
    }

    [Serializable]
    public enum UserRole
    {
        Waiter,
        Kitchen,
        Cashier,
        Manager
    }

    [Serializable]
    public enum TableState
    {
        Available,
        Reserved,
        Cleaning
    }

    [Serializable]
    public sealed class MenuItem
    {
        public MenuItem(int id, string name, string category, decimal price, int prepMinutes, IEnumerable<string> ingredients, int stockQuantity)
        {
            Id = id;
            Name = name;
            Category = category;
            Price = price;
            PrepMinutes = prepMinutes;
            Ingredients = ingredients.ToList();
            StockQuantity = stockQuantity;
            IsActive = true;
        }

        public int Id { get; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int PrepMinutes { get; set; }
        public List<string> Ingredients { get; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }

        public override string ToString()
        {
            var status = IsActive ? "Stock: " + StockQuantity : "Unavailable";
            return Name + " - " + Price.ToString("C") + " (" + status + ")";
        }
    }

    [Serializable]
    public sealed class OrderLine
    {
        public OrderLine(MenuItem item, int quantity, string customization)
        {
            Item = item;
            Quantity = quantity;
            Customization = customization;
        }

        public MenuItem Item { get; }
        public int Quantity { get; set; }
        public string Customization { get; set; }
        public decimal Total => Item.Price * Quantity;
        public int PrepMinutes => Item.PrepMinutes * Quantity;

        public string DisplayName
        {
            get
            {
                return string.IsNullOrWhiteSpace(Customization)
                    ? Item.Name
                    : Item.Name + " (" + Customization + ")";
            }
        }
    }

    [Serializable]
    public sealed class PaymentRecord
    {
        public PaymentRecord(PaymentMethod method, decimal amount)
        {
            Method = method;
            Amount = amount;
            PaidAt = DateTime.Now;
        }

        public PaymentMethod Method { get; }
        public decimal Amount { get; }
        public DateTime PaidAt { get; }
    }

    [Serializable]
    public sealed class RestaurantOrder
    {
        public RestaurantOrder(int id, int tableNumber, string serverName, string notes, IEnumerable<OrderLine> lines)
        {
            Id = id;
            TableNumber = tableNumber;
            ServerName = serverName;
            Notes = notes;
            Lines = lines.ToList();
            Payments = new List<PaymentRecord>();
            CreatedAt = DateTime.Now;
            LastActivityAt = CreatedAt;
            Status = OrderStatus.New;
            PaymentMethod = PaymentMethod.None;
        }

        public int Id { get; }
        public int TableNumber { get; set; }
        public string ServerName { get; }
        public string Notes { get; }
        public DateTime CreatedAt { get; }
        public DateTime LastActivityAt { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public List<OrderLine> Lines { get; }
        public List<PaymentRecord> Payments { get; }
        public decimal Total => Lines.Sum(line => line.Total);
        public decimal PaidAmount => Payments.Sum(payment => payment.Amount);
        public decimal Balance => Total - PaidAmount;
        public int EstimatedPrepMinutes => Lines.Sum(line => line.PrepMinutes);
        public string ItemsSummary => string.Join(", ", Lines.Select(line => line.Quantity + "x " + line.DisplayName));

        public string ReceiptText
        {
            get
            {
                var receiptLines = Lines.Select(line => line.Quantity + " x " + line.DisplayName + " = " + line.Total.ToString("C"));
                var paymentLines = Payments.Count == 0
                    ? "-"
                    : string.Join(Environment.NewLine, Payments.Select(payment => payment.Method + ": " + payment.Amount.ToString("C")));

                return "Order #" + Id + Environment.NewLine
                    + "Table: " + TableNumber + Environment.NewLine
                    + "Server: " + ServerName + Environment.NewLine
                    + "Status: " + Status + Environment.NewLine
                    + "Payment: " + PaymentMethod + Environment.NewLine
                    + "Created: " + CreatedAt.ToString("g") + Environment.NewLine
                    + "Elapsed: " + Math.Floor((DateTime.Now - CreatedAt).TotalMinutes) + " min" + Environment.NewLine
                    + "Notes: " + (string.IsNullOrWhiteSpace(Notes) ? "-" : Notes) + Environment.NewLine
                    + Environment.NewLine
                    + string.Join(Environment.NewLine, receiptLines) + Environment.NewLine
                    + Environment.NewLine
                    + "Payments:" + Environment.NewLine
                    + paymentLines + Environment.NewLine
                    + Environment.NewLine
                    + "Total: " + Total.ToString("C") + Environment.NewLine
                    + "Balance: " + Math.Max(0, Balance).ToString("C");
            }
        }
    }

    [Serializable]
    public sealed class WaiterAccount
    {
        public WaiterAccount(string username, string password, string displayName, UserRole role)
        {
            Username = username;
            Password = password;
            DisplayName = displayName;
            Role = role;
        }

        public string Username { get; }
        public string Password { get; }
        public string DisplayName { get; }
        public UserRole Role { get; }
    }

    [Serializable]
    public sealed class RestaurantTable
    {
        public RestaurantTable(int number)
        {
            Number = number;
            State = TableState.Available;
        }

        public int Number { get; }
        public DateTime? LastOrderAt { get; set; }
        public bool HasActiveOrder { get; set; }
        public TableState State { get; set; }

        public int MinutesSinceLastOrder
        {
            get
            {
                if (LastOrderAt == null)
                {
                    return 0;
                }

                return (int)Math.Floor((DateTime.Now - LastOrderAt.Value).TotalMinutes);
            }
        }
    }

    [Serializable]
    public sealed class AppState
    {
        public List<MenuItem> MenuItems { get; set; }
        public List<RestaurantOrder> Orders { get; set; }
        public List<RestaurantTable> Tables { get; set; }
        public int NextOrderId { get; set; }
    }
}
