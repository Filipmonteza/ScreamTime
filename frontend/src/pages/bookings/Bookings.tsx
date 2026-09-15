import { useEffect, useState } from "react";
import { BookingStatus, type BookingListItem } from "../../types/booking";
import {
  getCurrentBookings,
  getBookingHistory,
  cancelBooking,
} from "../../api/bookingApi";
import { formatDateTimeLabel } from "../../utils/dateOptions";
import { useNavigate } from "react-router-dom";
import "./Bookings.css";

function canCancel(screeningStartTime: string): boolean {
  return Date.now() < new Date(screeningStartTime).getTime() - 15 * 60 * 1000;
}

function Bookings() {
  const [current, setCurrent] = useState<BookingListItem[]>([]);
  const [history, setHistory] = useState<BookingListItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [cancelError, setCancelError] = useState<string | null>(null);
  const navigate = useNavigate();

  async function handleCancel(bookingId: string) {
    try {
      await cancelBooking(bookingId);
      setCancelError(null);
      const [currentBookings, pastBookings] = await Promise.all([
        getCurrentBookings(),
        getBookingHistory(),
      ]);
      setCurrent(currentBookings);
      setHistory(pastBookings);
    } catch (err) {
      const status =
        err instanceof Error
          ? (err as Error & { status?: number }).status
          : undefined;

      if (status === 401) {
        navigate("/login", { state: { from: "/bookings" } });
        return;
      }

      setCancelError(err instanceof Error ? err.message : "Kunde inte avboka.");
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
      <h1>Mina bokningar</h1>
      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && !error && (
        <>
          <div className="booking-section">
            <h2>Kommande</h2>
            {current.length === 0 && <p>Du har inga kommande bokningar.</p>}
            {cancelError && <p>Fel: {cancelError}</p>}
            <div className="booking-card-grid">
              {current.map((booking) => (
                <div key={booking.bookingId} className="booking-card">
                  <div className="booking-card-header">
                    <h3>{booking.movieTitle}</h3>
                    {canCancel(booking.screeningStartTime) && (
                      <button
                        className="btn-pill"
                        onClick={() => handleCancel(booking.bookingId)}
                      >
                        Avboka
                      </button>
                    )}
                  </div>
                  <p>Bokningsnr: {booking.bookingReference}</p>
                  <p>
                    {formatDateTimeLabel(booking.screeningStartTime)}
                  </p>
                  <p>{booking.auditoriumName}</p>
                  {booking.seats.map((seat) => (
                    <p key={seat.seatId}>
                      Rad {seat.rowLabel}, Plats {seat.seatNumber},{" "}
                      {seat.seatPrice} kr
                    </p>
                  ))}
                </div>
              ))}
            </div>
          </div>

          <div className="booking-section">
            <h2>Tidigare</h2>
            {pastBookings.length === 0 && (
              <p>Du har inga tidigare bokningar.</p>
            )}
            <div className="booking-card-grid">
              {pastBookings.map((booking) => (
                <div key={booking.bookingId} className="booking-card">
                  <h3>{booking.movieTitle}</h3>
                  <p>Bokningsnr: {booking.bookingReference}</p>
                  <p>
                    {formatDateTimeLabel(booking.screeningStartTime)}
                  </p>
                  <p>{booking.auditoriumName}</p>
                  {booking.seats.map((seat) => (
                    <p key={seat.seatId}>
                      Rad {seat.rowLabel}, Plats {seat.seatNumber},{" "}
                      {seat.seatPrice} kr
                    </p>
                  ))}
                </div>
              ))}
            </div>
          </div>
          <div className="booking-section">
            <h2>Avbokade</h2>
            {cancelledBookings.length === 0 && (
              <p>Du har inga avbokade biljetter.</p>
            )}
            <div className="booking-card-grid">
              {cancelledBookings.map((booking) => (
                <div key={booking.bookingId} className="booking-card">
                  <h3>{booking.movieTitle}</h3>
                  <p>Bokningsnr: {booking.bookingReference}</p>
                  <p>
                    {formatDateTimeLabel(booking.screeningStartTime)}
                  </p>
                  <p>{booking.auditoriumName}</p>
                  {booking.seats.map((seat) => (
                    <p key={seat.seatId}>
                      Rad {seat.rowLabel}, Plats {seat.seatNumber},{" "}
                      {seat.seatPrice} kr
                    </p>
                  ))}
                </div>
              ))}
            </div>
          </div>
        </>
      )}
    </div>
  );
}

export default Bookings;
