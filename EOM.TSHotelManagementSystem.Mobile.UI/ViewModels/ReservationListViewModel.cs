using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class ReservationListViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly IReservationService _reservationService;
        private bool _isLoading;
        private bool _isInitializing;

        public ReservationListViewModel(IReservationService reservationService)
        {
            _reservationService = reservationService;
            LoadCommand = new Command(async () => await LoadReservationsAsync());
        }

        public ICommand LoadCommand { get; }

        public ObservableCollection<ReadReserOutputDto> Reservations { get; set; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set => SetField(ref _isLoading, value);
        }

        public bool IsInitializing
        {
            get => _isInitializing;
            set => SetField(ref _isInitializing, value);
        }

        public bool IsEmpty => Reservations.Count == 0 && !IsInitializing;

        public async void OnViewAppearing()
        {
            try
            {
                IsInitializing = true;
                await LoadReservationsAsync();
            }
            finally
            {
                IsInitializing = false;
            }
        }

        private async Task LoadReservationsAsync()
        {
            try
            {
                IsLoading = true;

                var reservations = await _reservationService.GetMyReservationsAsync();

                Reservations.Clear();
                foreach (var item in reservations)
                {
                    Reservations.Add(item);
                }

                OnPropertyChanged(nameof(IsEmpty));
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("错误", $"加载预约记录失败: {ex.Message}", "确定");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void OnViewDisappearing()
        {
        }
    }
}
