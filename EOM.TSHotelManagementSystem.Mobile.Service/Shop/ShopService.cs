using EOM.TSHotelManagementSystem.Mobile.Contract;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public class ShopService : IShopService
    {
        private readonly IHttpService _httpService;

        public ShopService(IHttpService httpService)
        {
            _httpService = httpService;
        }

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var response = await _httpService.RequestAsync("MobileBooking/GetProductCategories");
            if (string.IsNullOrWhiteSpace(response?.Message))
                return new List<CategoryDto>();

            System.Diagnostics.Debug.WriteLine($"GetProductCategories raw: {response.Message}");

            var result = HttpHelper.JsonToModel<ApiResponse<PagedData<System.Text.Json.JsonElement>>>(response.Message);
            if (result?.Code == 0 && result.Data?.Items != null)
            {
                var categories = new List<CategoryDto>();
                foreach (var item in result.Data.Items)
                {
                    System.Diagnostics.Debug.WriteLine($"Category item ValueKind={item.ValueKind}, Raw={item}");
                    if (item.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        var code = TryGetPropertyString(item, "Code") ?? TryGetPropertyString(item, "code") ?? "";
                        var label = TryGetPropertyString(item, "Label") ?? TryGetPropertyString(item, "label") ?? code;
                        System.Diagnostics.Debug.WriteLine($"Parsed category: Code={code}, Label={label}");
                        categories.Add(new CategoryDto { Code = code, Label = label });
                    }
                    else
                    {
                        var raw = item.GetString() ?? "";
                        categories.Add(new CategoryDto { Code = raw, Label = raw });
                    }
                }
                return categories;
            }

            return new List<CategoryDto>();
        }

        public async Task<List<ProductDto>> GetProductsAsync(string categoryCode = null)
        {
            System.Diagnostics.Debug.WriteLine($"GetProductsAsync called with categoryCode='{categoryCode}'");
            var url = string.IsNullOrEmpty(categoryCode)
                ? "MobileBooking/GetProducts"
                : $"MobileBooking/GetProducts?category={Uri.EscapeDataString(categoryCode)}";
            System.Diagnostics.Debug.WriteLine($"GetProductsAsync url='{url}'");

            var response = await _httpService.RequestAsync(url);
            if (string.IsNullOrWhiteSpace(response?.Message))
                return new List<ProductDto>();

            var result = HttpHelper.JsonToModel<ApiResponse<PagedData<ProductDto>>>(response.Message);
            if (result?.Code == 0 && result.Data?.Items != null)
                return result.Data.Items;

            return new List<ProductDto>();
        }

        private static string? TryGetPropertyString(System.Text.Json.JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var prop) && prop.ValueKind == System.Text.Json.JsonValueKind.String
                ? prop.GetString()
                : null;
        }

        public async Task<bool> PlaceOrderAsync(PlaceOrderInputDto input)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(input);
            var response = await _httpService.RequestAsync("MobileBooking/PlaceOrder", json);
            if (string.IsNullOrWhiteSpace(response?.Message))
                return false;

            var result = HttpHelper.JsonToModel<ApiResponse<object>>(response.Message);
            return result?.Code == 0;
        }
    }
}
