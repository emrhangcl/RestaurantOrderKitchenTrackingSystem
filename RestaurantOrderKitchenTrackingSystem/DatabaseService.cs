using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace RestaurantOrderKitchenTrackingSystem
{
    public static class DatabaseService
    {
        private const string DatabaseName = "RestaurantOrderKitchenTrackingSystemDb";
        private const string MasterConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True";
        private const string AppConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=RestaurantOrderKitchenTrackingSystemDb;Integrated Security=True";

        public static void Initialize()
        {
            using (var connection = new SqlConnection(MasterConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("IF DB_ID(@name) IS NULL CREATE DATABASE [" + DatabaseName + "]", connection))
                {
                    command.Parameters.AddWithValue("@name", DatabaseName);
                    command.ExecuteNonQuery();
                }
            }

            using (var connection = new SqlConnection(AppConnectionString))
            {
                connection.Open();
                var sql = @"
IF OBJECT_ID('dbo.MenuItems', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.MenuItems
    (
        Id INT NOT NULL PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Category NVARCHAR(60) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        PrepMinutes INT NOT NULL,
        Ingredients NVARCHAR(MAX) NOT NULL,
        StockQuantity INT NOT NULL,
        IsActive BIT NOT NULL
    )
END";
                using (var command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public static List<MenuItem> GetMenuItems()
        {
            var items = new List<MenuItem>();
            using (var connection = new SqlConnection(AppConnectionString))
            using (var command = new SqlCommand("SELECT Id, Name, Category, Price, PrepMinutes, Ingredients, StockQuantity, IsActive FROM dbo.MenuItems ORDER BY Id", connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new MenuItem(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetDecimal(3),
                            reader.GetInt32(4),
                            reader.GetString(5).Split(',').Select(part => part.Trim()),
                            reader.GetInt32(6));
                        item.IsActive = reader.GetBoolean(7);
                        items.Add(item);
                    }
                }
            }

            return items;
        }

        public static void InsertMenuItem(MenuItem item)
        {
            const string sql = @"INSERT INTO dbo.MenuItems (Id, Name, Category, Price, PrepMinutes, Ingredients, StockQuantity, IsActive)
VALUES (@Id, @Name, @Category, @Price, @PrepMinutes, @Ingredients, @StockQuantity, @IsActive)";
            ExecuteMenuItemCommand(sql, item);
        }

        public static void UpdateMenuItem(MenuItem item)
        {
            const string sql = @"UPDATE dbo.MenuItems
SET Name = @Name,
    Category = @Category,
    Price = @Price,
    PrepMinutes = @PrepMinutes,
    Ingredients = @Ingredients,
    StockQuantity = @StockQuantity,
    IsActive = @IsActive
WHERE Id = @Id";
            ExecuteMenuItemCommand(sql, item);
        }

        public static void DeleteMenuItem(int id)
        {
            using (var connection = new SqlConnection(AppConnectionString))
            using (var command = new SqlCommand("DELETE FROM dbo.MenuItems WHERE Id = @Id", connection))
            {
                command.Parameters.AddWithValue("@Id", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static void ExecuteMenuItemCommand(string sql, MenuItem item)
        {
            using (var connection = new SqlConnection(AppConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@Id", item.Id);
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@Category", item.Category);
                command.Parameters.AddWithValue("@Price", item.Price);
                command.Parameters.AddWithValue("@PrepMinutes", item.PrepMinutes);
                command.Parameters.AddWithValue("@Ingredients", string.Join(", ", item.Ingredients));
                command.Parameters.AddWithValue("@StockQuantity", item.StockQuantity);
                command.Parameters.AddWithValue("@IsActive", item.IsActive);
                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
