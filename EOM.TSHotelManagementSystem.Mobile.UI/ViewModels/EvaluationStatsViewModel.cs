using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    /// <summary>
    /// 房间评价统计与列表视图模型（支持懒加载分页）
    /// </summary>
    public class EvaluationStatsViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly IEvaluationService _evaluationService;
        private string _roomNumber = string.Empty;
        private string _roomName = string.Empty;
        private string _stayId = string.Empty;
        private bool _isLoading;
        private string _loadedRoomNumber = string.Empty;
        private bool _hasLoaded;
        private RoomEvaluationStatisticsOutputDto _statistics = new();
        private ObservableCollection<ReadRoomEvaluationOutputDto> _recentComments = new();
        private ObservableCollection<DistributionItem> _distributionItems = new();

        // 懒加载分页状态
        private int _pageNumber = 1;
        private bool _hasMorePages = true;
        private bool _isLoadingMore;
        private const int PageSize = 10;

        public EvaluationStatsViewModel(IEvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
            WriteReviewCommand = new Command(async () => await OnWriteReviewAsync());
            RefreshCommand = new Command(async () => await LoadAsync(force: true));
            LoadMoreCommand = new Command(async () => await OnLoadMoreAsync());
        }

        public ICommand WriteReviewCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadMoreCommand { get; }

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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetField(ref _isLoading, value);
        }

        public bool IsLoadingMore
        {
            get => _isLoadingMore;
            set => SetField(ref _isLoadingMore, value);
        }

        public bool HasMorePages
        {
            get => _hasMorePages;
            set => SetField(ref _hasMorePages, value);
        }

        public RoomEvaluationStatisticsOutputDto Statistics
        {
            get => _statistics;
            set
            {
                if (SetField(ref _statistics, value))
                {
                    OnPropertyChanged(nameof(HasData));
                    OnPropertyChanged(nameof(AvgOverallStars));
                    OnPropertyChanged(nameof(AvgCleanlinessStars));
                    OnPropertyChanged(nameof(AvgServiceStars));
                    OnPropertyChanged(nameof(AvgComfortStars));
                    OnPropertyChanged(nameof(AvgLocationStars));
                    OnPropertyChanged(nameof(CommentCountLabel));
                }
            }
        }

        /// <summary>
        /// 最近点评标题：显示总数
        /// </summary>
        public string CommentCountLabel
            => Statistics == null || Statistics.TotalCount <= 0
                ? "最近点评"
                : $"最近点评（共 {Statistics.TotalCount} 条）";

        public int AvgOverallStars => Statistics == null ? 0 : (int)Math.Round(Statistics.AvgOverall);
        public int AvgCleanlinessStars => Statistics == null ? 0 : (int)Math.Round(Statistics.AvgCleanliness);
        public int AvgServiceStars => Statistics == null ? 0 : (int)Math.Round(Statistics.AvgService);
        public int AvgComfortStars => Statistics == null ? 0 : (int)Math.Round(Statistics.AvgComfort);
        public int AvgLocationStars => Statistics == null ? 0 : (int)Math.Round(Statistics.AvgLocation);

        public ObservableCollection<ReadRoomEvaluationOutputDto> RecentComments
        {
            get => _recentComments;
            set => SetField(ref _recentComments, value);
        }

        public ObservableCollection<DistributionItem> DistributionItems
        {
            get => _distributionItems;
            set => SetField(ref _distributionItems, value);
        }

        public bool HasData => Statistics != null && Statistics.TotalCount > 0;

        public void Init(string roomNumber, string roomName, string stayId)
        {
            RoomNumber = roomNumber;
            RoomName = roomName;
            StayId = stayId;
        }

        public async Task LoadAsync(bool force = false)
        {
            if (string.IsNullOrEmpty(RoomNumber))
                return;

            // 同一房间且已加载过：直接复用缓存，避免每次进入都重新请求造成的顿感
            if (!force && _hasLoaded && _loadedRoomNumber == RoomNumber)
                return;

            try
            {
                IsLoading = true;

                // 加载统计数据
                var stats = await _evaluationService.GetStatisticsAsync(RoomNumber);
                Statistics = stats ?? new RoomEvaluationStatisticsOutputDto();
                OnPropertyChanged(nameof(HasData));

                // 加载评分分布
                var max = Statistics.Distribution.Values.DefaultIfEmpty(0).Max();
                var items = new[] { 5, 4, 3, 2, 1 }.Select(star => new DistributionItem
                {
                    Star = star,
                    Count = Statistics.Distribution.TryGetValue(star, out var c) ? c : 0
                }).ToList();
                foreach (var it in items)
                {
                    it.Percent = max <= 0 ? 0 : (double)it.Count / max;
                    it.Width = max <= 0 ? 0 : (int)(it.Percent * 200);
                }
                DistributionItems = new ObservableCollection<DistributionItem>(items);

                // 初始评论来自 GetStatistics（保证兼容），超过10条的才需要懒加载翻页
                var initialComments = (stats?.RecentComments ?? new List<ReadRoomEvaluationOutputDto>())
                    .Select(e =>
                    {
                        if (e.IsAnonymous)
                            e.UserName = "匿名用户";
                        else if (!string.IsNullOrWhiteSpace(e.CustomerName))
                            e.UserName = e.CustomerName;
                        else if (!string.IsNullOrWhiteSpace(e.CustomerNumber))
                            e.UserName = e.CustomerNumber;
                        else
                            e.UserName = "住客";
                        return e;
                    }).ToList();
                RecentComments = new ObservableCollection<ReadRoomEvaluationOutputDto>(initialComments);
                _pageNumber = 1;
                _hasMorePages = initialComments.Count >= PageSize && Statistics.TotalCount > initialComments.Count;

                _loadedRoomNumber = RoomNumber;
                _hasLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EvaluationStats LoadAsync Exception: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 加载更多（下一页）
        /// </summary>
        private async Task OnLoadMoreAsync()
        {
            if (_isLoadingMore || !_hasMorePages || string.IsNullOrEmpty(RoomNumber))
                return;

            try
            {
                _isLoadingMore = true;
                _pageNumber++;
                await LoadPageAsync(_pageNumber, replace: false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EvaluationStats LoadMore Exception: {ex}");
                _pageNumber--; // 回滚页码
            }
            finally
            {
                _isLoadingMore = false;
            }
        }

        /// <summary>
        /// 加载指定页评论，replace=true 替换列表，false 追加
        /// </summary>
        private async Task LoadPageAsync(int page, bool replace)
        {
            var result = await _evaluationService.GetRoomEvaluationsAsync(RoomNumber, page, PageSize);
            if (result == null || result.Items == null || result.Items.Count == 0)
            {
                _hasMorePages = false;
                if (replace)
                    RecentComments = new ObservableCollection<ReadRoomEvaluationOutputDto>();
                return;
            }

            // 处理展示名
            foreach (var e in result.Items)
            {
                if (e.IsAnonymous)
                    e.UserName = "匿名用户";
                else if (!string.IsNullOrWhiteSpace(e.CustomerName))
                    e.UserName = e.CustomerName;
                else if (!string.IsNullOrWhiteSpace(e.CustomerNumber))
                    e.UserName = e.CustomerNumber;
                else
                    e.UserName = "住客";
            }

            if (replace)
            {
                RecentComments = new ObservableCollection<ReadRoomEvaluationOutputDto>(result.Items);
            }
            else
            {
                foreach (var item in result.Items)
                {
                    RecentComments.Add(item);
                }
            }

            _hasMorePages = result.Items.Count >= PageSize;
        }

        public void OnViewAppearing() => _ = LoadAsync();

        public void OnViewDisappearing()
        {
        }

        private async Task OnWriteReviewAsync()
        {
            if (string.IsNullOrEmpty(RoomNumber))
                return;

            try
            {
                var page = MauiProgram.Services.GetRequiredService<EvaluationView>();
                page.Initialize(RoomNumber, RoomName, StayId);
                await Shell.Current.Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnWriteReviewAsync Exception: {ex}");
            }
        }
    }

    /// <summary>
    /// 评分分布条目（用于条形图展示）
    /// </summary>
    public class DistributionItem
    {
        public int Star { get; set; }
        public int Count { get; set; }
        public double Percent { get; set; }
        public int Width { get; set; }
    }
}
