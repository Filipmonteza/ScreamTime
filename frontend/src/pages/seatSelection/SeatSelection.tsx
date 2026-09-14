import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { ScreeningSeatMap, SeatAvailability } from "../../types/seatMap";
import { getSeatMap, createBooking } from "../../api/bookingApi";
import type { BookingConfirmation } from "../../types/booking";
import { useLocation } from "react-router-dom";
import { useAuth } from "../../context/useAuth";

function SeatSelection() {
  const { screeningId } = useParams<{ screeningId: string }>();
  const navigate = useNavigate();
  const [seatMap, setSeatMap] = useState<ScreeningSeatMap | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedSeatIds, setSelectedSeatIds] = useState<string[]>([]);
  const location = useLocation();
  const { isAuthenticated } = useAuth();
  const [showConfirm, setShowConfirm] = useState(false);
  const [booking, setBooking] = useState<BookingConfirmation | null>(null);
  const [bookingError, setBookingError] = useState<string | null>(null);

  useEffect(() => {
    if (!screeningId) return;
    getSeatMap(screeningId)
      .then(setSeatMap)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [screeningId]);

  function toggleSeat(seat: SeatAvailability) {
    if (!seat.isAvailable) return;
    setSelectedSeatIds((prev) =>
      prev.includes(seat.seatId)
        ? prev.filter((id) => id !== seat.seatId)
        : [...prev, seat.seatId],
    );
  }

  function handleBokaClick() {
    if (!isAuthenticated) {
      navigate("/login", { state: { from: location.pathname } });
      return;
    }
    setShowConfirm(true);
  }

  async function handleConfirm() {
    if (!screeningId) return;
    try {
      const confirmation = await createBooking(screeningId, selectedSeatIds);
      setBooking(confirmation);
      setShowConfirm(false);
    } catch {
      setBookingError(
        "En eller flera platser blev nyss bokade av någon annan. Välj platser igen.",
      );
      setShowConfirm(false);
      setSelectedSeatIds([]);
      getSeatMap(screeningId).then(setSeatMap);
    }
  }

  const selectedSeats =
    seatMap?.seats.filter((s) => selectedSeatIds.includes(s.seatId)) ?? [];
  const totalPrice = selectedSeats.reduce((sum, s) => sum + s.price, 0);

  const rows = seatMap
    ? [...new Set(seatMap.seats.map((s) => s.rowLabel))].sort()
    : [];

  return (
    <div className="page seat-selection">
      <button onClick={() => navigate(-1)}>Tillbaka</button>
      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}
      {!loading && !error && seatMap && (
        <div className="seat-selection-content">
          <h1>{seatMap.movieTitle}</h1>
          <p>
            {new Date(seatMap.screeningStartTime).toLocaleString("sv-SE")},{" "}
            {seatMap.auditoriumName}
          </p>

          <h2>Välj platser</h2>
          {bookingError && (
            <p className="seat-selection-error">{bookingError}</p>
          )}
          <div className="seat-selection-screen">Duk</div>

          <div className="seat-selection-grid">
            {rows.map((row) => (
              <div key={row} className="seat-selection-row">
                {seatMap.seats
                  .filter((s) => s.rowLabel === row)
                  .sort((a, b) => a.seatNumber - b.seatNumber)
                  .map((seat) => {
                    const isSelected = selectedSeatIds.includes(seat.seatId);
                    const isVip = seat.seatType.toLowerCase() === "vip";
                    return (
                      <button
                        key={seat.seatId}
                        className={[
                          "seat",
                          isVip ? "seat-vip" : "",
                          isSelected ? "seat-selected" : "",
                          !seat.isAvailable ? "seat-unavailable" : "",
                        ]
                          .filter(Boolean)
                          .join(" ")}
                        disabled={!seat.isAvailable}
                        onClick={() => toggleSeat(seat)}
                      >
                        {isVip ? "VIP" : ""}
                      </button>
                    );
                  })}
              </div>
            ))}
          </div>

          <div className="seat-selection-bottom-bar">
            <div>
              <p>Biljetter</p>
              <p>{selectedSeats.length}</p>
            </div>
            <div>
              <p>Summa</p>
              <p>{totalPrice} kr</p>
            </div>
            <button
              className="btn-pill"
              disabled={selectedSeats.length === 0}
              onClick={handleBokaClick}
            >
              Boka
            </button>
          </div>
          {showConfirm && (
            <div className="seat-selection-confirm">
              <h2>Bekräfta bokning</h2>
              <p>Är du säker på att du vill boka {seatMap.movieTitle}?</p>
              <button onClick={() => setShowConfirm(false)}>Nej</button>
              <button onClick={handleConfirm}>Ja</button>
            </div>
          )}
          {booking && (
            <div className="seat-selection-confirm">
              <h2>Välkommen att se {booking.movieTitle}!</h2>
              <p>
                Du har bokat biljetter för att se {booking.movieTitle}, se dina
                bokningar i Bokningar.
              </p>
              <button
                className="btn-pill"
                onClick={() => navigate("/bookings")}
              >
                Mina bokningar
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default SeatSelection;
