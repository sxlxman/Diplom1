using System.Windows;
using System.Linq;

namespace Gofroychetqq
{
    public partial class EditMaterialWindow : Window
    {
        private etonEntities db = new etonEntities();
        private Material _material;
        public EditMaterialWindow()
        {
            InitializeComponent();
            TypeComboBox.ItemsSource = db.MaterialType.ToList();
        }
        public EditMaterialWindow(Material material) : this()
        {
            _material = db.Material.FirstOrDefault(x => x.MaterialID == material.MaterialID);
            if (_material != null)
            {
                NameTextBox.Text = _material.Name;
                TypeComboBox.SelectedValue = _material.MaterialTypeID;
                MinStockTextBox.Text = _material.MinimumStock.ToString();
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_material == null)
            {
                _material = new Material();
                db.Material.Add(_material);
            }
            _material.Name = NameTextBox.Text;
            _material.MaterialTypeID = (int)TypeComboBox.SelectedValue;
            double minStock;
            if (double.TryParse(MinStockTextBox.Text, out minStock))
                _material.MinimumStock = minStock;
            db.SaveChanges();
            DialogResult = true;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
} 