namespace ScreenTime.Application.Exceptions;

// Validation for status code 409 if bookings collide.
public sealed class SeatsUnavailableException : Exception
{
    public SeatsUnavailableException() 
        : base("Ett eller flera av de valda sätena är inte längre tillgängliga.")
    {
    }
    
}