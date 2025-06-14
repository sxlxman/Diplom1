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
    public partial class MaterialMovementReportsWindow : Window
    {
        private List<MaterialMovementReportItem> _allReports;
        private List<MaterialMovementReportItem> _filteredReports;

        public MaterialMovementReportsWindow()
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
                    // Загрузка сырья
                    var materials = db.Material.ToList();
                    MaterialComboBox.ItemsSource = materials;
                    MaterialComboBox.SelectedIndex = -1;

                    // Загрузка сырых данных о приходах
                    var prihodData = db.Prihod.Include(p => p.Material)
                                              .Include(p => p.User)
                                              .Include(p => p.Nakladnaya)
                                              .ToList();

                    // Загрузка сырых данных о расходах
                    var rashodData = db.Rashod.Include(r => r.Material)
                                              .Include(r => r.User)
                                              .Include(r => r.Nakladnaya)
                                              .Include(r => r.Reason)
                                              .ToList();

                    // Проекция в MaterialMovementReportItem в памяти
                    var incomeReports = prihodData.Select(p => new MaterialMovementReportItem
                    {
                        MovementDate = p.PrihodDate,
                        MaterialName = p.Material.Name,
                        MovementType = "Приход",
                        Quantity = (decimal)p.Quantity,
                        UserName = p.User.Surname + " " + p.User.Name,
                        InvoiceNumber = p.Nakladnaya.Number.ToString(),
                        Note = "",
                        UnitName = p.Material.MaterialType.Unit.Name
                    }).ToList();

                    var outcomeReports = rashodData.Select(r => new MaterialMovementReportItem
                    {
                        MovementDate = r.RashodDate,
                        MaterialName = r.Material.Name,
                        MovementType = "Расход",
                        Quantity = (decimal)r.Quantity,
                        UserName = r.User.Surname + " " + r.User.Name,
                        InvoiceNumber = r.Nakladnaya.Number.ToString(),
                        Note = r.Reason.Name,
                        UnitName = r.Material.MaterialType.Unit.Name
                    }).ToList();

                    _allReports = incomeReports.Concat(outcomeReports).OrderBy(r => r.MovementDate).ToList();
                    _filteredReports = _allReports;
                    ReportsDataGrid.ItemsSource = _filteredReports;

                    // Установка "Все" в ComboBox для типа движения
                    MovementTypeComboBox.SelectedIndex = 0; // Первый элемент "Все"
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

                // Фильтр по сырью
                if (MaterialComboBox.SelectedItem != null)
                {
                    var material = (Material)MaterialComboBox.SelectedItem;
                    filtered = filtered.Where(r => r.MaterialName == material.Name);
                }

                // Фильтр по типу движения
                if (MovementTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    var tag = selectedItem.Tag?.ToString();
                    if (tag == "Income")
                    {
                        filtered = filtered.Where(r => r.MovementType == "Приход");
                    }
                    else if (tag == "Outcome")
                    {
                        filtered = filtered.Where(r => r.MovementType == "Расход");
                    }
                }

                // Фильтр по датам
                if (StartDatePicker.SelectedDate.HasValue)
                {
                    filtered = filtered.Where(r => r.MovementDate >= StartDatePicker.SelectedDate.Value);
                }

                if (EndDatePicker.SelectedDate.HasValue)
                {
                    filtered = filtered.Where(r => r.MovementDate <= EndDatePicker.SelectedDate.Value);
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
            MaterialComboBox.SelectedIndex = -1;
            MovementTypeComboBox.SelectedIndex = 0; // Установка "Все"
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
                    FileName = $"Отчёт о движении сырья {DateTime.Now:dd.MM.yyyy}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Отчёт о движении сырья");

                        // Заголовки
                        worksheet.Cells[1, 1].Value = "Дата";
                        worksheet.Cells[1, 2].Value = "Сырьё";
                        worksheet.Cells[1, 3].Value = "Тип движения";
                        worksheet.Cells[1, 4].Value = "Количество";
                        worksheet.Cells[1, 5].Value = "Ед. изм.";
                        worksheet.Cells[1, 6].Value = "Пользователь";
                        worksheet.Cells[1, 7].Value = "Накладная";
                        worksheet.Cells[1, 8].Value = "Примечание";

                        // Данные
                        int row = 2;
                        foreach (var report in _filteredReports)
                        {
                            worksheet.Cells[row, 1].Value = report.MovementDate;
                            worksheet.Cells[row, 1].Style.Numberformat.Format = "dd.MM.yyyy";
                            worksheet.Cells[row, 2].Value = report.MaterialName;
                            worksheet.Cells[row, 3].Value = report.MovementType;
                            worksheet.Cells[row, 4].Value = report.Quantity;
                            worksheet.Cells[row, 5].Value = report.UnitName;
                            worksheet.Cells[row, 6].Value = report.UserName;
                            worksheet.Cells[row, 7].Value = report.InvoiceNumber;
                            worksheet.Cells[row, 8].Value = report.Note;
                            row++;
                        }

                        // Форматирование
                        worksheet.Cells[1, 1, 1, 8].Style.Font.Bold = true;
                        worksheet.Cells[1, 1, row - 1, 8].AutoFitColumns();

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

    public class MaterialMovementReportItem
    {
        public DateTime MovementDate { get; set; }
        public string MaterialName { get; set; }
        public string MovementType { get; set; } // "Приход" или "Расход"
        public decimal Quantity { get; set; }
        public string UnitName { get; set; } // Единица измерения
        public string UserName { get; set; }
        public string InvoiceNumber { get; set; }
        public string Note { get; set; } // Причина расхода
    }
} 