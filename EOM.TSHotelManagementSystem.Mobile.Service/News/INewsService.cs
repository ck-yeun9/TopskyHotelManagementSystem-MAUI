using EOM.TSHotelManagementSystem.Mobile.Contract;

namespace EOM.TSHotelManagementSystem.Mobile.Service
{
    public interface INewsService
    {
        Task<List<ReadNewsOutputDto>> GetNewsAsync(int page = 1, int pageSize = 10);
        Task<ReadNewsOutputDto> GetNewsDetailAsync(string newsId);
    }
}
