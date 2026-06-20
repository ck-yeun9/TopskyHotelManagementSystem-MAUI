namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public int Code { get; set; }
        public string Message { get; set; }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }
    }

    public class PagedData<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
