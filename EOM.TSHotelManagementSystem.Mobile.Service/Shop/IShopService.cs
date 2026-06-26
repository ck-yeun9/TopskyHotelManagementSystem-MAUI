using EOM.TSHotelManagementSystem.Mobile.Contract;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface IShopService
    {
        Task<List<CategoryDto>> GetCategoriesAsync();
        Task<List<ProductDto>> GetProductsAsync(string categoryCode = null);
        Task<bool> PlaceOrderAsync(PlaceOrderInputDto input);
    }
}
