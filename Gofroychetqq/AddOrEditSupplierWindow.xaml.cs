using System.Windows;
using System.Data.Entity;

namespace Gofroychetqq
{
    public partial class AddOrEditSupplierWindow : Window
    {
        private etonEntities _context;
        private Supplier _supplier;
        private bool _isEdit;

        public AddOrEditSupplierWindow(etonEntities context, Supplier supplier = null)
        {
            InitializeComponent();
            _context = context;
            _supplier = supplier;
            _isEdit = supplier != null;

            if (_isEdit)
            {
                TitleText.Text = "Редактирование поставщика";
                NameTextBox.Text = _supplier.Name;
                INNTextBox.Text = _supplier.INN;
                AddressTextBox.Text = _supplier.Address;
                PhoneTextBox.Text = _supplier.Phone;
                EmailTextBox.Text = _supplier.Email;
                ContactPersonTextBox.Text = _supplier.ContactPerson;
                NoteTextBox.Text = _supplier.Note;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название поставщика", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_isEdit)
            {
                _supplier.Name = NameTextBox.Text;
                _supplier.INN = INNTextBox.Text;
                _supplier.Address = AddressTextBox.Text;
                _supplier.Phone = PhoneTextBox.Text;
                _supplier.Email = EmailTextBox.Text;
                _supplier.ContactPerson = ContactPersonTextBox.Text;
                _supplier.Note = NoteTextBox.Text;
            }
            else
            {
                var newSupplier = new Supplier
                {
                    Name = NameTextBox.Text,
                    INN = INNTextBox.Text,
                    Address = AddressTextBox.Text,
                    Phone = PhoneTextBox.Text,
                    Email = EmailTextBox.Text,
                    ContactPerson = ContactPersonTextBox.Text,
                    Note = NoteTextBox.Text
                };
                _context.Supplier.Add(newSupplier);
            }

            try
            {
                _context.SaveChanges();
                DialogResult = true;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}