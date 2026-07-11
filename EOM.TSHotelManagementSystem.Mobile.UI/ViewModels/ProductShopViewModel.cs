using EOM.TSHotelManagementSystem.Mobile.Contract;
using EOM.TSHotelManagementSystem.Mobile.Service;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace EOM.TSHotelManagementSystem.Mobile.UI
{
    public class ProductShopViewModel : ViewModelBase
    {
        public static bool NeedsReload { get; set; }
        private readonly IShopService _shopService;
        private string _roomNumber;
        private string _currentRoomNumber;
        private string _selectedCategory;
        private bool _isLoading;
        private bool _isCartOpen;

        public ProductShopViewModel(IShopService shopService)
        {
            _shopService = shopService;

            Categories = new ObservableCollection<CategoryDto>();
            Products = new ObservableCollection<ProductDto>();
            CartItems = new ObservableCollection<CartItemDto>();

            SelectCategoryCommand = new Command<CategoryDto>(async (cat) => await SelectCategoryAsync(cat));
            AddToCartCommand = new Command<ProductDto>(AddToCart);
            RemoveFromCartCommand = new Command<ProductDto>(RemoveFromCart);
            AddCartItemCommand = new Command<CartItemDto>(AddCartItem);
            RemoveCartItemCommand = new Command<CartItemDto>(RemoveCartItem);
            ClearCartCommand = new Command(ClearCart);
            ToggleCartCommand = new Command(() => IsCartOpen = !IsCartOpen);
            CloseCartCommand = new Command(() => IsCartOpen = false);
            CheckoutCommand = new Command(async () => await CheckoutAsync());
            SetCartQuantityCommand = new Command<ProductDto>(async (p) => await SetCartQuantityAsync(p));
        }

        public ObservableCollection<CategoryDto> Categories { get; }
        public ObservableCollection<ProductDto> Products { get; }
        public ObservableCollection<CartItemDto> CartItems { get; }

        public ICommand SelectCategoryCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand AddCartItemCommand { get; }
        public ICommand RemoveCartItemCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand ToggleCartCommand { get; }
        public ICommand CloseCartCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand SetCartQuantityCommand { get; }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetField(ref _selectedCategory, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetField(ref _isLoading, value);
        }

        public bool IsCartOpen
        {
            get => _isCartOpen;
            set => SetField(ref _isCartOpen, value);
        }

        public decimal CartTotal => CartItems.Sum(i => i.Subtotal);
        public int CartItemCount => CartItems.Sum(i => i.Quantity);
        public bool HasCartItems => CartItemCount > 0;
        public Color CartIconColor => HasCartItems ? Color.FromArgb("#E4393C") : Color.FromArgb("#333333");

        public void Initialize(string roomNumber)
        {
            // ViewModel 为单例，购物车数据跨房间会残留。
            // 切换房间时清空上一次购物车，避免把 A 房间的商品结算到 B 房间。
            if (!string.Equals(_currentRoomNumber, roomNumber, StringComparison.Ordinal))
            {
                ClearCart();
                _currentRoomNumber = roomNumber;
            }
            _roomNumber = roomNumber;
        }

        private void ResetState()
        {
            Categories.Clear();
            Products.Clear();
            CartItems.Clear();
            NotifyCartChanged();
        }

        public async Task LoadDataAsync()
        {
            if (NeedsReload)
            {
                NeedsReload = false;
                ResetState();
            }

            if (Categories.Count > 0) return;

            try
            {
                IsLoading = true;

                var categoryList = await _shopService.GetCategoriesAsync();
                Categories.Clear();
                Categories.Add(new CategoryDto { Code = null, Label = "全部", IsSelected = true });
                foreach (var c in categoryList) Categories.Add(c);

                SelectedCategory = "全部";
                await LoadProductsAsync(null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDataAsync Exception: {ex}");
                await Shell.Current.DisplayAlertAsync("加载失败", ex.ToString(), "确定");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SelectCategoryAsync(CategoryDto category)
        {
            if (category == null) return;

            foreach (var c in Categories) c.IsSelected = false;
            category.IsSelected = true;

            SelectedCategory = category.Label;
            await LoadProductsAsync(category.Code);
        }

        private async Task LoadProductsAsync(string category)
        {
            try
            {
                IsLoading = true;
                var products = await _shopService.GetProductsAsync(category);

                Products.Clear();
                foreach (var p in products)
                {
                    p.OriginalStock = p.Stock;
                    var cartItem = CartItems.FirstOrDefault(c => c.ProductNumber == p.ProductNumber);
                    p.CartQuantity = cartItem?.Quantity ?? 0;
                    Products.Add(p);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadProductsAsync Exception: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddToCart(ProductDto product)
        {
            if (product == null || product.AvailableStock <= 0) return;

            var existing = CartItems.FirstOrDefault(i => i.ProductNumber == product.ProductNumber);
            if (existing != null)
            {
                existing.Quantity++;
            }
            else
            {
                CartItems.Add(new CartItemDto
                {
                    ProductNumber = product.ProductNumber,
                    ProductName = product.ProductName,
                    ProductPrice = product.ProductPrice,
                    Specification = product.Specification,
                    Quantity = 1
                });
            }

            product.CartQuantity = (CartItems.FirstOrDefault(i => i.ProductNumber == product.ProductNumber)?.Quantity) ?? 0;
            NotifyCartChanged();
        }

        private void RemoveFromCart(ProductDto product)
        {
            if (product == null) return;

            var existing = CartItems.FirstOrDefault(i => i.ProductNumber == product.ProductNumber);
            if (existing == null) return;

            if (existing.Quantity > 1)
            {
                existing.Quantity--;
            }
            else
            {
                CartItems.Remove(existing);
            }

            product.CartQuantity = (CartItems.FirstOrDefault(i => i.ProductNumber == product.ProductNumber)?.Quantity) ?? 0;
            NotifyCartChanged();
        }

        private void AddCartItem(CartItemDto item)
        {
            if (item == null) return;
            var product = Products.FirstOrDefault(p => p.ProductNumber == item.ProductNumber);
            if (product != null && product.AvailableStock <= 0) return;

            item.Quantity++;
            if (product != null) product.CartQuantity = item.Quantity;
            NotifyCartChanged();
        }

        private void RemoveCartItem(CartItemDto item)
        {
            if (item == null) return;
            if (item.Quantity > 1)
            {
                item.Quantity--;
                var product = Products.FirstOrDefault(p => p.ProductNumber == item.ProductNumber);
                if (product != null) product.CartQuantity = item.Quantity;
            }
            else
            {
                CartItems.Remove(item);
                var product = Products.FirstOrDefault(p => p.ProductNumber == item.ProductNumber);
                if (product != null) product.CartQuantity = 0;
            }
            NotifyCartChanged();
        }

        private async Task SetCartQuantityAsync(ProductDto product)
        {
            if (product == null) return;

            var result = await Shell.Current.DisplayPromptAsync(
                "设置数量", $"购买 {product.ProductName} 的数量",
                keyboard: Keyboard.Numeric,
                initialValue: product.CartQuantity.ToString(),
                placeholder: "请输入数量");

            if (result == null) return;

            if (!int.TryParse(result, out var qty) || qty < 0)
            {
                await Shell.Current.DisplayAlertAsync("提示", "请输入有效的数量", "确定");
                return;
            }

            if (qty > product.OriginalStock)
            {
                qty = product.OriginalStock;
            }

            var existing = CartItems.FirstOrDefault(i => i.ProductNumber == product.ProductNumber);

            if (qty == 0)
            {
                if (existing != null) CartItems.Remove(existing);
                product.CartQuantity = 0;
            }
            else
            {
                if (existing != null)
                {
                    existing.Quantity = qty;
                }
                else
                {
                    CartItems.Add(new CartItemDto
                    {
                        ProductNumber = product.ProductNumber,
                        ProductName = product.ProductName,
                        ProductPrice = product.ProductPrice,
                        Specification = product.Specification,
                        Quantity = qty
                    });
                }
                product.CartQuantity = qty;
            }

            NotifyCartChanged();
        }

        private void ClearCart()
        {
            foreach (var p in Products) p.CartQuantity = 0;
            CartItems.Clear();
            NotifyCartChanged();
        }

        private void NotifyCartChanged()
        {
            OnPropertyChanged(nameof(CartTotal));
            OnPropertyChanged(nameof(CartItemCount));
            OnPropertyChanged(nameof(HasCartItems));
            OnPropertyChanged(nameof(CartIconColor));
        }

        private async Task CheckoutAsync()
        {
            if (CartItems.Count == 0)
            {
                await Shell.Current.DisplayAlertAsync("提示", "购物车为空，请先添加商品。", "确定");
                return;
            }

            var checkoutVm = MauiProgram.Services.GetService<OrderCheckoutViewModel>();
            checkoutVm.Initialize(_roomNumber, new List<CartItemDto>(CartItems));
            await Application.Current.Windows[0].Page.Navigation.PushAsync(new OrderCheckoutView(checkoutVm));
        }
    }
}
