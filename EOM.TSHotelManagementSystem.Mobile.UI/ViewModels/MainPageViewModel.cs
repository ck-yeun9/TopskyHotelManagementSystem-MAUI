using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private string _activeTab = "checkin";

        public MainPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            NavigateCommand = new Command<string>(NavigateTo);
        }

        public Command<string> NavigateCommand { get; }

        public ObservableCollection<NewsItem> NewsItems { get; } = new()
        {
            new()
            {
                Id = 1,
                Title = "酒店升级会员体系，新老会员尊享专享优惠",
                Date = new DateTime(2023, 10, 15),
                ViewCount = 142,
                ImageUrl = "news_upgrade.png",
                IsHot = true
            },
            new()
            {
                Id = 2,
                Title = "国庆假期预订已开放，提前预订享85折特惠",
                Date = new DateTime(2023, 9, 20),
                ViewCount = 286,
                ImageUrl = "news_national_day.png"
            },
            new()
            {
                Id = 3,
                Title = "我酒店荣获'2023年度最佳服务酒店'称号",
                Date = new DateTime(2023, 9, 5),
                ViewCount = 532,
                ImageUrl = "news_award.png"
            },
            new()
            {
                Id = 4,
                Title = "行政套房全新升级，体验奢华住宿新高度",
                Date = new DateTime(2023, 8, 28),
                ViewCount = 378,
                ImageUrl = "news_suite.jpg"
            },
            new()
            {
                Id = 5,
                Title = "酒店餐厅推出秋季限定菜单，多款特色美食等您品尝",
                Date = new DateTime(2023, 9, 12),
                ViewCount = 215,
                ImageUrl = "news_menu.jpg"
            }
        };

        public string ActiveTab
        {
            get => _activeTab;
            set => SetField(ref _activeTab, value);
        }

        private async void NavigateTo(string route)
        {
            await _navigationService.NavigateToAsync(route);
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
