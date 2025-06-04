using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.Entity;

namespace Gofroychetqq
{
    /// <summary>
    /// Логика взаимодействия для SupplyWindow.xaml
    /// </summary>
    public partial class SupplyWindow : Window
    {
        private etonEntities db = new etonEntities();
        private Supply selectedSupply;
        private User _currentUser;

        public SupplyWindow(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadSupplies();
            SetupEventHandlers();
        }

        private void LoadSupplies()
        {
            db = new etonEntities();
            var supplies = db.Supply
                .Include("Supplier")
                .Include("SupplyStatus")
                .OrderByDescending(s => s.SupplyDate)
                .ToList();
            SuppliesDataGrid.ItemsSource = supplies;
        }

        private void SetupEventHandlers()
        {
            SearchTextBox.TextChanged += SearchTextBox_TextChanged;
            SuppliesDataGrid.SelectionChanged += SuppliesDataGrid_SelectionChanged;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchTextBox.Text.ToLower();
            var supplies = db.Supply
                .Include("Supplier")
                .Include("SupplyStatus")
                .Where(s => s.DocumentNumber.ToLower().Contains(searchText) ||
                           s.Supplier.Name.ToLower().Contains(searchText))
                .OrderByDescending(s => s.SupplyDate)
                .ToList();
            SuppliesDataGrid.ItemsSource = supplies;
        }

        private void SuppliesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedSupply = SuppliesDataGrid.SelectedItem as Supply;
        }

        private void AddSupply_Click(object sender, RoutedEventArgs e)
        {
            var prihodWindow = new PrihodWindow(_currentUser);
            if (prihodWindow.ShowDialog() == true)
            {
                LoadSupplies();
            }
        }

        private void EditSupply_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSupply == null)
            {
                MessageBox.Show("Пожалуйста, выберите поставку для редактирования", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editSupplyWindow = new EditSupplyWindow(selectedSupply);
            if (editSupplyWindow.ShowDialog() == true)
            {
                LoadSupplies();
            }
        }

        private void DeleteSupply_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSupply == null)
            {
                MessageBox.Show("Пожалуйста, выберите поставку для удаления", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить эту поставку?", "Подтверждение", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var relatedPrihods = db.Prihod.Where(p => p.SupplyID == selectedSupply.SupplyID).ToList();
                    db.Prihod.RemoveRange(relatedPrihods);

                    db.Supply.Remove(selectedSupply);
                    db.SaveChanges();
                    LoadSupplies();
                    MessageBox.Show("Поставка успешно удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении поставки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var supply = button.DataContext as Supply;
            if (supply != null)
            {
                var supplyDetailsWindow = new SupplyDetailsWindow(supply.SupplyID);
                supplyDetailsWindow.Owner = this;
                supplyDetailsWindow.ShowDialog();
            }
        }
    }
}
