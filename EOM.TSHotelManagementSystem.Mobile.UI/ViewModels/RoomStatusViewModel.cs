using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class RoomStatusViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly IStaffService _staffService;

        public RoomStatusViewModel(IStaffService staffService)
        {
            _staffService = staffService;

            RoomStatusList = new ObservableCollection<RoomStatusOutputDto>();
            UpdateRoomStatusCommand = new Command<RoomStatusOutputDto>(async (room) => await UpdateRoomStatusAsync(room));
            RefreshCommand = new Command(async () => await LoadRoomStatusListAsync());
        }

        public ObservableCollection<RoomStatusOutputDto> RoomStatusList { get; }

        private int _selectedFloorId;
        public int SelectedFloorId
        {
            get => _selectedFloorId;
            set
            {
                if (SetField(ref _selectedFloorId, value))
                {
                    _ = LoadRoomStatusListAsync();
                }
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetField(ref _isBusy, value);
        }

        public ICommand UpdateRoomStatusCommand { get; }
        public ICommand RefreshCommand { get; }

        public void OnViewAppearing()
        {
            _ = LoadRoomStatusListAsync();
        }

        public void OnViewDisappearing()
        {
        }

        private async Task LoadRoomStatusListAsync()
        {
            IsBusy = true;
            var result = await _staffService.GetRoomStatusListAsync(_selectedFloorId);

            if (result.StatusCode == StatusCodeConstants.Success && result.listSource != null)
            {
                RoomStatusList.Clear();
                foreach (var item in result.listSource)
                {
                    RoomStatusList.Add(item);
                }
                ErrorMessage = string.Empty;
            }
            else
            {
                ErrorMessage = result.Message ?? "加载房间状态失败";
            }
            IsBusy = false;
        }

        private async Task UpdateRoomStatusAsync(RoomStatusOutputDto room)
        {
            if (room == null) return;

            var nextStates = new[] { 0, 1, 2, 3, 4 };
            var currentIndex = Array.IndexOf(nextStates, room.RoomState);
            var nextState = currentIndex >= 0 && currentIndex < nextStates.Length - 1
                ? nextStates[currentIndex + 1]
                : nextStates[0];

            var input = new RoomStatusInputDto
            {
                RoomId = room.RoomId,
                FloorId = room.FloorId,
                RoomState = nextState
            };

            IsBusy = true;
            var result = await _staffService.UpdateRoomStatusAsync(input);

            if (result.StatusCode == StatusCodeConstants.Success)
            {
                await LoadRoomStatusListAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "更新房间状态失败";
            }
            IsBusy = false;
        }
    }
}
