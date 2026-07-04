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
            CancelReservationCommand = new Command<ReadReserOutputDto>(async (r) => await CancelReservationAsync(r));
        }

        public ICommand LoadCommand { get; }
        public ICommand CancelReservationCommand { get; }

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
                await Shell.Current.DisplayAlertAsync("错误", $"加载预约记录失败: {ex.Message}", "确定");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CancelReservationAsync(ReadReserOutputDto reservation)
        {
            if (reservation == null) return;

            var confirm = await Shell.Current.DisplayAlertAsync("取消预约",
                $"确定取消预约 {reservation.ReservationId} 吗？\n房号: {reservation.ReservationRoomNumber}",
                "确定", "取消");

            if (!confirm) return;

            try
            {
                IsLoading = true;
                var success = await _reservationService.CancelReservationAsync(reservation.Id ?? 0);

                if (success)
                {
                    await Shell.Current.DisplayAlertAsync("成功", "预约已取消", "确定");
                    await LoadReservationsAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("失败", "取消预约失败，请稍后重试", "确定");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("错误", $"取消预约失败: {ex.Message}", "确定");
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
