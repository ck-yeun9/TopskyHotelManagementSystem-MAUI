using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class ProductDto : INotifyPropertyChanged
    {
        private int _cartQuantity;
        private int _originalStock;

        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public decimal ProductPrice { get; set; }
        public string Specification { get; set; }

        public int Stock { get; set; }

        public int OriginalStock
        {
            get => _originalStock;
            set
            {
                if (_originalStock != value)
                {
                    _originalStock = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AvailableStock));
                }
            }
        }

        public int AvailableStock => OriginalStock - CartQuantity;

        public string ProductCategory { get; set; }

        public int CartQuantity
        {
            get => _cartQuantity;
            set
            {
                if (_cartQuantity != value)
                {
                    _cartQuantity = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(AvailableStock));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
