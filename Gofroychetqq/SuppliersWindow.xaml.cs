using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace Gofroychetqq
{
    public partial class SuppliersWindow : Window
    {
        private etonEntities _context;

        public SuppliersWindow()
        {
            InitializeComponent();
            _context = new etonEntities();
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            _context.Supplier.Load();
            SuppliersDataGrid.ItemsSource = _context.Supplier.Local;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddOrEditSupplierWindow(_context);
            if (addWindow.ShowDialog() == true)
            {
                LoadSuppliers();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSupplier = SuppliersDataGrid.SelectedItem as Supplier;
            if (selectedSupplier == null)
            {
                MessageBox.Show("Пожалуйста, выберите поставщика для редактирования",
                              "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editWindow = new AddOrEditSupplierWindow(_context, selectedSupplier);
            if (editWindow.ShowDialog() == true)
            {
                LoadSuppliers();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedSupplier = SuppliersDataGrid.SelectedItem as Supplier;
            if (selectedSupplier == null)
            {
                MessageBox.Show("Пожалуйста, выберите поставщика для удаления",
                              "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить поставщика '{selectedSupplier.Name}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Supplier.Remove(selectedSupplier);
                    _context.SaveChanges();
                    LoadSuppliers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}