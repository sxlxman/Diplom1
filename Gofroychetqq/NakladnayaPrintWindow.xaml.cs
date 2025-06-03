using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gofroychetqq
{
    public partial class NakladnayaPrintWindow : Window
    {
        private Supply _supply;
        private etonEntities _context;

        public NakladnayaPrintWindow(Supply supply)
        {
            InitializeComponent();
            _supply = supply;
            _context = new etonEntities();
            LoadNakladnayaData();
        }

        private void LoadNakladnayaData()
        {
            // Загрузка данных поставщика
            var supplier = _context.Supplier.Find(_supply.SupplierID);
            
            // Создание содержимого накладной
            var content = new StackPanel();
            
            // Заголовок
            content.Children.Add(new TextBlock
            {
                Text = "НАКЛАДНАЯ",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // Информация о поставщике
            content.Children.Add(CreateInfoBlock("Поставщик:", supplier.Name));
            content.Children.Add(CreateInfoBlock("ИНН:", supplier.INN));
            content.Children.Add(CreateInfoBlock("Адрес:", supplier.Address));
            content.Children.Add(CreateInfoBlock("Телефон:", supplier.Phone));

            // Информация о накладной
            content.Children.Add(CreateInfoBlock("Номер документа:", _supply.DocumentNumber));
            content.Children.Add(CreateInfoBlock("Дата:", _supply.SupplyDate.ToShortDateString()));

            // Таблица материалов
            var materialsGrid = new Grid();
            materialsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            materialsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            materialsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Заголовки таблицы
            AddGridHeader(materialsGrid, "Наименование", 0, 0);
            AddGridHeader(materialsGrid, "Количество", 1, 0);
            AddGridHeader(materialsGrid, "Ед. изм.", 2, 0);

            // Данные материалов
            int row = 1;
            foreach (var prihod in _supply.Prihod)
            {
                var material = _context.Material.Find(prihod.MaterialID);
                var materialType = _context.MaterialType.Find(material.MaterialTypeID);
                var unit = _context.Unit.Find(materialType.UnitID);

                AddGridCell(materialsGrid, material.Name, 0, row);
                AddGridCell(materialsGrid, prihod.Quantity.ToString(), 1, row);
                AddGridCell(materialsGrid, unit.Name, 2, row);
                row++;
            }

            content.Children.Add(materialsGrid);

            // Добавляем содержимое в окно
            NakladnayaContent.Children.Add(content);
        }

        private UIElement CreateInfoBlock(string label, string value)
        {
            var stack = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
            stack.Children.Add(new TextBlock { Text = label, FontWeight = FontWeights.Bold, Width = 150 });
            stack.Children.Add(new TextBlock { Text = value });
            return stack;
        }

        private void AddGridHeader(Grid grid, string text, int column, int row)
        {
            var header = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5),
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(header, column);
            Grid.SetRow(header, row);
            grid.Children.Add(header);
        }

        private void AddGridCell(Grid grid, string text, int column, int row)
        {
            var cell = new TextBlock
            {
                Text = text,
                Margin = new Thickness(5),
                TextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(cell, column);
            Grid.SetRow(cell, row);
            grid.Children.Add(cell);
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Создаем временный контейнер для печати
                    var printContainer = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(20),
                        Child = NakladnayaContent
                    };

                    // Печатаем содержимое
                    printDialog.PrintVisual(printContainer, "Накладная");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}