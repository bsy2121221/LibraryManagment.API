namespace LibraryManagement.Models.Enums
{
    // What the public seat map shows. Occupied covers both held (payment in progress) and booked seats.
    public enum SeatAvailability : byte
    {
        Available = 1,
        Occupied = 2,
        Disabled = 3
    }
}
