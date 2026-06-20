using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class NewsViewModel : ViewModelBase, ILoadableViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly INewsService _newsService;
        private bool _isRefreshing;
        private bool _isLoadingMore;
        private bool _isInitializing = false;
        private int _currentPage = 1;
        private const int PageSize = 10;

        public NewsViewModel(INavigationService navigationService, INewsService newsService)
        {
            _navigationService = navigationService;
            _newsService = newsService;
            NavigateCommand = new Command<string>(NavigateTo);

            RefreshCommand = new Command(async () => await RefreshNewsAsync());
            LoadMoreCommand = new Command(async () => await LoadMoreNewsAsync());
        }

        public string _pageTitle;
        public string PageTitle
        {
            get => _pageTitle;
            set => SetField(ref _pageTitle, value);
        }

        public async void OnViewAppearing()
        {
            try
            {
                IsInitializing = true;
                if (NewsItems.Count == 0)
                {
                    await LoadNewsAsync();
                }
            }
            finally
            {
                IsInitializing = false;
            }
        }

        public Command<string> NavigateCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand LoadMoreCommand { get; }

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

        public ObservableCollection<NewsItem> NewsItems { get; set; } = new();

        public async Task LoadNewsAsync()
        {
            try
            {
                _currentPage = 1;
                var newsList = await _newsService.GetNewsAsync(_currentPage, PageSize);
                NewsItems.Clear();
                foreach (var item in newsList.Select(MapToNewsItem))
                {
                    NewsItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载新闻失败: {ex.Message}");
            }
        }

        public async Task LoadMoreNewsAsync()
        {
            if (IsLoadingMore) return;

            IsLoadingMore = true;

            try
            {
                _currentPage++;
                var newsList = await _newsService.GetNewsAsync(_currentPage, PageSize);
                foreach (var item in newsList.Select(MapToNewsItem))
                {
                    NewsItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载更多新闻失败: {ex.Message}");
                _currentPage--;
            }
            finally
            {
                IsLoadingMore = false;
            }
        }

        private async Task RefreshNewsAsync()
        {
            IsRefreshing = true;
            try
            {
                await LoadNewsAsync();
            }
            finally
            {
                IsRefreshing = false;
                IsInitializing = false;
            }
        }

        private NewsItem MapToNewsItem(ReadNewsOutputDto dto)
        {
            return new NewsItem
            {
                Id = dto.NewId,
                Title = dto.NewsTitle,
                Content = dto.NewsContent,
                Date = dto.NewsDate,
                Type = dto.NewsTypeDescription ?? dto.NewsType,
                ImageUrl = dto.NewsImage,
                Status = dto.NewsStatusDescription ?? dto.NewsStatus,
                IsHot = dto.NewsType?.Contains("热点") == true
            };
        }

        private async void NavigateTo(string route)
        {
            _navigationService?.NavigateToAsync(route);
        }

        public void OnViewDisappearing()
        {
        }
    }

    public class NewsItem
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public bool IsHot { get; set; }
    }
}
