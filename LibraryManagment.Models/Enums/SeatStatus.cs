namespace LibraryManagement.Models.Enums
{
    // Admin-controlled only. Occupancy comes from Bookings, not from this status.
    public enum SeatStatus : byte
    {
        Active = 1,
        Disabled = 2
    }
}
