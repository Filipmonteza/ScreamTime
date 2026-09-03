using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScreenTime.Application.Contracts;
using ScreenTime.Application.Interfaces;
using ScreenTime.WebApi.Filters;

namespace ScreenTime.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[BookingExceptionFilter]
public class BookingsController : ControllerBase
{
    // No roles are seeded yet, so these match nobody today and every caller is
    // treated as a customer acting on their own bookings. Move them to a shared
    // constant once role seeding lands.
    private const string AdminRole = "Admin";
    private const string StaffRole = "Staff";

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
    // login. It lives here rather than on a ScreeningsController so that the
    // /api/screenings route stays free for whoever writes that controller.
    [HttpGet("screenings/{screeningId:guid}/seats")]
    [AllowAnonymous]
    public async Task<ActionResult<ScreeningSeatMapDto>> GetSeatMap(
        Guid screeningId,
        CancellationToken cancellationToken) =>
        Ok(await _bookingService.GetSeatMapAsync(screeningId, cancellationToken));

    private bool CanManageAllBookings =>
        User.IsInRole(AdminRole) || User.IsInRole(StaffRole);

    // JwtTokenService writes the user id to both sub and NameIdentifier.
    private Guid GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new UnauthorizedAccessException(
                "Token saknar ett giltigt användar-ID.");
    }
}
