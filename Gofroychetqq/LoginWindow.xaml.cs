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
using Gofroychetqq;
using static Gofroychetqq.StockNotificationHelper;
using System.Media;
using System.Windows.Media.Animation;

namespace Gofroychetqq
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            ErrorText.Text = "";
            // Проверяем, что TextBlock создан
            if (ErrorText == null)
            {
                MessageBox.Show("ErrorText не инициализирован!");
            }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Foreground = Brushes.Red;
            SystemSounds.Exclamation.Play();

            // Анимация появления ошибки
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3)
            };
            ErrorText.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                try
                {
                    ErrorText.Text = "Введите логин и пароль!";
                    ErrorText.Foreground = Brushes.Red;
                    SystemSounds.Exclamation.Play();
                    ErrorText.UpdateLayout();
                    // Проверяем, что текст установлен
                    MessageBox.Show($"Текст ошибки: {ErrorText.Text}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при установке текста: {ex.Message}");
                }
                return;
            }

            using (var db = new etonEntities())
            {
                var user = db.User.FirstOrDefault(u => u.Login == login && u.Password == password);
                if (user != null)
                {
                    // Открываем главное окно
                    MainWindow mainWindow = new MainWindow(user);
                    mainWindow.Show();
                    CheckAndShowLowStockNotification();
                    this.Close();
                }
                else
                {
                    try
                    {
                        ErrorText.Text = "Неверный логин или пароль!";
                        ErrorText.Foreground = Brushes.Red;
                        SystemSounds.Exclamation.Play();
                        PasswordBox.Password = "";
                        PasswordBox.Focus();
                        ErrorText.UpdateLayout();
                        // Проверяем, что текст установлен
                        MessageBox.Show($" {ErrorText.Text}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при установке текста: {ex.Message}");
                    }
                }
            }
        }
    }
}
