using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace RestaurantOrderKitchenTrackingSystem
{
    public static class DataStore
    {
        private static readonly string FolderPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RestaurantOrderKitchenTrackingSystem");

        private static readonly string StatePath = Path.Combine(FolderPath, "restaurant-system.dat");

        public static string ReceiptFolder => Path.Combine(FolderPath, "Receipts");

        public static AppState Load()
        {
            if (!File.Exists(StatePath))
            {
                return null;
            }

            try
            {
                using (var stream = File.OpenRead(StatePath))
                {
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(stream) as AppState;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void Save(AppState state)
        {
            Directory.CreateDirectory(FolderPath);
            using (var stream = File.Create(StatePath))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, state);
            }
        }

        public static string SaveReceipt(RestaurantOrder order)
        {
            Directory.CreateDirectory(ReceiptFolder);
            var path = Path.Combine(ReceiptFolder, "receipt-" + order.Id + ".txt");
            File.WriteAllText(path, order.ReceiptText);
            return path;
        }
    }
}
