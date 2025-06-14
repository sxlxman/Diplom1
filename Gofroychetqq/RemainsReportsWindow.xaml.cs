using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using OfficeOpenXml;
using System.IO;
using System.Data.Entity;

namespace Gofroychetqq
{
    public partial class RemainsReportsWindow : Window
    {
        private List<RemainsReportItem> _allRemains;

        public RemainsReportsWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new etonEntities())
                {
                    // Загружаем все необходимые данные в память
                    var materials = db.Material.Include(m => m.MaterialType.Unit).ToList();
                    var prihods = db.Prihod.ToList();
                    var rashods = db.Rashod.ToList();

                    _allRemains = materials.Select(m => 
                    {
                        decimal income = (decimal)(prihods.Where(p => p.MaterialID == m.MaterialID).Sum(p => (double?)p.Quantity) ?? 0);
                        decimal outcome = (decimal)(rashods.Where(r => r.MaterialID == m.MaterialID).Sum(r => (double?)r.Quantity) ?? 0);

                        return new RemainsReportItem
                        {
                            MaterialName = m.Name,
                            MaterialTypeName = m.MaterialType.Name,
                            CurrentStock = income - outcome,
                            UnitName = m.MaterialType.Unit.Name,
                            MinimumStock = (decimal)m.MinimumStock
                        };
                    }).ToList();

                    ReportsDataGrid.ItemsSource = _allRemains;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных об остатках: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel файлы (*.xlsx)|*.xlsx",
                    Title = "Сохранить отчёт об остатках",
                    FileName = $"Отчёт об остатках сырья {DateTime.Now:dd.MM.yyyy}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Отчёт об остатках");

                        // Заголовки
                        worksheet.Cells[1, 1].Value = "Сырьё";
                        worksheet.Cells[1, 2].Value = "Тип сырья";
                        worksheet.Cells[1, 3].Value = "Текущий остаток";
                        worksheet.Cells[1, 4].Value = "Ед. изм.";
                        worksheet.Cells[1, 5].Value = "Минимальный остаток";

                        // Данные
                        int row = 2;
                        foreach (var item in _allRemains)
                        {
                            worksheet.Cells[row, 1].Value = item.MaterialName;
                            worksheet.Cells[row, 2].Value = item.MaterialTypeName;
                            worksheet.Cells[row, 3].Value = item.CurrentStock;
                            worksheet.Cells[row, 4].Value = item.UnitName;
                            worksheet.Cells[row, 5].Value = item.MinimumStock;
                            row++;
                        }

                        // Форматирование
                        worksheet.Cells[1, 1, 1, 5].Style.Font.Bold = true;
                        worksheet.Cells[1, 1, row - 1, 5].AutoFitColumns();

                        // Сохранение файла
                        package.SaveAs(new FileInfo(saveFileDialog.FileName));
                    }

                    MessageBox.Show("Отчёт успешно экспортирован в Excel", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class RemainsReportItem
    {
        public string MaterialName { get; set; }
        public string MaterialTypeName { get; set; }
        public decimal CurrentStock { get; set; }
        public string UnitName { get; set; }
        public decimal MinimumStock { get; set; }
    }
} 