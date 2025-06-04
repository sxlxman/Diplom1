using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Gofroychetqq
{
    public partial class EditSupplyWindow : Window
    {
        private readonly etonEntities db;
        private Supply _supply;

        public EditSupplyWindow(Supply supply)
        {
            InitializeComponent();
            db = new etonEntities();
            _supply = supply;
            LoadSupplyData();
            LoadStatuses();
        }

        private void LoadSupplyData()
        {
            DocumentNumberTextBox.Text = _supply.DocumentNumber;
            SupplyDatePicker.SelectedDate = _supply.SupplyDate;
            NoteTextBox.Text = _supply.Note;
            // StatusComboBox будет загружен отдельно в LoadStatuses()
        }

        private void LoadStatuses()
        {
            StatusComboBox.ItemsSource = db.SupplyStatus.ToList();
            // Выбираем текущий статус поставки
            StatusComboBox.SelectedValue = _supply.SupplyStatusID;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Обновляем данные поставки из полей окна
                _supply.DocumentNumber = DocumentNumberTextBox.Text;
                _supply.SupplyDate = SupplyDatePicker.SelectedDate.Value;
                _supply.Note = NoteTextBox.Text;
                _supply.SupplyStatusID = (int)StatusComboBox.SelectedValue;

                db.Entry(_supply).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                MessageBox.Show("Изменения успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}