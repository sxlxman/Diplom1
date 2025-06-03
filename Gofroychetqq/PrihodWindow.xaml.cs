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
using System.Collections.ObjectModel;
using System.Data.Entity;

namespace Gofroychetqq
{
    /// <summary>
    /// Логика взаимодействия для PrihodWindow.xaml
    /// </summary>
    public partial class PrihodWindow : Window
    {
        private etonEntities _context;
        private List<Material> _materials;
        private User _currentUser;
        private Supply _currentSupply;

        public PrihodWindow(User currentUser)
        {
            InitializeComponent();
            _context = new etonEntities();
            _currentUser = currentUser;
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Загрузка поставщиков
                SupplierComboBox.ItemsSource = _context.Supplier.ToList();

                // Загрузка материалов
                _materials = _context.Material.ToList();
                var materialColumn = (DataGridComboBoxColumn)MaterialsDataGrid.Columns[0];
                materialColumn.ItemsSource = _materials;

                // Установка текущей даты
                SupplyDatePicker.SelectedDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInput())
                    return;

                // 1. Создаем поставку
                _currentSupply = new Supply
                {
                    SupplierID = ((Supplier)SupplierComboBox.SelectedItem).SupplierID,
                    DocumentNumber = DocumentNumberTextBox.Text,
                    SupplyDate = SupplyDatePicker.SelectedDate.Value,
                    Note = NoteTextBox.Text,
                    SupplyStatusID = 1 // Статус "Новая поставка"
                };
                _context.Supply.Add(_currentSupply);
                _context.SaveChanges();

                // 2. Создаем накладную
                var nakladnaya = new Nakladnaya
                {
                    Number = GetNextNakladnayaNumber(),
                    CreateDate = DateTime.Now,
                    TypeNakladnaya = 1 // Тип "Приходная накладная"
                };
                _context.Nakladnaya.Add(nakladnaya);
                _context.SaveChanges();

                // 3. Создаем записи прихода
                foreach (var item in MaterialsDataGrid.Items)
                {
                    var materialItem = item as dynamic;
                    if (materialItem?.Material != null && materialItem.Quantity > 0)
                    {
                        var prihod = new Prihod
                        {
                            SupplyID = _currentSupply.SupplyID,
                            MaterialID = materialItem.Material.MaterialID,
                            Quantity = (double)materialItem.Quantity,
                            PrihodDate = DateTime.Now,
                            UserID = _currentUser.UserID,
                            NakladnayaID = nakladnaya.NakladnayaID
                        };
                        _context.Prihod.Add(prihod);
                    }
                }

                _context.SaveChanges();
                MessageBox.Show("Приход сырья успешно оформлен", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                // Активируем кнопку печати
                PrintButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInput()
        {
            if (SupplierComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DocumentNumberTextBox.Text))
            {
                MessageBox.Show("Введите номер документа", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!SupplyDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату поставки", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка на наличие материалов
            if (MaterialsDataGrid.Items.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один материал", "Предупреждение",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            // Проверка на корректность количества
            foreach (var item in MaterialsDataGrid.Items)
            {
                var materialItem = item as dynamic;
                if (materialItem?.Material == null || materialItem.Quantity <= 0)
                {
                    MessageBox.Show("Укажите корректное количество для всех материалов", "Предупреждение",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }

            return true;
        }

        private int GetNextNakladnayaNumber()
        {
            var lastNakladnaya = _context.Nakladnaya
                .OrderByDescending(n => n.Number)
                .FirstOrDefault();

            return lastNakladnaya?.Number + 1 ?? 1;
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем базовый каталог приложения
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                // Формируем полный путь к файлу PDF в подкаталоге Templates
                string pdfFilePath = System.IO.Path.Combine(baseDirectory, "Templates", "nakladnaya.pdf");

                // Проверяем, существует ли файл
                if (System.IO.File.Exists(pdfFilePath))
                {
                    // Открываем PDF файл в браузере
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(pdfFilePath) { UseShellExecute = true });
                }
                else
                {
                    MessageBox.Show($"Файл шаблона накладной не найден: {pdfFilePath}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при открытии накладной: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            var materials = MaterialsDataGrid.ItemsSource as ObservableCollection<MaterialQuantity>;
            if (materials == null)
            {
                materials = new ObservableCollection<MaterialQuantity>();
                MaterialsDataGrid.ItemsSource = materials;
            }
            materials.Add(new MaterialQuantity { Quantity = 0 });
        }

        private void DeleteMaterial_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                var material = button.DataContext as MaterialQuantity;
                if (material != null)
                {
                    var materials = MaterialsDataGrid.ItemsSource as ObservableCollection<MaterialQuantity>;
                    materials?.Remove(material);
                }
            }
        }

        private void MaterialsDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Количество")
            {
                if (e.EditingElement is TextBox textBox)
                {
                    var editedValue = textBox.Text;
                    var materialItem = e.Row.Item as MaterialQuantity;

                    if (materialItem != null)
                    {
                        if (decimal.TryParse(editedValue, out decimal quantity))
                        {
                            // Успешно преобразовано, обновляем свойство
                            materialItem.Quantity = quantity;
                        }
                        else
                        {
                            // Ошибка преобразования, возможно, стоит уведомить пользователя или сбросить значение
                            // Например, можно вывести сообщение или оставить предыдущее значение
                             MessageBox.Show("Введите корректное числовое значение для количества.", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                            // Можно также сбросить текст в TextBox или значение в объекте
                             textBox.Undo(); // Отменить ввод пользователя
                        }
                    }
                }
            }
        }
    }
}
