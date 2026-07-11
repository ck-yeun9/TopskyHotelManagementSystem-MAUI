using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    /// <summary>
    /// 房间体验评价表单视图模型
    /// </summary>
    public class EvaluationViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly IEvaluationService _evaluationService;
        private string _roomNumber = string.Empty;
        private string _roomName = string.Empty;
        private string _stayId = string.Empty;
        private int _cleanlinessScore;
        private int _serviceScore;
        private int _comfortScore;
        private int _locationScore;
        private string _comment = string.Empty;
        private bool _isAnonymous;
        private bool _isSubmitting;
        private string _submitMessage = string.Empty;
        private ObservableCollection<string> _photoUrls = new();
        private string _newPhotoUrl = string.Empty;

        public EvaluationViewModel(IEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
            SubmitCommand = new Command(async () => await SubmitAsync());
            AddPhotoCommand = new Command(AddPhoto);
            RemovePhotoCommand = new Command<string>(RemovePhoto);
        }

        public ICommand SubmitCommand { get; }
        public ICommand AddPhotoCommand { get; }
        public ICommand RemovePhotoCommand { get; }

        public string RoomNumber
        {
            get => _roomNumber;
            set => SetField(ref _roomNumber, value);
        }

        public string RoomName
        {
            get => _roomName;
            set => SetField(ref _roomName, value);
        }

        public string StayId
        {
            get => _stayId;
            set => SetField(ref _stayId, value);
        }

        public int CleanlinessScore
        {
            get => _cleanlinessScore;
            set => SetField(ref _cleanlinessScore, value);
        }

        public int ServiceScore
        {
            get => _serviceScore;
            set => SetField(ref _serviceScore, value);
        }

        public int ComfortScore
        {
            get => _comfortScore;
            set => SetField(ref _comfortScore, value);
        }

        public int LocationScore
        {
            get => _locationScore;
            set => SetField(ref _locationScore, value);
        }

        public string Comment
        {
            get => _comment;
            set => SetField(ref _comment, value);
        }

        public bool IsAnonymous
        {
            get => _isAnonymous;
            set => SetField(ref _isAnonymous, value);
        }

        public bool IsSubmitting
        {
            get => _isSubmitting;
            set
            {
                if (SetField(ref _isSubmitting, value))
                    OnPropertyChanged(nameof(CanSubmit));
            }
        }

        public bool CanSubmit => !IsSubmitting;

        public string SubmitMessage
        {
            get => _submitMessage;
            set => SetField(ref _submitMessage, value);
        }

        public ObservableCollection<string> PhotoUrls
        {
            get => _photoUrls;
            set => SetField(ref _photoUrls, value);
        }

        public string NewPhotoUrl
        {
            get => _newPhotoUrl;
            set => SetField(ref _newPhotoUrl, value);
        }

        public void Init(string roomNumber, string roomName, string stayId)
        {
            RoomNumber = roomNumber;
            RoomName = roomName;
            StayId = stayId;
        }

        public void OnViewAppearing()
        {
        }

        public void OnViewDisappearing()
        {
        }

        private void AddPhoto()
        {
            if (string.IsNullOrWhiteSpace(NewPhotoUrl))
                return;

            PhotoUrls.Add(NewPhotoUrl.Trim());
            NewPhotoUrl = string.Empty;
        }

        private void RemovePhoto(string url)
        {
            if (!string.IsNullOrEmpty(url))
                PhotoUrls.Remove(url);
        }

        private async Task SubmitAsync()
        {
            // 防止重复点击：已在提交中则直接返回
            if (IsSubmitting)
                return;

            if (CleanlinessScore == 0 || ServiceScore == 0 || ComfortScore == 0 || LocationScore == 0)
            {
                SubmitMessage = "请为四个维度都打分（1-5 星）。";
                return;
            }

            IsSubmitting = true;
            SubmitMessage = string.Empty;

            bool ok = false;
            string errorMessage = string.Empty;
            try
            {
                var input = new CreateEvaluationInputDto
                {
                    RoomNumber = RoomNumber,
                    RoomTypeId = 0,
                    IsAnonymous = IsAnonymous,
                    StayId = StayId,
                    CleanlinessScore = CleanlinessScore,
                    ServiceScore = ServiceScore,
                    ComfortScore = ComfortScore,
                    LocationScore = LocationScore,
                    Comment = Comment,
                    PhotoUrls = string.Join(",", PhotoUrls)
                };

                (ok, errorMessage) = await _evaluationService.SubmitEvaluationAsync(input);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SubmitEvaluation Exception: {ex.Message}");
                ok = false;
            }

            if (ok)
            {
                // 必须在 PopAsync 之前把 IsSubmitting 复位：
                // 页面出栈后按钮的 Handler/PlatformView 已被销毁，此刻再改动 CanSubmit
                // 会触发 Material 按钮的 VisualState 切换 -> MapBackground 访问空 PlatformView
                // -> 在 Android 上抛 "PlatformView cannot be null here" 致 App 崩溃。
                IsSubmitting = false;
                SubmitMessage = "评价提交成功，感谢您的反馈！";
                await Shell.Current.Navigation.PopAsync();
                return;
            }

            // 失败路径：页面仍在导航栈中，可安全复位
            IsSubmitting = false;
            SubmitMessage = string.IsNullOrWhiteSpace(errorMessage)
                ? "评价提交失败，请稍后重试。"
                : errorMessage;
        }
    }
}
