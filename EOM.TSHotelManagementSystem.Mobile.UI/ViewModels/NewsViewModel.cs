using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class NewsViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly Random _random = new();
        private bool _isRefreshing;
        private bool _isLoadingMore;
        private bool _isInitializing = false;

        public NewsViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            NavigateCommand = new Command<string>(NavigateTo);

            RefreshCommand = new Command(async () => await RefreshNewsAsync());
            LoadMoreCommand = new Command(async () => await LoadMoreNewsAsync());
        }
        public Command<string> NavigateCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadMoreCommand { get; }

        public NewsViewModel()
        {
            RefreshCommand = new Command(async () => await RefreshNewsAsync());
            LoadMoreCommand = new Command(async () => await LoadMoreNewsAsync());
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetField(ref _isRefreshing, value);
        }

        public bool IsLoadingMore
        {
            get => _isLoadingMore;
            set => SetField(ref _isLoadingMore, value);
        }

        public bool IsInitializing
        {
            get => _isInitializing;
            set => SetField(ref _isInitializing, value);
        }

        public ObservableCollection<NewsItem> NewsItems { get; } = new();

        /// <summary>
        /// 从数据源获取新闻（替换为您的实际数据获取逻辑）
        /// </summary>
        private async Task<List<NewsItem>> FetchNewsAsync(int count = 5)
        {
            await Task.Delay(1000); // 模拟网络延迟

            var newsList = new List<NewsItem>();

            for (int i = 0; i < count; i++)
            {
                var id = newsList.Count + 1;
                newsList.Add(new NewsItem
                {
                    Id = id,
                    Title = $"[新鲜出炉] {_random.Next(1, 100)}号新闻：这是最新刷新的内容",
                    Date = DateTime.Now.AddDays(-_random.Next(0, 30)),
                    ViewCount = _random.Next(50, 1000),
                    ImageUrl = _random.Next(2) == 0 ? "dotnet_bot.png" : "dotnet_bot.jpg",
                    IsHot = _random.Next(3) == 0 // 1/3 概率为热点新闻
                });
            }

            return newsList;
        }

        /// <summary>
        /// 模拟加载新闻数据
        /// </summary>
        public async Task LoadNewsAsync(int count = 5)
        {
            try
            {
                var newItems = await FetchNewsAsync(count);
                foreach (var item in newItems)
                {
                    NewsItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载新闻失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 刷新新闻
        /// </summary>
        private async Task RefreshNewsAsync()
        {
            IsRefreshing = true;

            try
            {
                await LoadNewsAsync(8);
            }
            finally
            {
                IsRefreshing = false;
                IsInitializing = false;
            }
        }

        /// <summary>
        /// 加载更多新闻
        /// </summary>
        public async Task LoadMoreNewsAsync()
        {
            if (IsLoadingMore) return;

            IsLoadingMore = true;

            try
            {
                var newItems = await FetchNewsAsync(5);
                foreach (var item in newItems)
                {
                    NewsItems.Add(item);
                }
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

        private async void NavigateTo(string route)
        {
            _navigationService?.NavigateToAsync(route);
        }
    }

    public class NewsItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public int ViewCount { get; set; }
        public string ImageUrl { get; set; }
        public bool IsHot { get; set; }
    }
}
