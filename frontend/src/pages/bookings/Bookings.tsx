import { useEffect, useState } from "react";
import { BookingStatus, type BookingListItem } from "../../types/booking";
import {
  getCurrentBookings,
  getBookingHistory,
  cancelBooking,
} from "../../api/bookingApi";

function Bookings() {
  const [current, setCurrent] = useState<BookingListItem[]>([]);
  const [history, setHistory] = useState<BookingListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function handleCancel(bookingId: string) {
    try {
      await cancelBooking(bookingId);
      const [currentBookings, pastBookings] = await Promise.all([
        getCurrentBookings(),
        getBookingHistory(),
      ]);
      setCurrent(currentBookings);
      setHistory(pastBookings);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Kunde inte avboka.");
    }
  }

  useEffect(() => {
    Promise.all([getCurrentBookings(), getBookingHistory()])
      .then(([currentBookings, pastBookings]) => {
        setCurrent(currentBookings);
        setHistory(pastBookings);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  const pastBookings = history.filter(
    (b) => b.status !== BookingStatus.Cancelled,
  );
  const cancelledBookings = history.filter(
    (b) => b.status === BookingStatus.Cancelled,
  );

  return (
    <div className="page bookings">
      <h1>Mina biljetter</h1>
      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && !error && (
        <>
          <h2>Kommande</h2>
          {current.map((booking) => (
            <div key={booking.bookingId} className="booking-card">
              <h3>{booking.movieTitle}</h3>
              <p>
                {new Date(booking.screeningStartTime).toLocaleString("sv-SE")},{" "}
                {booking.auditoriumName}
              </p>
              {booking.seats.map((seat) => (
                <p key={seat.seatId}>
                  Rad {seat.rowLabel}, Plats {seat.seatNumber}, {seat.seatPrice}{" "}
                  kr
                </p>
              ))}
              <p>Bokningsnummer: {booking.bookingReference}</p>
              <button onClick={() => handleCancel(booking.bookingId)}>
                Avboka
              </button>
            </div>
          ))}
          <h2>Tidigare</h2>
          {pastBookings.map((booking) => (
            <div key={booking.bookingId} className="booking-card">
              <h3>{booking.movieTitle}</h3>
              <p>
                {new Date(booking.screeningStartTime).toLocaleString("sv-SE")},{" "}
                {booking.auditoriumName}
              </p>
              {booking.seats.map((seat) => (
                <p key={seat.seatId}>
                  Rad {seat.rowLabel}, Plats {seat.seatNumber}, {seat.seatPrice}{" "}
                  kr
                </p>
              ))}
              <p>Bokningsnummer: {booking.bookingReference}</p>
            </div>
          ))}
          <h2>Avbokade biljetter</h2>
          {cancelledBookings.map((booking) => (
            <div key={booking.bookingId} className="booking-card">
              <h3>{booking.movieTitle}</h3>
              <p>
                {new Date(booking.screeningStartTime).toLocaleString("sv-SE")},{" "}
                {booking.auditoriumName}
              </p>
              {booking.seats.map((seat) => (
                <p key={seat.seatId}>
                  Rad {seat.rowLabel}, Plats {seat.seatNumber}, {seat.seatPrice}{" "}
                  kr
                </p>
              ))}
              <p>Bokningsnummer: {booking.bookingReference}</p>
            </div>
          ))}
        </>
      )}
    </div>
  );
}

export default Bookings;
