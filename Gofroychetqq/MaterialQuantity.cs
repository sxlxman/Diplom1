using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Gofroychetqq
{
    public class MaterialQuantity : INotifyPropertyChanged
    {
        private Material _material;
        private decimal _quantity;

        public Material Material
        {
            get => _material;
            set
            {
                _material = value;
                OnPropertyChanged();
            }
        }

        public decimal Quantity
        {
            get => _quantity;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Количество не может быть отрицательным");
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 