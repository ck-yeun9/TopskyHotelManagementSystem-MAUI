using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class CategoryDto : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Code { get; set; }
        public string Label { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
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
