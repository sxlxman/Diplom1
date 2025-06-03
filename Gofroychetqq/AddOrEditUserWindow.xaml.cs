using System.Linq;
using System.Windows;

namespace Gofroychetqq
{
    public partial class AddOrEditUserWindow : Window
    {
        private etonEntities _db = new etonEntities();
        private User _editUser;
        public AddOrEditUserWindow() : this(null) { }
        public AddOrEditUserWindow(User user)
        {
            InitializeComponent();
            RoleComboBox.ItemsSource = _db.Role.ToList();
            _editUser = null;
            if (user != null)
            {
                // Редактирование
                _editUser = _db.User.Find(user.UserID);
                LoginBox.Text = _editUser.Login;
                LoginBox.IsEnabled = false;
                PasswordBox.Password = _editUser.Password;
                SurnameBox.Text = _editUser.Surname;
                NameBox.Text = _editUser.Name;
                PatronymicBox.Text = _editUser.Patronymic;
                RoleComboBox.SelectedValue = _editUser.RoleID;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                string.IsNullOrWhiteSpace(SurnameBox.Text) || string.IsNullOrWhiteSpace(NameBox.Text) ||
                RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_editUser == null)
            {
                // Добавление
                var user = new User
                {
                    Login = LoginBox.Text.Trim(),
                    Password = PasswordBox.Password.Trim(),
                    Surname = SurnameBox.Text.Trim(),
                    Name = NameBox.Text.Trim(),
                    Patronymic = PatronymicBox.Text.Trim(),
                    RoleID = (int)RoleComboBox.SelectedValue
                };
                _db.User.Add(user);
            }
            else
            {
                // Редактирование
                _editUser.Password = PasswordBox.Password.Trim();
                _editUser.Surname = SurnameBox.Text.Trim();
                _editUser.Name = NameBox.Text.Trim();
                _editUser.Patronymic = PatronymicBox.Text.Trim();
                _editUser.RoleID = (int)RoleComboBox.SelectedValue;
            }
            _db.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 