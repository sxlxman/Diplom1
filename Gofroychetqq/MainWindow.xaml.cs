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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gofroychetqq
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private User _currentUser;
        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            if (_currentUser != null && _currentUser.Role != null)
            {
                UserInfoText.Text = $"Пользователь: {_currentUser.Surname} {_currentUser.Name} {_currentUser.Patronymic} | Роль: {_currentUser.Role.Name}";
                if (_currentUser.RoleID == 1)
                {
                    ManageAccountsButton.Visibility = Visibility.Visible;
                }
            }
            else if (_currentUser != null)
            {
                UserInfoText.Text = $"Пользователь: {_currentUser.Surname} {_currentUser.Name} {_currentUser.Patronymic}";
            }
            // Загрузка фото
            if (_currentUser != null && !string.IsNullOrEmpty(_currentUser.Photo))
            {
                try
                {
                    var image = new BitmapImage(new Uri(_currentUser.Photo, UriKind.RelativeOrAbsolute));
                    UserPhoto.Source = image;
                }
                catch
                {
                    UserPhoto.Source = null;
                }
            }
            else
            {
                UserPhoto.Source = null;
            }
        }
        public MainWindow() : this(null) { }

        private void RawMaterialsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new RawMaterialsWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ManageAccountsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ManageAccountsWindow();
            window.Owner = this;
            window.ShowDialog();
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реализовать открытие окна товаров
            MessageBox.Show("Функционал в разработке", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Реализовать открытие окна категорий
            MessageBox.Show("Функционал в разработке", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SuppliersButton_Click(object sender, RoutedEventArgs e)
        {
            var suppliersWindow = new SuppliersWindow();
            suppliersWindow.Show();
        }

        private void SuppliesButton_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно поставок и передаем текущего пользователя
            var supplyWindow = new SupplyWindow(_currentUser);
            supplyWindow.Owner = this;
            supplyWindow.ShowDialog();
        }

        private void IncomeButton_Click(object sender, RoutedEventArgs e)
        {
            var prihodWindow = new PrihodWindow(_currentUser);
            prihodWindow.Owner = this;
            prihodWindow.ShowDialog();
        }

        private void OutcomeButton_Click(object sender, RoutedEventArgs e)
        {
            var writeOffWindow = new WriteOffMaterialWindow();
            writeOffWindow.Owner = this;
            writeOffWindow.ShowDialog();
        }

        private void RemainsButton_Click(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new RemainsReportsWindow();
            reportsWindow.Owner = this;
            reportsWindow.ShowDialog();
        }

        private void MovementButton_Click(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new MaterialMovementReportsWindow();
            reportsWindow.Owner = this;
            reportsWindow.ShowDialog();
        }

        private void SupplyReportsButton_Click(object sender, RoutedEventArgs e)
        {
            var reportsWindow = new SupplyReportsWindow();
            reportsWindow.Owner = this;
            reportsWindow.ShowDialog();
        }

        private void CreatePrihodButton_Click(object sender, RoutedEventArgs e)
        {
            var prihodWindow = new PrihodWindow(_currentUser);
            prihodWindow.Owner = this;
            prihodWindow.ShowDialog();
        }
    }
}
