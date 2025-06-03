using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Gofroychetqq
{
    public partial class ManageAccountsWindow : Window
    {
        private etonEntities _db = new etonEntities();
        public ManageAccountsWindow()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            UsersDataGrid.ItemsSource = _db.User.Include("Role").ToList();
        }

        private void AddUser_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddOrEditUserWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            var user = UsersDataGrid.SelectedItem as User;
            if (user == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (MessageBox.Show($"Удалить пользователя {user.Login}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _db.User.Remove(user);
                _db.SaveChanges();
                LoadUsers();
            }
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var user = UsersDataGrid.SelectedItem as User;
            if (user == null)
            {
                MessageBox.Show("Выберите пользователя для изменения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var editWindow = new AddOrEditUserWindow(user);
            if (editWindow.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
} 