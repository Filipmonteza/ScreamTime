import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { Movie } from "../../types/movie";
import { getMovieById } from "../../api/moviesApi";
import "./MovieDetail.css";
import type { Screening } from "../../types/screening";
import { getAllScreenings } from "../../api/screeningApi";
import { getPosterForMovie } from "../../utils/posterMap";
import FilterSelect from "../../components/FilterSelect";
import iconArrowBack from "../../assets/icons/Icon_Arrow_Back.svg";

const ageRatingLabels: Record<number, string> = {
  0: "Barntillåten",
  7: "7 år",
  11: "11 år",
  15: "15 år",
};

function formatWeekdayLabel(dateStr: string): string {
  const weekday = new Date(dateStr).toLocaleDateString("sv-SE", {
    weekday: "long",
  });
  return weekday.charAt(0).toUpperCase() + weekday.slice(1);
}

function formatDateKey(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function nextDays(count: number): string[] {
  const today = new Date();
  return Array.from({ length: count }, (_, i) => {
    const date = new Date(
      today.getFullYear(),
      today.getMonth(),
      today.getDate() + i,
    );
    return formatDateKey(date);
  });
}

function formatDateOptionLabel(dateStr: string, index: number): string {
  if (index === 0) return "Idag";
  if (index === 1) return "Imorgon";
  const date = new Date(dateStr);
  const weekday = date.toLocaleDateString("sv-SE", { weekday: "long" });
  const dayMonth = date.toLocaleDateString("sv-SE", {
    day: "numeric",
    month: "long",
  });
  const capitalizedWeekday = weekday.charAt(0).toUpperCase() + weekday.slice(1);
  return `${capitalizedWeekday} ${dayMonth}`;
}

function MovieDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [movie, setMovie] = useState<Movie | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    getMovieById(id)
      .then(setMovie)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [id]);

  const [screenings, setScreenings] = useState<Screening[]>([]);

  useEffect(() => {
    getAllScreenings()
      .then(setScreenings)
      .catch((err) => setError(err.message));
  }, []);

  const movieScreenings = movie
    ? screenings.filter(
        (s) =>
          s.movieTitle === movie.title && new Date(s.startTime) > new Date(),
      )
    : [];

  const [selectedDate, setSelectedDate] = useState<string | null>(null);
  const [selectedAuditorium, setSelectedAuditorium] = useState<string>("");

  const dateKey = (iso: string) => iso.slice(0, 10);

  const availableDates = nextDays(5);

  const availableAuditoriums = [
    ...new Set(movieScreenings.map((s) => s.auditoriumName)),
  ].sort();

  const activeDate = selectedDate ?? "";

  const visibleDates = activeDate === "" ? availableDates : [activeDate];

  const screeningsByDate = visibleDates.map((date) => ({
    date,
    screenings: movieScreenings
      .filter((s) => dateKey(s.startTime) === date)
      .filter(
        (s) =>
          selectedAuditorium === "" || s.auditoriumName === selectedAuditorium,
      )
      .sort((a, b) => a.startTime.localeCompare(b.startTime)),
  }));

  return (
    <div className="page movie-detail">
      <button className="movie-detail-back" onClick={() => navigate(-1)}>
        <img
          className="movie-detail-back-icon"
          src={iconArrowBack}
          alt="Tillbaka"
        />
      </button>
      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && !error && movie && (
        <div className="movie-detail-content">
          <img
            className="movie-detail-poster"
            src={getPosterForMovie(movie)}
            alt={movie.title}
          />
          <div className="movie-detail-body">
            <div className="movie-detail-info">
              <h1>{movie.title}</h1>
              <p className="movie-detail-meta">
                {ageRatingLabels[movie.ageRating] ?? movie.ageRating} |{" "}
                {movie.runtimeMinutes} min | {movie.genres.join(", ")}
              </p>
              <p className="movie-detail-description">{movie.description}</p>
            </div>
            <div className="movie-detail-tickets">
              <h3>Biljetter</h3>
              <div className="filter-row">
                <FilterSelect
                  value={activeDate}
                  onChange={setSelectedDate}
                  options={availableDates.map((date, index) => ({
                    value: date,
                    label: formatDateOptionLabel(date, index),
                  }))}
                  placeholder="Alla dagar"
                />
                <FilterSelect
                  value={selectedAuditorium}
                  onChange={setSelectedAuditorium}
                  options={availableAuditoriums.map((name) => ({
                    value: name,
                    label: name,
                  }))}
                  placeholder="Alla salonger"
                />
              </div>
              <div className="movie-detail-tickets-list">
                {screeningsByDate.map(
                  ({ date, screenings }) =>
                    screenings.length > 0 && (
                      <div key={date}>
                        <h3 className="movie-detail-date-heading">
                          {formatWeekdayLabel(date)}
                        </h3>
                        {screenings.map((screening) => (
                          <div
                            key={screening.screeningId}
                            className="movie-detail-showtime"
                          >
                            <span className="movie-detail-showtime-time">
                              {new Date(screening.startTime).toLocaleTimeString(
                                "sv-SE",
                                {
                                  hour: "2-digit",
                                  minute: "2-digit",
                                },
                              )}
                            </span>
                            <div className="movie-detail-showtime-info">
                              <strong>{screening.auditoriumName}</strong>
                              <span>231 platser | 12 platser kvar</span>
                            </div>
                            <button
                              className="btn-pill"
                              onClick={() =>
                                navigate(`/booking/${screening.screeningId}`)
                              }
                            >
                              Biljetter
                            </button>
                          </div>
                        ))}
                      </div>
                    ),
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default MovieDetail;
