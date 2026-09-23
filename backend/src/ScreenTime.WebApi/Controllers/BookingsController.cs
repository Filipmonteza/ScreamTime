using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using ScreenTime.Infrastructure.Identity;
using ScreenTime.WebApi.Filters;
using System.Security.Claims;

namespace ScreenTime.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[DomainExceptionFilter]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<ActionResult<BookingConfirmationDto>> CreateBooking(
        CreateBookingDto request,
        CancellationToken cancellationToken)
    {
        var booking = await _bookingService.CreateBookingAsync(
            GetUserId(),
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetBookingById),
            new { id = booking.BookingId },
            booking);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingConfirmationDto>> GetBookingById(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await _bookingService.GetBookingByIdAsync(
            id,
            GetUserId(),
            CanManageAllBookings,
            cancellationToken));

    // Upcoming, not-cancelled bookings.
    [HttpGet("current")]
    public async Task<ActionResult<IReadOnlyList<BookingListItemDto>>> GetCurrentBookings(
        CancellationToken cancellationToken) =>
        Ok(await _bookingService.GetCurrentBookingsAsync(
            GetUserId(),
            cancellationToken));

    // Screenings that have been and gone, plus anything the customer cancelled.
    [HttpGet("history")]
    public async Task<ActionResult<IReadOnlyList<BookingListItemDto>>> GetBookingHistory(
        CancellationToken cancellationToken) =>
        Ok(await _bookingService.GetBookingHistoryAsync(
            GetUserId(),
            cancellationToken));

    // POST rather than DELETE: the booking survives as a cancelled record, and
    // the caller gets it back.
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<BookingDto>> CancelBooking(
        Guid id,
        CancellationToken cancellationToken) =>
        Ok(await _bookingService.CancelBookingAsync(
            id,
            GetUserId(),
            CanManageAllBookings,
            cancellationToken));

    // Browsing the seat map is part of picking a screening, so it needs no
    // login. It answers on /api/bookings rather than /api/screenings because
    // availability is booking data, not programme data.
    [HttpGet("screenings/{screeningId:guid}/seats")]
    [AllowAnonymous]
    public async Task<ActionResult<ScreeningSeatMapDto>> GetSeatMap(
        Guid screeningId,
        CancellationToken cancellationToken) =>
        Ok(await _bookingService.GetSeatMapAsync(screeningId, cancellationToken));

    // Staff act on any booking; a customer only on their own.
    private bool CanManageAllBookings =>
        User.IsInRole(IdentitySeeder.AdminRole) || User.IsInRole(IdentitySeeder.StaffRole);

    // JwtTokenService writes the user id to both sub and NameIdentifier.
    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException(
                "Token saknar ett giltigt användar-ID.");
    }

    [HttpGet("search")]
    [Authorize(Roles = $"{IdentitySeeder.AdminRole},{IdentitySeeder.StaffRole}")]
    public async Task<ActionResult<PagedResult<BookingSummaryDto>>> Search(
        [FromQuery] BookingSearchDto dto,
        CancellationToken cancellationToken)
        => Ok(await _bookingService.SearchBookingsAsync(dto, cancellationToken));
}
