using System.Linq;
using System.Windows;

namespace Gofroychetqq
{
    public class MaterialViewModel
    {
        public int MaterialID { get; set; }
        public string Name { get; set; }
        public MaterialType MaterialType { get; set; }
        public double MinimumStock { get; set; }
        public double Stock { get; set; }
    }

    public partial class RawMaterialsWindow : Window
    {
        private etonEntities db = new etonEntities();
        public RawMaterialsWindow()
        {
            InitializeComponent();
            LoadMaterials();
        }

        private void LoadMaterials()
        {
            db = new etonEntities(); // Пересоздаём контекст для получения свежих данных
            var materials = db.Material.ToList().Select(m => new MaterialViewModel
            {
                MaterialID = m.MaterialID,
                Name = m.Name,
                MaterialType = m.MaterialType,
                MinimumStock = m.MinimumStock,
                Stock = (db.Prihod.Where(p => p.MaterialID == m.MaterialID).Sum(p => (double?)p.Quantity) ?? 0)
                        - (db.Rashod.Where(r => r.MaterialID == m.MaterialID).Sum(r => (double?)r.Quantity) ?? 0)
            }).ToList();
            RawMaterialsDataGrid.ItemsSource = materials;
        }

        private void AddMaterial_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditMaterialWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadMaterials();
            }
        }

        private void EditMaterial_Click(object sender, RoutedEventArgs e)
        {
            var selectedMaterial = RawMaterialsDataGrid.SelectedItem as MaterialViewModel;
            if (selectedMaterial != null)
            {
                var material = db.Material.FirstOrDefault(m => m.MaterialID == selectedMaterial.MaterialID);
                if (material != null)
                {
                    var editWindow = new EditMaterialWindow(material);
                    if (editWindow.ShowDialog() == true)
                    {
                        LoadMaterials();
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите материал для редактирования", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void WriteOffMaterial_Click(object sender, RoutedEventArgs e)
        {
            var writeOffWindow = new WriteOffMaterialWindow();
            if (writeOffWindow.ShowDialog() == true)
            {
                LoadMaterials();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
} 