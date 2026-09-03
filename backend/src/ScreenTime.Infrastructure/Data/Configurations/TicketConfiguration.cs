using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ScreenTime.Domain.Entities;

namespace ScreenTime.Infrastructure.Data.Configurations;

public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        // Snapshot of SeatType.Price at purchase, so later price changes do
        // not rewrite history.
        builder.Property(t => t.Price).HasPrecision(18, 2);

        // The double-booking guard: one *active* ticket per seat per screening.
        // Cancelling releases a ticket (Ticket.ReleasedAt) instead of deleting
        // it, so the row survives as history. The filter is what stops a
        // released row from holding the seat for good.
        builder.HasIndex(t => new { t.ScreeningId, t.SeatId })
            .IsUnique()
            .HasFilter("[ReleasedAt] IS NULL");

        builder.HasOne(t => t.Booking)
            .WithMany(b => b.Tickets)
            .HasForeignKey(t => t.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Screening)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.ScreeningId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Seat)
            .WithMany(s => s.Tickets)
            .HasForeignKey(t => t.SeatId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
