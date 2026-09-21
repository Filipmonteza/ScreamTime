import { useCallback, useEffect, useMemo, useState } from "react";
import type { Screening } from "../../types/screening";
import { getAllScreenings, cancelScreening } from "../../api/screeningsApi";
import ScreeningForm from "./ScreeningForm";
import SearchBar from "../../components/SearchBar";
import FilterSelect from "../../components/FilterSelect";
import DayStrip from "../../components/DayStrip";
import ListToolbar from "../../components/ListToolbar";
import ListCard from "../../components/ListCard";
import "./Screenings.css";
import { useAuth } from "../../context/useAuth";
import { useNavigate } from "react-router-dom";
import { dateKeyFromIso, formatDayLabel } from "../../utils/dateOptions";

function formatTime(dateStr: string): string {
  return new Date(dateStr).toLocaleTimeString("sv-SE", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

function Screenings() {
  const navigate = useNavigate();
  const { user } = useAuth();

  const canManageScreenings =
    user?.role === "Admin" || user?.role === "Staff";

  const [screenings, setScreenings] = useState<Screening[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [selectedDay, setSelectedDay] = useState<string | null>(null);
  const [auditoriumFilter, setAuditoriumFilter] = useState("");
  const [confirmingDeleteId, setConfirmingDeleteId] = useState<string | null>(
    null,
  );
  const [showAddForm, setShowAddForm] = useState(false);
  const [editingScreening, setEditingScreening] = useState<Screening | null>(
    null,
  );
  const [deletingId, setDeletingId] = useState<string | null>(null);

  const loadScreenings = useCallback(() => {
    getAllScreenings({ includeCancelled: canManageScreenings })
      .then((data) => {
        setError(null);
        setScreenings(data);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [canManageScreenings]);

  useEffect(() => {
    loadScreenings();
  }, [loadScreenings]);

  function handleCancel(id: string) {
    setError(null);
    setDeletingId(id);

    cancelScreening(id)
      .then(() => {
        setConfirmingDeleteId(null);
        loadScreenings();
      })
      .catch((err) => {
        setConfirmingDeleteId(null);
        setError(err.message);
      })
      .finally(() => setDeletingId(null));
  }

  const uniqueAuditoriums = useMemo(
    () => [...new Set(screenings.map((s) => s.auditoriumName))],
    [screenings],
  );

  const filteredScreenings = useMemo(() => {
    const q = query.trim().toLowerCase();

    return screenings.filter((s) => {
      const matchesQuery = s.movieTitle.toLowerCase().includes(q);

      const matchesAuditorium =
        !auditoriumFilter || s.auditoriumName === auditoriumFilter;

      return matchesQuery && matchesAuditorium;
    });
  }, [screenings, query, auditoriumFilter]);

  // The days that still have something to show after the search and auditorium
  // filters. The API returns screenings in start-time order, so the keys come
  // out in date order too.
  const days = useMemo(
    () => [
      ...new Set(filteredScreenings.map((s) => dateKeyFromIso(s.startTime))),
    ],
    [filteredScreenings],
  );

  // The chosen day is kept even while a filter hides it, so clearing the
  // filter lands back on it. Until then the strip shows the nearest day on
  // or after it, or the last day if nothing follows.
  const activeDay =
    selectedDay === null
      ? days[0]
      : days.find((date) => date >= selectedDay) ?? days[days.length - 1];

  const visibleScreenings = filteredScreenings.filter(
    (s) => dateKeyFromIso(s.startTime) === activeDay,
  );

  return (
    <div className="page screenings">
      <h1>Visningar</h1>

      <SearchBar
        value={query}
        onChange={setQuery}
        placeholder="Sök film"
      />

      <div className="screening-filters">
        <FilterSelect
          value={auditoriumFilter}
          onChange={setAuditoriumFilter}
          options={uniqueAuditoriums.map((name) => ({
            value: name,
            label: name,
          }))}
          placeholder="Alla salonger"
        />
      </div>

      {canManageScreenings && (
        <ListToolbar
          addLabel="Lägg till visning"
          onAdd={() => setShowAddForm(true)}
        />
      )}

      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && !error && days.length === 0 && (
        <p>Inga visningar kunde hittas</p>
      )}

      {!loading && activeDay && (
        <section className="screening-day">
          <DayStrip
            days={days}
            selected={activeDay}
            onSelect={setSelectedDay}
          />

          <h2 className="screening-day-heading">
            {formatDayLabel(activeDay)}
          </h2>

          <div className="card-grid">
            {visibleScreenings.map((screening) => (
              <ListCard
                key={screening.screeningId}
                title={screening.movieTitle}
                statusLabel={
                  screening.isCancelled ? "INSTÄLLD" : undefined
                }
                primaryActionLabel={
                  canManageScreenings || screening.isCancelled
                      ? undefined
                      : screening.bookedSeats >= screening.capacity
                          ? "Fullbokad"
                          : "Boka"
                }
                primaryActionDisabled={
                    screening.bookedSeats >= screening.capacity
                }
                onPrimaryAction={() =>
                    navigate(`/booking/${screening.screeningId}`)
                }
                inactiveStyle={screening.isCancelled}
                showActions={
                  canManageScreenings && !screening.isCancelled
                }
                deleteLabel="Ställ in"
                infoLines={[
                  formatTime(screening.startTime),
                  screening.auditoriumName,
                  `Sålda biljetter: ${screening.bookedSeats}/${screening.capacity}`,
                ]}
                confirming={
                  canManageScreenings &&
                  confirmingDeleteId === screening.screeningId
                }
                deleting={
                  canManageScreenings &&
                  deletingId === screening.screeningId
                }
                onEdit={() => {
                  if (canManageScreenings) {
                    setEditingScreening(screening);
                  }
                }}
                onRequestDelete={() => {
                  if (canManageScreenings) {
                    setConfirmingDeleteId(screening.screeningId);
                  }
                }}
                onConfirmDelete={() => {
                  if (canManageScreenings) {
                    handleCancel(screening.screeningId);
                  }
                }}
                onCancelDelete={() => setConfirmingDeleteId(null)}
              />
            ))}
          </div>
        </section>
      )}

      {canManageScreenings && showAddForm && (
        <ScreeningForm
          onClose={() => setShowAddForm(false)}
          onSaved={() => {
            setShowAddForm(false);
            loadScreenings();
          }}
        />
      )}

      {canManageScreenings && editingScreening && (
        <ScreeningForm
          screening={editingScreening}
          onClose={() => setEditingScreening(null)}
          onSaved={() => {
            setEditingScreening(null);
            loadScreenings();
          }}
        />
      )}
    </div>
  );
}

export default Screenings;
