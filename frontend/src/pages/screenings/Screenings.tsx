import { useEffect, useMemo, useState } from "react";
import type { Screening } from "../../types/screening";
import { getAllScreenings, cancelScreening } from "../../api/screeningsApi";
import ScreeningForm from "./ScreeningForm";
import SearchBar from "../../components/SearchBar";
import ListToolbar from "../../components/ListToolbar";
import usePagination from "../../hooks/usePagination";
import ListCard from "../../components/ListCard";
import "./Screenings.css";
import iconChevronDown from "../../assets/icons/Icon_Chevron_Down.svg";
import { useAuth } from "../../context/useAuth";

function formatDay(dateStr: string): string {
  return new Date(dateStr).toLocaleDateString("sv-SE", {
    weekday: "long",
  });
}

function formatTime(dateStr: string): string {
  return new Date(dateStr).toLocaleTimeString("sv-SE", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

function Screenings() {
  const { user } = useAuth();

  const canManageScreenings =
    user?.role === "Admin" || user?.role === "Staff";

  const [screenings, setScreenings] = useState<Screening[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [dayFilter, setDayFilter] = useState("");
  const [auditoriumFilter, setAuditoriumFilter] = useState("");
  const [confirmingDeleteId, setConfirmingDeleteId] = useState<string | null>(
    null,
  );
  const [showAddForm, setShowAddForm] = useState(false);
  const [editingScreening, setEditingScreening] = useState<Screening | null>(
    null,
  );
  const [deletingId, setDeletingId] = useState<string | null>(null);

  function loadScreenings() {
    getAllScreenings(canManageScreenings)
      .then((data) => {
        setError(null);
        setScreenings(data);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    loadScreenings();
  }, [canManageScreenings]);

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

  const uniqueDays = useMemo(
    () => [...new Set(screenings.map((s) => formatDay(s.startTime)))],
    [screenings],
  );

  const uniqueAuditoriums = useMemo(
    () => [...new Set(screenings.map((s) => s.auditoriumName))],
    [screenings],
  );

  const filteredScreenings = useMemo(() => {
    const q = query.trim().toLowerCase();

    return screenings.filter((s) => {
      const matchesQuery = s.movieTitle.toLowerCase().includes(q);

      const matchesDay =
        !dayFilter || formatDay(s.startTime) === dayFilter;

      const matchesAuditorium =
        !auditoriumFilter || s.auditoriumName === auditoriumFilter;

      return matchesQuery && matchesDay && matchesAuditorium;
    });
  }, [screenings, query, dayFilter, auditoriumFilter]);

  const filterKey = `${query}__${dayFilter}__${auditoriumFilter}`;

  const {
    page,
    setPage,
    totalPages,
    pagedItems: pagedScreenings,
  } = usePagination(filteredScreenings, 12, filterKey);

  return (
    <div className="page screenings">
      <h1>Visningar</h1>

      <SearchBar
        value={query}
        onChange={setQuery}
        placeholder="Sök film"
      />

      <div className="screening-filters">
        <div className="filter-select-wrapper">
          <span className="filter-label">
            {dayFilter || "Dag"}
          </span>

          <img
            src={iconChevronDown}
            alt=""
            className="filter-chevron"
          />

          <select
            className="filter-select"
            value={dayFilter}
            onChange={(e) => setDayFilter(e.target.value)}
          >
            <option value="">Dag</option>

            {uniqueDays.map((day) => (
              <option key={day} value={day}>
                {day}
              </option>
            ))}
          </select>
        </div>

        <div className="filter-select-wrapper">
          <span className="filter-label">
            {auditoriumFilter || "Salong"}
          </span>

          <img
            src={iconChevronDown}
            alt=""
            className="filter-chevron"
          />

          <select
            className="filter-select"
            value={auditoriumFilter}
            onChange={(e) => setAuditoriumFilter(e.target.value)}
          >
            <option value="">Salong</option>

            {uniqueAuditoriums.map((name) => (
              <option key={name} value={name}>
                {name}
              </option>
            ))}
          </select>
        </div>
      </div>

      <ListToolbar
        addLabel={canManageScreenings ? "Lägg till visning" : undefined}
        onAdd={
          canManageScreenings
            ? () => setShowAddForm(true)
            : undefined
        }
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && (
        <div className="card-grid">
          {pagedScreenings.map((screening) => (
            <ListCard
              key={screening.screeningId}
              title={screening.movieTitle}
              statusLabel={
                screening.isCancelled ? "AVBOKAD" : undefined
              }
              inactiveStyle={screening.isCancelled}
              showActions={
                canManageScreenings && !screening.isCancelled
              }
              infoLines={[
                `${formatDay(screening.startTime)}, ${formatTime(
                  screening.startTime,
                )}`,
                screening.auditoriumName,
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