import { useEffect, useState } from "react";
import type { PagedBookingResult } from "../../types/booking";
import { searchBookings } from "../../api/bookingApi";
import { cancelBooking } from "../../api/bookingApi";
import { formatDateTimeLabel } from "../../utils/dateOptions";
import SearchBar from "../../components/SearchBar";
import Pagination from "../../components/Pagination";
import "./AdminBookings.css";

function AdminBookings() {
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [result, setResult] = useState<PagedBookingResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [confirmingId, setConfirmingId] = useState<string | null>(null);
  const [cancellingId, setCancellingId] = useState<string | null>(null);

  useEffect(() => {
    // Under 2 tecken: ingen sökning alls, matchar backendens MinLength(2)
    // och undviker ett anrop per bokstav.
    if (search.trim().length < 2) {
      setResult(null);
      setError(null);
      return;
    }

    setLoading(true);
    searchBookings(search, page)
      .then((data) => {
        setError(null);
        setResult(data);
      })
      .catch((err: unknown) =>
        setError(err instanceof Error ? err.message : String(err)),
      )
      .finally(() => setLoading(false));
  }, [search, page]);

  function handleCancel(bookingId: string) {
    setCancellingId(bookingId);
    cancelBooking(bookingId)
      .then(() => {
        setConfirmingId(null);
        return searchBookings(search, page);
      })
      .then((data) => setResult(data))
      .catch((err: unknown) =>
        setError(err instanceof Error ? err.message : String(err)),
      )
      .finally(() => setCancellingId(null));
  }

  return (
    <div className="page bookings">
      <h1>Bokningar</h1>

      <SearchBar
        value={search}
        onChange={(value) => {
          setSearch(value);
          setPage(1);
        }}
        placeholder="Sök bokningsnummer eller email"
      />

      {loading && <p>Söker...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && search.trim().length >= 2 && result?.items.length === 0 && (
        <p>Inga bokningar matchade sökningen.</p>
      )}

      {result && result.items.length > 0 && (
        <>
          <div className="booking-card-grid">
            {result.items.map((booking) => {
              const isCancelled = booking.status === 2;

              return (
                <div key={booking.bookingId} className="booking-card">
                  <div className="booking-card-header">
                    <h3>Nr: {booking.bookingReference}</h3>

                    {!isCancelled &&
                      (confirmingId === booking.bookingId ? (
                        <div className="booking-confirm-actions">
                          <span>Säker?</span>
                          <button
                            className="btn-pill"
                            onClick={() => handleCancel(booking.bookingId)}
                            disabled={
                              cancellingId === booking.bookingId
                            }
                          >
                            Ja
                          </button>
                          <button
                            className="btn-pill"
                            onClick={() => setConfirmingId(null)}
                          >
                            Nej
                          </button>
                        </div>
                      ) : (
                        <button
                          className="btn-pill"
                          onClick={() =>
                            setConfirmingId(booking.bookingId)
                          }
                        >
                          Avboka
                        </button>
                      ))}
                  </div>

                  <p>{booking.customerEmail}</p>
                  <p>
                    <strong>{booking.movieTitle}</strong>
                  </p>
                  <p>
                    {formatDateTimeLabel(booking.screeningStartTime)},{" "}
                    {booking.auditoriumName}
                  </p>

                  {booking.seats.map((seat, i) => (
                    <p key={i}>
                      Rad {seat.rowLabel}, Plats {seat.seatNumber},{" "}
                      {seat.price} kr
                    </p>
                  ))}
                </div>
              );
            })}
          </div>

          <Pagination
            page={result.page}
            totalPages={result.totalPages}
            onPageChange={setPage}
          />
        </>
      )}
    </div>
  );
}

export default AdminBookings;
