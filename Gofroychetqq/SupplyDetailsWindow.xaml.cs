using System.Linq;
using System.Windows;
using Gofroychetqq;
using System.Data.Entity;

namespace Gofroychetqq
{
    public partial class SupplyDetailsWindow : Window
    {
        private readonly etonEntities db;
        private int _supplyId;

        public SupplyDetailsWindow(int supplyId)
        {
            InitializeComponent();
            db = new etonEntities();
            _supplyId = supplyId;
            LoadSupplyDetails();
        }

        private void LoadSupplyDetails()
        {
            // Загружаем поставку по ID, включая связанные данные
            var supply = db.Supply
                         .Include(s => s.Supplier)
                         .Include(s => s.SupplyStatus)
                         .Include(s => s.Prihod.Select(p => p.Material.MaterialType.Unit))
                         .FirstOrDefault(s => s.SupplyID == _supplyId);

            if (supply == null)
            {
                MessageBox.Show("Поставка не найдена.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                return;
            }

            // Отображаем основную информацию о поставке
            DocumentNumberTextBlock.Text = supply.DocumentNumber;
            SupplyDateTextBlock.Text = supply.SupplyDate.ToShortDateString();
            SupplierTextBlock.Text = supply.Supplier.Name;
            StatusTextBlock.Text = supply.SupplyStatus.Name;
            NoteTextBlock.Text = supply.Note;

            // Отображаем список материалов в DataGrid
            MaterialsDataGrid.ItemsSource = supply.Prihod.ToList();
        }
    }
}