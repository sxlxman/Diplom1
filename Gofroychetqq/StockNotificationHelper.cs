using System.Linq;
using System.Windows;
using System.Text;
using System.Data.Entity;

namespace Gofroychetqq
{
    public static class StockNotificationHelper
    {
        public static void CheckAndShowLowStockNotification()
        {
            try
            {
                using (var db = new etonEntities())
                {
                    // Получаем все материалы с минимальным остатком
                    var materials = db.Material.ToList();

                    var lowStockMaterials = new StringBuilder();

                    foreach (var material in materials)
                    {
                        // Рассчитываем текущий остаток
                        var prihodQuantity = db.Prihod
                            .Where(p => p.MaterialID == material.MaterialID)
                            .Sum(p => (double?)p.Quantity) ?? 0;

                        var rashodQuantity = db.Rashod
                            .Where(r => r.MaterialID == material.MaterialID)
                            .Sum(r => (double?)r.Quantity) ?? 0;

                        var currentStock = prihodQuantity - rashodQuantity;

                        // Проверяем, если текущий остаток меньше минимального
                        if (currentStock < material.MinimumStock)
                        {
                            lowStockMaterials.AppendLine($"- {material.Name} (Текущий остаток: {currentStock}, Мин. остаток: {material.MinimumStock})");
                        }
                    }

                    // Если есть материалы с низким остатком, показываем уведомление
                    if (lowStockMaterials.Length > 0)
                    {
                        var message = "Следующие материалы имеют остаток ниже минимального:\n" + lowStockMaterials.ToString();
                        MessageBox.Show(message, "Низкий остаток сырья", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Логируем или обрабатываем ошибку, если не удалось проверить остатки
                System.Diagnostics.Debug.WriteLine($"Ошибка при проверке остатков сырья: {ex.Message}");
            }
        }
    }
} 