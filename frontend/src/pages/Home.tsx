import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import type { Movie } from "../types/movie";
import { getAllMovies } from "../api/moviesApi";
import type { Screening } from "../types/screening";
import { getAllScreenings } from "../api/screeningsApi";
import "./Home.css";
import MovieCard from "../components/MovieCard";
import { todayKey } from "../utils/dateOptions";

function shuffle<T>(arr: T[]): T[] {
  const copy = [...arr];
  for (let i = copy.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [copy[i], copy[j]] = [copy[j], copy[i]];
  }
  return copy;
}

function formatTime(dateStr: string): string {
  return new Date(dateStr).toLocaleTimeString("sv-SE", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

type AuditoriumProgramme = {
  auditoriumId: string;
  name: string;
  screenings: Screening[];
};

function Home() {
  const navigate = useNavigate();
  const [tipMovies, setTipMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [todaysScreenings, setTodaysScreenings] = useState<Screening[]>([]);
  const [screeningsLoading, setScreeningsLoading] = useState(true);
  const [screeningsError, setScreeningsError] = useState<string | null>(null);

  useEffect(() => {
    getAllMovies()
      .then((data) => setTipMovies(shuffle(data).slice(0, 6)))
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    getAllScreenings({ date: todayKey() })
      .then(setTodaysScreenings)
      .catch((err) => setScreeningsError(err.message))
      .finally(() => setScreeningsLoading(false));
  }, []);

  // One group per auditorium, in name order; the API already returns each
  // auditorium's screenings in start-time order. Screenings that have already
  // started are dropped the same way MovieDetail does it.
  const programme = useMemo<AuditoriumProgramme[]>(() => {
    const now = new Date();
    const groups = new Map<string, AuditoriumProgramme>();
    todaysScreenings
      .filter((s) => new Date(s.startTime) > now)
      .forEach((s) => {
        const group = groups.get(s.auditoriumId) ?? {
          auditoriumId: s.auditoriumId,
          name: s.auditoriumName,
          screenings: [],
        };
        group.screenings.push(s);
        groups.set(s.auditoriumId, group);
      });
    return [...groups.values()].sort((a, b) =>
      a.name.localeCompare(b.name, "sv"),
    );
  }, [todaysScreenings]);

  return (
    <div className="page home">
      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && !error && (
        <>
          <div className="tips-section">
            <h2>Våra tips</h2>
            {tipMovies.length > 0 && (
              <div className="movie-row">
                {tipMovies.map((movie) => (
                  <MovieCard key={movie.movieId} movie={movie} />
                ))}
              </div>
            )}
          </div>
          <div className="today-section">
            <h2>Dagens visningar</h2>
            {screeningsLoading && <p>Laddar...</p>}
            {screeningsError && <p>Fel: {screeningsError}</p>}

            {!screeningsLoading && !screeningsError && programme.length === 0 && (
              <p>Inga visningar idag</p>
            )}

            {programme.length > 0 && (
              <div className="today-auditoriums">
                {programme.map((auditorium) => (
                  <div
                    className="today-auditorium"
                    key={auditorium.auditoriumId}
                  >
                    <h3>{auditorium.name}</h3>
                    <div className="today-showtimes">
                      {auditorium.screenings.map((screening) => {
                        const seatsLeft =
                          screening.capacity - screening.bookedSeats;
                        return (
                          <div
                            className="today-showtime"
                            key={screening.screeningId}
                          >
                            <span className="today-showtime-time">
                              {formatTime(screening.startTime)}
                            </span>
                            <div className="today-showtime-info">
                              <Link
                                to={`/movieDetail/${screening.movieId}`}
                                className="today-showtime-title"
                              >
                                {screening.movieTitle}
                              </Link>
                              <span className="today-showtime-seats">
                                {seatsLeft} platser kvar
                              </span>
                            </div>
                            <button
                              className="btn-pill"
                              disabled={seatsLeft <= 0}
                              onClick={() =>
                                navigate(`/booking/${screening.screeningId}`)
                              }
                            >
                              {seatsLeft <= 0 ? "Fullbokad" : "Biljetter"}
                            </button>
                          </div>
                        );
                      })}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}

export default Home;
