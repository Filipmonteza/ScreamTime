import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { ScreeningSeatMap, SeatAvailability } from "../../types/seatMap";
import { getSeatMap, createBooking } from "../../api/bookingApi";
import type { BookingConfirmation } from "../../types/booking";
import { useLocation } from "react-router-dom";
import { useAuth } from "../../context/useAuth";
import { getScreeningById } from "../../api/screeningsApi";
import { getMovieById } from "../../api/moviesApi";
import { getPosterForMovie } from "../../utils/posterMap";
import type { Movie } from "../../types/movie";
import { formatDateTimeLabel } from "../../utils/dateOptions";
import "./SeatSelection.css";
import iconSeatSelected from "../../assets/icons/Icon_Seat_Selected.svg";
import iconSeatUnavailable from "../../assets/icons/Icon_Seat_Unavailable.svg";
import iconArrowBack from "../../assets/icons/Icon_Arrow_Back.svg";
import Modal from "../../components/Modal";
import Message from "../../components/Message";

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

  const [movie, setMovie] = useState<Movie | null>(null);

  useEffect(() => {
    if (!screeningId) return;
    getScreeningById(screeningId)
      .then((screening) => getMovieById(screening.movieId))
      .then(setMovie)
      .catch(() => {});
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
    setBookingError(null);
    try {
      const confirmation = await createBooking(screeningId, selectedSeatIds);
      setBooking(confirmation);
      setShowConfirm(false);
    } catch (err) {
      const status =
        err instanceof Error
          ? (err as Error & { status?: number }).status
          : undefined;

      if (status === 401) {
        navigate("/login", { state: { from: location.pathname } });
        return;
      }

      setBookingError(
        status === 409
          ? "En eller flera platser blev nyss bokade av någon annan. Välj platser igen."
          : "Bokningen kunde inte genomföras. Försök igen.",
      );
      setShowConfirm(false);
      setSelectedSeatIds([]);
      getSeatMap(screeningId)
        .then(setSeatMap)
        .catch((err) => setError(err.message));
    }
  }

  const selectedSeats =
    seatMap?.seats.filter((s) => selectedSeatIds.includes(s.seatId)) ?? [];
  const totalPrice = selectedSeats.reduce((sum, s) => sum + s.price, 0);

  const rows = seatMap
    ? [...new Set(seatMap.seats.map((s) => s.rowLabel))].sort()
    : [];

  const seatsPerRowCounts = rows.map(
    (row) => seatMap?.seats.filter((s) => s.rowLabel === row).length ?? 0,
  );
  const maxSeatsPerRow =
    seatsPerRowCounts.length > 0 ? Math.max(...seatsPerRowCounts) : 8;

  const GAP_RATIO = 12 / 30;
  const MIN_SEAT_SIZE = 16;
  const fittedSeatSize =
    324 / (maxSeatsPerRow + (maxSeatsPerRow - 1) * GAP_RATIO);
  const seatSize = Math.min(30, Math.max(MIN_SEAT_SIZE, fittedSeatSize));
  const seatGap = seatSize * GAP_RATIO;

  const vipCount =
    seatMap?.seats.filter((s) => s.seatType.toLowerCase() === "vip").length ??
    0;
  const standardCount = (seatMap?.seats.length ?? 0) - vipCount;

  const standardPrice =
    seatMap?.seats.find((s) => s.seatType.toLowerCase() === "standard")
      ?.price ?? 0;
  const vipPrice =
    seatMap?.seats.find((s) => s.seatType.toLowerCase() === "vip")?.price ?? 0;

  return (
    <div className="page seat-selection">
      <button className="seat-selection-back" onClick={() => navigate(-1)}>
        <img
          className="seat-selection-back-icon"
          src={iconArrowBack}
          alt="Tillbaka"
        />
      </button>
      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}
      {!loading && !error && seatMap && (
        <div className="seat-selection-content">
          <div className="seat-selection-card">
            <h2>{seatMap.auditoriumName}</h2>
            <p>
              {vipCount} VIP, {standardCount} standard
            </p>
            <div className="seat-selection-legend">
              {standardCount > 0 && (
                <div className="seat-selection-legend-item">
                  <span className="seat-selection-legend-swatch seat-selection-legend-swatch-standard" />
                  Standard
                  <br />
                  {standardPrice} kr
                </div>
              )}
              {vipCount > 0 && (
                <div className="seat-selection-legend-item">
                  <span className="seat-selection-legend-swatch seat-selection-legend-swatch-vip" />
                  VIP
                  <br />
                  {vipPrice} kr
                </div>
              )}
              <div className="seat-selection-legend-item">
                <span className="seat-selection-legend-swatch seat-selection-legend-swatch-unavailable">
                  <img
                    src={iconSeatUnavailable}
                    alt=""
                    className="seat-selection-legend-icon"
                  />
                </span>
                Upptagen
              </div>
            </div>

            <div className="seat-selection-screen" />
            {bookingError && <Message type="error">{bookingError}</Message>}
            <div
              className="seat-selection-grid"
              style={
                {
                  "--seat-size": `${seatSize}px`,
                  "--seat-gap": `${seatGap}px`,
                } as React.CSSProperties
              }
            >
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
                          {!seat.isAvailable && (
                            <img
                              src={iconSeatUnavailable}
                              alt=""
                              className="seat-icon"
                            />
                          )}
                          {seat.isAvailable && isSelected && (
                            <img
                              src={iconSeatSelected}
                              alt=""
                              className="seat-icon"
                            />
                          )}
                        </button>
                      );
                    })}
                </div>
              ))}
            </div>
          </div>

          <div className="seat-selection-purchase">
            <h2>Mitt köp</h2>
            <div className="seat-selection-purchase-header">
              {movie && (
                <img
                  className="seat-selection-purchase-poster"
                  src={getPosterForMovie(movie)}
                  alt={movie.title}
                />
              )}
              <div>
                <p className="seat-selection-purchase-title">
                  {seatMap.movieTitle}
                </p>
                <p className="seat-selection-purchase-date">
                  {formatDateTimeLabel(seatMap.screeningStartTime)}
                </p>
                <p className="seat-selection-purchase-auditorium">
                  {seatMap.auditoriumName}
                </p>
              </div>
            </div>
            {selectedSeats.map((seat) => (
              <p
                key={seat.seatId}
                className="seat-selection-purchase-seat-line"
              >
                {seat.seatType}: Rad {seat.rowLabel}, Plats {seat.seatNumber}
              </p>
            ))}
            <p className="seat-selection-purchase-total">
              Summa: {totalPrice} kr
            </p>
            <button
              className="btn-pill"
              disabled={selectedSeats.length === 0}
              onClick={handleBokaClick}
            >
              Boka
            </button>
          </div>
          {showConfirm && (
            <Modal onClose={() => setShowConfirm(false)}>
              <h2 className="form-title">Bekräfta bokning</h2>
              <p>Är du säker på att du vill boka {seatMap.movieTitle}?</p>
              <div className="form-actions">
                <button className="btn-pill" onClick={handleConfirm}>
                  Ja
                </button>
                <button
                  className="btn-pill"
                  onClick={() => setShowConfirm(false)}
                >
                  Nej
                </button>
              </div>
            </Modal>
          )}
          {booking && (
            <Modal onClose={() => navigate("/bookings")}>
              <h2 className="form-title">
                Välkommen att se {booking.movieTitle}!
              </h2>
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
            </Modal>
          )}
        </div>
      )}
    </div>
  );
}

export default SeatSelection;
