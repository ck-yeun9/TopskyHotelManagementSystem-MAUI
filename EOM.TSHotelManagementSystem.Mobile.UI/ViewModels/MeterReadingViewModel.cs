using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class MeterReadingViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly IStaffService _staffService;

        public MeterReadingViewModel(IStaffService staffService)
        {
            _staffService = staffService;

            ReadingList = new ObservableCollection<MeterReadingOutputDto>();
            SubmitReadingCommand = new Command(async () => await SubmitReadingAsync());
            RefreshCommand = new Command(async () => await LoadReadingListAsync());
        }

        public ObservableCollection<MeterReadingOutputDto> ReadingList { get; }

        private int _selectedFloorId;
        public int SelectedFloorId
        {
            get => _selectedFloorId;
            set
            {
                if (SetField(ref _selectedFloorId, value))
                {
                    _ = LoadReadingListAsync();
                }
            }
        }

        private int _selectedRoomId;
        public int SelectedRoomId
        {
            get => _selectedRoomId;
            set => SetField(ref _selectedRoomId, value);
        }

        private decimal _waterReading;
        public decimal WaterReading
        {
            get => _waterReading;
            set => SetField(ref _waterReading, value);
        }

        private decimal _electricReading;
        public decimal ElectricReading
        {
            get => _electricReading;
            set => SetField(ref _electricReading, value);
        }

        private string _readingNote;
        public string ReadingNote
        {
            get => _readingNote;
            set => SetField(ref _readingNote, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetField(ref _successMessage, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetField(ref _isBusy, value);
        }

        public ICommand SubmitReadingCommand { get; }
        public ICommand RefreshCommand { get; }

        public void OnViewAppearing()
        {
            _ = LoadReadingListAsync();
        }

        public void OnViewDisappearing()
        {
        }

        private async Task LoadReadingListAsync()
        {
            IsBusy = true;
            var result = await _staffService.GetMeterReadingListAsync(_selectedFloorId);

            if (result.StatusCode == StatusCodeConstants.Success && result.listSource != null)
            {
                ReadingList.Clear();
                foreach (var item in result.listSource)
                {
                    ReadingList.Add(item);
                }
                ErrorMessage = string.Empty;
            }
            else
            {
                ErrorMessage = result.Message ?? "加载抄表记录失败";
            }
            IsBusy = false;
        }

        private async Task SubmitReadingAsync()
        {
            if (_selectedRoomId <= 0)
            {
                ErrorMessage = "请选择房间";
                return;
            }

            var input = new MeterReadingInputDto
            {
                RoomId = _selectedRoomId,
                FloorId = _selectedFloorId,
                WaterReading = _waterReading,
                ElectricReading = _electricReading,
                ReadingNote = _readingNote
            };

            IsBusy = true;
            var result = await _staffService.AddMeterReadingAsync(input);

            if (result.StatusCode == StatusCodeConstants.Success)
            {
                SuccessMessage = "抄表记录已提交";
                WaterReading = 0;
                ElectricReading = 0;
                ReadingNote = string.Empty;
                await LoadReadingListAsync();
            }
            else
            {
                ErrorMessage = result.Message ?? "提交抄表记录失败";
            }
            IsBusy = false;
        }
    }
}
