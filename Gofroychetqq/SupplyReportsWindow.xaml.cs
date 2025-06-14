using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OfficeOpenXml;
using System.IO;
using System.Data.Entity;

namespace Gofroychetqq
{
    public partial class SupplyReportsWindow : Window
    {
        private List<SupplyReportItem> _allReports;
        private List<SupplyReportItem> _filteredReports;

        public SupplyReportsWindow()
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
                    // Загрузка поставщиков
                    var suppliers = db.Supplier.ToList();
                    SupplierComboBox.ItemsSource = suppliers;
                    SupplierComboBox.SelectedIndex = -1;

                    // Загрузка сырья
                    var materials = db.Material.ToList();
                    MaterialComboBox.ItemsSource = materials;
                    MaterialComboBox.SelectedIndex = -1;

                    // Загрузка данных о поставках
                    _allReports = (from s in db.Supply
                                 join sd in db.Prihod on s.SupplyID equals sd.SupplyID
                                 join sup in db.Supplier on s.SupplierID equals sup.SupplierID
                                 join m in db.Material on sd.MaterialID equals m.MaterialID
                                 select new 
                                 {
                                     SupplyDate = s.SupplyDate,
                                     SupplierName = sup.Name,
                                     MaterialName = m.Name,
                                     Quantity = sd.Quantity,
                                     InvoiceNumber = s.DocumentNumber
                                 }).ToList()
                                 .Select(x => new SupplyReportItem
                                 {
                                     SupplyDate = x.SupplyDate,
                                     SupplierName = x.SupplierName,
                                     MaterialName = x.MaterialName,
                                     Quantity = (decimal)x.Quantity,
                                     InvoiceNumber = x.InvoiceNumber
                                 }).ToList();

                    _filteredReports = _allReports;
                    ReportsDataGrid.ItemsSource = _filteredReports;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var filtered = _allReports.AsQueryable();

                // Фильтр по поставщику
                if (SupplierComboBox.SelectedItem != null)
                {
                    var supplier = (Supplier)SupplierComboBox.SelectedItem;
                    filtered = filtered.Where(r => r.SupplierName == supplier.Name);
                }

                // Фильтр по сырью
                if (MaterialComboBox.SelectedItem != null)
                {
                    var material = (Material)MaterialComboBox.SelectedItem;
                    filtered = filtered.Where(r => r.MaterialName == material.Name);
                }

                // Фильтр по датам
                if (StartDatePicker.SelectedDate.HasValue)
                {
                    filtered = filtered.Where(r => r.SupplyDate >= StartDatePicker.SelectedDate.Value);
                }

                if (EndDatePicker.SelectedDate.HasValue)
                {
                    filtered = filtered.Where(r => r.SupplyDate <= EndDatePicker.SelectedDate.Value);
                }

                _filteredReports = filtered.ToList();
                ReportsDataGrid.ItemsSource = _filteredReports;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при применении фильтров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SupplierComboBox.SelectedIndex = -1;
            MaterialComboBox.SelectedIndex = -1;
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;
            _filteredReports = _allReports;
            ReportsDataGrid.ItemsSource = _filteredReports;
        }

        private void ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel файлы (*.xlsx)|*.xlsx",
                    Title = "Сохранить отчёт",
                    FileName = $"Отчёт о поставках {DateTime.Now:dd.MM.yyyy}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Отчёт о поставках");

                        // Заголовки
                        worksheet.Cells[1, 1].Value = "Дата поставки";
                        worksheet.Cells[1, 2].Value = "Поставщик";
                        worksheet.Cells[1, 3].Value = "Сырьё";
                        worksheet.Cells[1, 4].Value = "Количество";
                        worksheet.Cells[1, 5].Value = "Номер накладной";

                        // Данные
                        int row = 2;
                        foreach (var report in _filteredReports)
                        {
                            worksheet.Cells[row, 1].Value = report.SupplyDate;
                            worksheet.Cells[row, 1].Style.Numberformat.Format = "dd.MM.yyyy";
                            worksheet.Cells[row, 2].Value = report.SupplierName;
                            worksheet.Cells[row, 3].Value = report.MaterialName;
                            worksheet.Cells[row, 4].Value = report.Quantity;
                            worksheet.Cells[row, 5].Value = report.InvoiceNumber;
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

    public class SupplyReportItem
    {
        public DateTime SupplyDate { get; set; }
        public string SupplierName { get; set; }
        public string MaterialName { get; set; }
        public decimal Quantity { get; set; }
        public string InvoiceNumber { get; set; }
    }
} 