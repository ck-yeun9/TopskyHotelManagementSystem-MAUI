namespace EOM.TSHotelManagementSystem.Mobile.Contract
{
    public class AvailableRoomDto
    {
        public string RoomTypeId { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string BedType { get; set; } = string.Empty;

        public int MaxGuests { get; set; }

        public bool IncludesBreakfast { get; set; }

        public decimal PricePerNight { get; set; }

        public int RemainingRooms { get; set; }
    }
}
