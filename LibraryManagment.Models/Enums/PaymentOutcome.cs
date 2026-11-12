namespace LibraryManagement.Models.Enums
{
    // Result of usp_Booking_ConfirmPayment
    public enum PaymentOutcome : byte
    {
        Confirmed = 1,
        AlreadyProcessed = 2,

        // Paid after the hold ended and someone else took the seat: refund needed
        SeatLost = 3
    }
}
