using System.Linq;
using System.Windows;

namespace Gofroychetqq
{
    public partial class ChangeRoleWindow : Window
    {
        private etonEntities _db = new etonEntities();
        private User _user;
        public ChangeRoleWindow(User user)
        {
            InitializeComponent();
            _user = _db.User.Find(user.UserID);
            RoleComboBox.ItemsSource = _db.Role.ToList();
            RoleComboBox.SelectedValue = _user.RoleID;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (RoleComboBox.SelectedItem == null) return;
            _user.RoleID = (int)RoleComboBox.SelectedValue;
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