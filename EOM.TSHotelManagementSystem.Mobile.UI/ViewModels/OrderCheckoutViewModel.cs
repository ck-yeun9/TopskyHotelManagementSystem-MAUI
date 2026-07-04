using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class OrderCheckoutViewModel : ViewModelBase
    {
        private readonly IShopService _shopService;
        private string _roomNumber;
        private string _deliveryMethod = "RoomDelivery";
        private DateTime _deliveryDate = DateTime.Today;
        private TimeSpan _deliveryTime = DateTime.Now.TimeOfDay;
        private string _paymentMethod = "CheckoutSettle";
        private bool _isSubmitting;

        public OrderCheckoutViewModel(IShopService shopService)
        {
            _shopService = shopService;

            OrderItems = new ObservableCollection<CartItemDto>();
            SelectDeliveryCommand = new Command<string>(method => DeliveryMethod = method);
            SelectPaymentCommand = new Command<string>(method => PaymentMethod = method);
            PlaceOrderCommand = new Command(async () => await PlaceOrderAsync());
        }

        public ObservableCollection<CartItemDto> OrderItems { get; }

        public ICommand SelectDeliveryCommand { get; }
        public ICommand SelectPaymentCommand { get; }
        public ICommand PlaceOrderCommand { get; }

        public string RoomNumber
        {
            get => _roomNumber;
            set => SetField(ref _roomNumber, value);
        }

        public string DeliveryMethod
        {
            get => _deliveryMethod;
            set
            {
                if (SetField(ref _deliveryMethod, value))
                {
                    OnPropertyChanged(nameof(IsRoomDelivery));
                    OnPropertyChanged(nameof(IsSelfPickup));
                }
            }
        }

        public bool IsRoomDelivery => DeliveryMethod == "RoomDelivery";
        public bool IsSelfPickup => DeliveryMethod == "SelfPickup";

        public DateTime DeliveryDate
        {
            get => _deliveryDate;
            set => SetField(ref _deliveryDate, value);
        }

        public TimeSpan DeliveryTime
        {
            get => _deliveryTime;
            set => SetField(ref _deliveryTime, value);
        }

        public string PaymentMethod
        {
            get => _paymentMethod;
            set => SetField(ref _paymentMethod, value);
        }

        public bool IsSubmitting
        {
            get => _isSubmitting;
            set => SetField(ref _isSubmitting, value);
        }

        public decimal TotalAmount => OrderItems.Sum(i => i.Subtotal);

        public void Initialize(string roomNumber, List<CartItemDto> items)
        {
            RoomNumber = roomNumber;
            OrderItems.Clear();
            foreach (var item in items) OrderItems.Add(item);
            OnPropertyChanged(nameof(TotalAmount));
        }

        private async Task PlaceOrderAsync()
        {
            try
            {
                IsSubmitting = true;

                var input = new PlaceOrderInputDto
                {
                    RoomNumber = RoomNumber,
                    DeliveryMethod = DeliveryMethod,
                    DeliveryTime = DeliveryDate.Add(DeliveryTime),
                    Items = OrderItems.Select(i => new OrderItemDto
                    {
                        ProductNumber = i.ProductNumber,
                        ProductName = i.ProductName,
                        ProductPrice = i.ProductPrice,
                        Quantity = i.Quantity
                    }).ToList()
                };

                var success = await _shopService.PlaceOrderAsync(input);
                if (success)
                {
                    ProductShopViewModel.NeedsReload = true;
                    await Shell.Current.DisplayAlertAsync("成功", "下单成功！", "确定");
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync("失败", "下单失败，请重试。", "确定");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("错误", $"下单异常: {ex.Message}", "确定");
            }
            finally
            {
                IsSubmitting = false;
            }
        }
    }
}
