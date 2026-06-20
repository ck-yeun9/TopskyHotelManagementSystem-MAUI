namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class NewsDetailViewModel : ViewModelBase
    {
        private string _title;
        private string _content;
        private DateTime _date;
        private string _type;
        private string _status;
        private string _imageUrl;

        public string Title
        {
            get => _title;
            set => SetField(ref _title, value);
        }

        public string Content
        {
            get => _content;
            set => SetField(ref _content, value);
        }

        public DateTime Date
        {
            get => _date;
            set => SetField(ref _date, value);
        }

        public string Type
        {
            get => _type;
            set => SetField(ref _type, value);
        }

        public string Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetField(ref _imageUrl, value);
        }

        public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);

        public void LoadNewsItem(NewsItem item)
        {
            if (item == null) return;
            Title = item.Title;
            Content = item.Content;
            Date = item.Date;
            Type = item.Type;
            Status = item.Status;
            ImageUrl = item.ImageUrl;
        }
    }
}
