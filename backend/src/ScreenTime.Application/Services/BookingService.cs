using System.Security.Cryptography;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Exceptions;
using ScreenTime.Application.Interfaces;
using ScreenTime.Domain.Entities;
using ScreenTime.Domain.Enums;

namespace ScreenTime.Application.Services;

// Handles the logic for creating bookings
public sealed class BookingService : IBookingService
{
    // Characters used to generate the booking reference
    private const string BookingReferenceCharacters =
        "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    // Repository used to get and save booking data
    private readonly IBookingRepository _bookingRepository;

    // Gets the booking repository through dependency injection
    public BookingService(IBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    // Creates a new booking and returns a booking confirmation
    public async Task<BookingConfirmationDto> CreateBookingAsync(
        Guid userId,
        CreateBookingDto request,
        CancellationToken cancellationToken = default)
    {
        // Validates the user ID and booking request
        ValidateRequest(userId, request);

        // Removes duplicate seat IDs
        var seatIds = request.SeatIds
            .Distinct()
            .ToArray();

        // Prevents the same seat from being selected more than once
        if (seatIds.Length != request.SeatIds.Count)
        {
            throw new ArgumentException(
                "Samma säte får inte väljas flera gånger.",
                nameof(request));
        }

        // Gets the selected screening from the database
        var screening = await _bookingRepository.GetScreeningAsync(
            request.ScreeningId,
            cancellationToken);

        // Checks if the screening exists
        if (screening is null)
        {
            throw new KeyNotFoundException("Visningen kunde inte hittas.");
        }

        // Prevents booking a cancelled screening
        if (screening.IsCancelled)
        {
            throw new InvalidOperationException("Visningen är avbokad.");
        }

        // Prevents booking a screening that has already started
        if (screening.StartTime <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "Det går inte att boka en visning som redan har börjat.");
        }

        // Gets the selected seats from the screening auditorium
        var seats = await _bookingRepository.GetSeatsByIdsAsync(
            screening.AuditoriumId,
            seatIds,
            cancellationToken);

        // Checks that all selected seats exist
        if (seats.Count != seatIds.Length)
        {
            throw new ArgumentException(
                "Ett eller flera säten finns inte i visningens salong.",
                nameof(request));
        }

        // Checks if all selected seats are available
        var seatsAreAvailable =
            await _bookingRepository.AreSeatsAvailableAsync(
                screening.Id,
                seatIds,
                cancellationToken);

        // Stops the booking if one or more seats are already booked
        if (!seatsAreAvailable)
        {
            throw new SeatsUnavailableException();
        }

        // Saves the current time as the booking creation time
        var createdAt = DateTime.UtcNow;

        // Creates the new booking
        var booking = new Booking
        {
            UserId = userId,
            ScreeningId = screening.Id,
            BookingReference =
                await CreateUniqueBookingReferenceAsync(cancellationToken),
            Status = BookingStatus.Paid,
            CreatedAt = createdAt
        };

        // Creates a ticket for each selected seat
        foreach (var seat in seats)
        {
            booking.Tickets.Add(new Ticket
            {
                BookingId = booking.Id,
                ScreeningId = screening.Id,
                SeatId = seat.Id,
                Price = seat.SeatType.Price
            });
        }

        // Calculates the total price of all tickets
        booking.TotalAmount = booking.Tickets.Sum(ticket => ticket.Price);

        // Adds the booking to the database
        await _bookingRepository.AddAsync(booking, cancellationToken);

        // Saves all changes to the database
        await _bookingRepository.SaveChangesAsync(cancellationToken);

        // Creates a list of the booked seats for the confirmation
        var bookedSeats = seats
            .OrderBy(seat => seat.RowLabel)
            .ThenBy(seat => seat.SeatNumber)
            .Select(seat => new BookedSeatDto(
                seat.Id,
                seat.RowLabel,
                seat.SeatNumber,
                seat.SeatType.Name,
                seat.SeatType.Price))
            .ToList();

        // Returns the completed booking confirmation
        return new BookingConfirmationDto(
            booking.Id,
            screening.Id,
            booking.BookingReference,
            booking.Status,
            screening.Movie.Title,
            screening.StartTime,
            screening.Auditorium.Name,
            bookedSeats,
            booking.TotalAmount,
            booking.CreatedAt);
    }
    
    // Converts a Booking entity to a BookingListItemDto
    private static BookingListItemDto MapToBookingListItemDto(
        Booking booking)
    {
        // Gets and sorts all booked seats
        var seats = booking.Tickets
            // Sorts the seats by row
            .OrderBy(ticket => ticket.Seat.RowLabel)

            // Sorts seats in the same row by seat number
            .ThenBy(ticket => ticket.Seat.SeatNumber)

            // Converts each ticket to a BookedSeatDto
            .Select(ticket => new BookedSeatDto(
                ticket.SeatId,
                ticket.Seat.RowLabel,
                ticket.Seat.SeatNumber,
                ticket.Seat.SeatType.Name,
                ticket.Price))

            // Converts the result to a list
            .ToList();

        // Creates and returns the booking information as a DTO
        return new BookingListItemDto(
            booking.Id,
            booking.ScreeningId,
            booking.BookingReference,
            booking.Status,
            booking.Screening.Movie.Title,
            booking.Screening.StartTime,
            booking.Screening.Auditorium.Name,
            seats,
            booking.TotalAmount);
    }
    
    // Converts a Booking entity to a BookingDto
    private static BookingDto MapToBookingDto(Booking booking)
    {
        return new BookingDto(
            booking.Id,
            booking.ScreeningId,
            booking.BookingReference,
            booking.Status,
            booking.CreatedAt,
            booking.CancelledAt,
            booking.TotalAmount,
            booking.Tickets.Count);
    }

    // Gets all current bookings for a user
    public async Task<IReadOnlyList<BookingListItemDto>> GetCurrentBookingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Checks that the user ID is valid
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "Användarens ID får inte vara tomt.",
                nameof(userId));
        }

        // Gets all bookings that belong to the user
        var bookings = await _bookingRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        // Gets the current time
        var currentTime = DateTime.UtcNow;

        return bookings
            // Makes sure the booking belongs to the user
            .Where(booking => booking.UserId == userId)

            // Removes cancelled bookings
            .Where(booking =>
                booking.Status != BookingStatus.Cancelled)

            // Only includes bookings for upcoming screenings
            .Where(booking =>
                booking.Screening.StartTime > currentTime)

            // Sorts bookings by the closest screening first
            .OrderBy(booking =>
                booking.Screening.StartTime)

            // Converts each booking to a BookingListItemDto
            .Select(MapToBookingListItemDto)

            // Converts the result to a list
            .ToList();
    }

    // Gets the customer's completed paid bookings
    public async Task<IReadOnlyList<BookingListItemDto>> GetBookingHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "Användarens ID får inte vara tomt.",
                nameof(userId));
        }

        var bookings = await _bookingRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        var currentTime = DateTime.UtcNow;

        return bookings
            // Only return the customer's bookings
            .Where(booking => booking.UserId == userId)

            // Everything that is no longer upcoming: the screening has been
            // and gone, or the customer cancelled it. Cancelled bookings have
            // to land somewhere, or the customer never sees that status.
            .Where(booking =>
                booking.Status == BookingStatus.Cancelled ||
                booking.Screening.StartTime <= currentTime)

            // Show the most recent screening first
            .OrderByDescending(booking =>
                booking.Screening.StartTime)

            .Select(MapToBookingListItemDto)
            .ToList();
    }

    public async Task<ScreeningSeatMapDto> GetSeatMapAsync(Guid screeningId, CancellationToken cancellationToken = default)
    {
        if (screeningId == Guid.Empty)
        {
            throw new ArgumentException(
                "Visningens ID får inte vara tomt.",
                nameof(screeningId));
        }

        var screening = await _bookingRepository.GetScreeningAsync(
            screeningId,
            cancellationToken);

        if (screening is null)
        {
            throw new KeyNotFoundException(
                "Visningen kunde inte hittas.");
        }

        if (screening.IsCancelled)
        {
            throw new InvalidOperationException(
                "Visningen är avbokad.");
        }

        if (screening.StartTime <= DateTime.UtcNow)
        {
            throw new InvalidOperationException(
                "Visningen har redan börjat.");
        }

        var seats = await _bookingRepository.GetSeatsByAuditoriumIdAsync(
            screening.AuditoriumId,
            cancellationToken);

        var unavailableSeatIds =
            await _bookingRepository.GetUnavailableSeatIdsAsync(
                screening.Id,
                cancellationToken);

        var unavailableSeatIdSet = unavailableSeatIds.ToHashSet();

        var seatDtos = seats
            .OrderBy(seat => seat.RowLabel)
            .ThenBy(seat => seat.SeatNumber)
            .Select(seat => new SeatAvailabilityDto(
                seat.Id,
                seat.RowLabel,
                seat.SeatNumber,
                seat.SeatType.Name,
                seat.SeatType.Price,
                !unavailableSeatIdSet.Contains(seat.Id)))
            .ToList();

        return new ScreeningSeatMapDto(
            screening.Id,
            screening.Movie.Title,
            screening.StartTime,
            screening.Auditorium.Name,
            seatDtos);
    }


    // Gets a booking with all information needed by the details page
    public async Task<BookingConfirmationDto> GetBookingByIdAsync(
        Guid bookingId,
        Guid requesterUserId,
        bool canManageAllBookings,
        CancellationToken cancellationToken = default)
    {
        if (bookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "Bokningens ID får inte vara tomt.",
                nameof(bookingId));
        }

        if (requesterUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Användarens ID får inte vara tomt.",
                nameof(requesterUserId));
        }

        var booking = await _bookingRepository.GetByIdAsync(
            bookingId,
            cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException(
                "Bokningen kunde inte hittas.");
        }

        var ownsBooking = booking.UserId == requesterUserId;

        if (!ownsBooking && !canManageAllBookings)
        {
            throw new UnauthorizedAccessException(
                "Du har inte behörighet att visa bokningen.");
        }

        var seats = booking.Tickets
            .OrderBy(ticket => ticket.Seat.RowLabel)
            .ThenBy(ticket => ticket.Seat.SeatNumber)
            .Select(ticket => new BookedSeatDto(
                ticket.SeatId,
                ticket.Seat.RowLabel,
                ticket.Seat.SeatNumber,
                ticket.Seat.SeatType.Name,
                ticket.Price))
            .ToList();

        return new BookingConfirmationDto(
            booking.Id,
            booking.ScreeningId,
            booking.BookingReference,
            booking.Status,
            booking.Screening.Movie.Title,
            booking.Screening.StartTime,
            booking.Screening.Auditorium.Name,
            seats,
            booking.TotalAmount,
            booking.CreatedAt);
    }

    // Cancels a booking and releases its seats
    public async Task<BookingDto> CancelBookingAsync(Guid bookingId, Guid requesterUserId, bool canManageAllBookings,
        CancellationToken cancellationToken = default)
    {
        if (bookingId == Guid.Empty)
        {
            throw new ArgumentException(
                "Bokningens ID får inte vara tomt.",
                nameof(bookingId));
        }

        if (requesterUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Användarens ID får inte vara tomt.",
                nameof(requesterUserId));
        }

        var booking = await _bookingRepository.GetByIdAsync(
            bookingId,
            cancellationToken);

        if (booking is null)
        {
            throw new KeyNotFoundException(
                "Bokningen kunde inte hittas.");
        }

        var ownsBooking = booking.UserId == requesterUserId;

        if (!ownsBooking && !canManageAllBookings)
        {
            throw new UnauthorizedAccessException(
                "Du har inte behörighet att avboka bokningen.");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "Bokningen är redan avbokad.");
        }

        var cancelledAt = DateTime.UtcNow;

        var cancellationDeadline =
            booking.Screening.StartTime.AddMinutes(-15);

        if (cancelledAt >= cancellationDeadline)
        {
            throw new InvalidOperationException(
                "Bokningen kan inte avbokas senare än 15 minuter före visningen.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAt = cancelledAt;

        foreach (var ticket in booking.Tickets.Where(ticket => ticket.IsActive))
        {
            ticket.Release(cancelledAt);
        }

        await _bookingRepository.SaveChangesAsync(cancellationToken);

        return MapToBookingDto(booking);
    }

    // Validates that the booking request contains valid information
    private static void ValidateRequest(
        Guid userId,
        CreateBookingDto request)
    {
        // Checks that the user ID is valid
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "Användarens ID får inte vara tomt.",
                nameof(userId));
        }

        // Checks that the screening ID is valid
        if (request.ScreeningId == Guid.Empty)
        {
            throw new ArgumentException(
                "Visningens ID får inte vara tomt.",
                nameof(request));
        }

        // Checks that at least one seat has been selected
        if (request.SeatIds is null || request.SeatIds.Count == 0)
        {
            throw new ArgumentException(
                "Minst ett säte måste väljas.",
                nameof(request));
        }

        // Checks that all seat IDs are valid
        if (request.SeatIds.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException(
                "Ett sätes-ID får inte vara tomt.",
                nameof(request));
        }
    }

    // Creates a unique booking reference
    private async Task<string> CreateUniqueBookingReferenceAsync(
        CancellationToken cancellationToken)
    {
        // Maximum number of attempts to create a unique reference
        const int maximumAttempts = 5;

        // Tries to create a unique booking reference
        for (var attempt = 0; attempt < maximumAttempts; attempt++)
        {
            var reference = CreateBookingReference();

            // Checks if the booking reference already exists
            var alreadyExists =
                await _bookingRepository.BookingReferenceExistsAsync(
                    reference,
                    cancellationToken);

            // Returns the reference if it is unique
            if (!alreadyExists)
            {
                return reference;
            }
        }

        throw new InvalidOperationException(
            "Det gick inte att skapa ett unikt bokningsnummer.");
    }

    // Generates a random 10-character booking reference
    private static string CreateBookingReference()
    {
        const int referenceLength = 10;
        Span<char> reference = stackalloc char[referenceLength];

        // Generates each character in the booking reference
        for (var i = 0; i < reference.Length; i++)
        {
            var index = RandomNumberGenerator.GetInt32(
                BookingReferenceCharacters.Length);

            reference[i] = BookingReferenceCharacters[index];
        }

        // Converts the generated characters into a string
        return new string(reference);
    }
}