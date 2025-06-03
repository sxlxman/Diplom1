using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Gofroychetqq
{
    public partial class WriteOffMaterialWindow : Window
    {
        private readonly etonEntities db;
        private Material selectedMaterial;

        public WriteOffMaterialWindow()
        {
            InitializeComponent();
            db = new etonEntities();

            // Загружаем список материалов
            MaterialComboBox.ItemsSource = db.Material.ToList();

            // Загружаем список причин списания
            ReasonComboBox.ItemsSource = db.Reason.ToList();

            // Устанавливаем текущую дату
            RashodDatePicker.SelectedDate = DateTime.Now;

            // Генерируем номер накладной
            var lastNakladnaya = db.Nakladnaya.OrderByDescending(n => n.Number).FirstOrDefault();
            int nextNumber = (lastNakladnaya?.Number ?? 0) + 1;
            NakladnayaNumberTextBox.Text = nextNumber.ToString();
        }

        private void MaterialComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedMaterial = MaterialComboBox.SelectedItem as Material;
            if (selectedMaterial != null)
            {
                // Рассчитываем и отображаем текущий остаток
                var currentStock = (db.Prihod.Where(p => p.MaterialID == selectedMaterial.MaterialID).Sum(p => (double?)p.Quantity) ?? 0)
                                - (db.Rashod.Where(r => r.MaterialID == selectedMaterial.MaterialID).Sum(r => (double?)r.Quantity) ?? 0);
                CurrentStockTextBlock.Text = $"{currentStock} {selectedMaterial.MaterialType.Unit.Name}";
            }
            else
            {
                CurrentStockTextBlock.Text = string.Empty;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedMaterial == null)
                {
                    MessageBox.Show("Пожалуйста, выберите материал", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем введенные данные
                if (!double.TryParse(QuantityTextBox.Text, out double quantity) || quantity <= 0)
                {
                    MessageBox.Show("Пожалуйста, введите корректное количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (ReasonComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите причину списания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!int.TryParse(NakladnayaNumberTextBox.Text, out int nakladnayaNumber) || nakladnayaNumber <= 0)
                {
                    MessageBox.Show("Пожалуйста, введите корректный номер накладной", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!RashodDatePicker.SelectedDate.HasValue)
                {
                    MessageBox.Show("Пожалуйста, выберите дату списания", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверяем остаток материала
                var currentStock = (db.Prihod.Where(p => p.MaterialID == selectedMaterial.MaterialID).Sum(p => (double?)p.Quantity) ?? 0)
                                - (db.Rashod.Where(r => r.MaterialID == selectedMaterial.MaterialID).Sum(r => (double?)r.Quantity) ?? 0);

                if (quantity > currentStock)
                {
                    MessageBox.Show($"Недостаточно материала. Доступно: {currentStock} {selectedMaterial.MaterialType.Unit.Name}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Создаем накладную
                var nakladnaya = new Nakladnaya
                {
                    Number = nakladnayaNumber,
                    CreateDate = DateTime.Now,
                    TypeNakladnaya = 2 // Тип накладной для списания
                };
                db.Nakladnaya.Add(nakladnaya);
                db.SaveChanges();

                // Создаем запись о списании
                var rashod = new Rashod
                {
                    MaterialID = selectedMaterial.MaterialID,
                    Quantity = quantity,
                    RashodDate = RashodDatePicker.SelectedDate.Value,
                    ReasonID = ((Reason)ReasonComboBox.SelectedItem).ReasonID,
                    UserID = 1, // TODO: Заменить на ID текущего пользователя
                    NakladnayaID = nakladnaya.NakladnayaID
                };
                db.Rashod.Add(rashod);
                db.SaveChanges();

                MessageBox.Show("Списание успешно оформлено", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
    }
}