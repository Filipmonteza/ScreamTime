import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import type { Movie } from "../types/movie";
import { getAllMovies } from "../api/moviesApi";
import type { Screening } from "../types/screening";
import { getAllScreenings } from "../api/screeningsApi";
import "./Home.css";
import { getPosterForMovie } from "../utils/posterMap";
import SearchBar from "../components/SearchBar";
import Pagination from "../components/Pagination";
import usePagination from "../hooks/usePagination";
import FilterSelect from "../components/FilterSelect";
import {
  dateKeyFromIso,
  nextDays,
  formatDateOptionLabel,
} from "../utils/dateOptions";

const PAGE_SIZE = 5;
const dayOptions = nextDays(5);

function shuffle<T>(arr: T[]): T[] {
  const copy = [...arr];
  for (let i = copy.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [copy[i], copy[j]] = [copy[j], copy[i]];
  }
  return copy;
}

function Home() {
  const [movies, setMovies] = useState<Movie[]>([]);
  const [tipMovies, setTipMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [dayFilter, setDayFilter] = useState("");
  const [genreFilter, setGenreFilter] = useState("");
  const [auditoriumFilter, setAuditoriumFilter] = useState("");
  const [screeningsError, setScreeningsError] = useState<string | null>(null);
  const [screenings, setScreenings] = useState<Screening[]>([]);

  useEffect(() => {
    getAllMovies()
      .then((data) => {
        setMovies(data);
        setTipMovies(shuffle(data).slice(0, 6));
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  const filteredMovies = useMemo(() => {
    const q = query.trim().toLowerCase();
    return movies.filter((movie) => {
      const matchesQuery = !q || movie.title.toLowerCase().includes(q);
      const matchesGenre = !genreFilter || movie.genres.includes(genreFilter);
      const matchesDay =
        !dayFilter ||
        screenings.some(
          (s) =>
            s.movieId === movie.movieId &&
            dateKeyFromIso(s.startTime) === dayFilter,
        );
      const matchesAuditorium =
        !auditoriumFilter ||
        screenings.some(
          (s) =>
            s.movieId === movie.movieId &&
            s.auditoriumName === auditoriumFilter,
        );
      return matchesQuery && matchesGenre && matchesDay && matchesAuditorium;
    });
  }, [movies, screenings, query, genreFilter, dayFilter, auditoriumFilter]);

  useEffect(() => {
    getAllScreenings()
      .then(setScreenings)
      .catch((err) => setScreeningsError(err.message));
  }, []);
  const auditoriumOptions = useMemo(() => {
    const set = new Set<string>();
    screenings.forEach((s) => set.add(s.auditoriumName));
    return Array.from(set).sort();
  }, [screenings]);

  const genreOptions = useMemo(() => {
    const set = new Set<string>();
    movies.forEach((movie) => movie.genres.forEach((genre) => set.add(genre)));
    return Array.from(set).sort();
  }, [movies]);

  const filterKey = `${query}__${genreFilter}__${dayFilter}__${auditoriumFilter}`;
  const {
    page,
    setPage,
    totalPages,
    pagedItems: pagedMovies,
  } = usePagination(filteredMovies, PAGE_SIZE, filterKey);

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
                  <Link
                    to={`/movieDetail/${movie.movieId}`}
                    className="movie-card"
                    key={movie.movieId}
                  >
                    <img
                      className="movie-card-poster"
                      src={getPosterForMovie(movie)}
                      alt={movie.title}
                    />
                    <p className="title-movie-card">{movie.title}</p>
                    <p className="genre-movie-card">
                      {movie.genres.join(", ")}
                    </p>
                  </Link>
                ))}
              </div>
            )}
          </div>
          <div className="find-section">
            <h2>Hitta film</h2>
            {screeningsError && <p>Fel: {screeningsError}</p>}
            <SearchBar
              value={query}
              onChange={setQuery}
              placeholder="Sök efter film..."
            />
            <div className="toolbar">
              <div className="filter-row">
                <FilterSelect
                  value={dayFilter}
                  onChange={setDayFilter}
                  options={dayOptions.map((day, index) => ({
                    value: day,
                    label: formatDateOptionLabel(day, index),
                  }))}
                  placeholder="Dag"
                />
                <FilterSelect
                  value={genreFilter}
                  onChange={setGenreFilter}
                  options={genreOptions.map((genre) => ({
                    value: genre,
                    label: genre,
                  }))}
                  placeholder="Alla genrer"
                />
                <FilterSelect
                  value={auditoriumFilter}
                  onChange={setAuditoriumFilter}
                  options={auditoriumOptions.map((room) => ({
                    value: room,
                    label: room,
                  }))}
                  placeholder="Salong"
                />
              </div>
              <Pagination
                page={page}
                totalPages={totalPages}
                onPageChange={setPage}
              />
            </div>
            <div>
              {pagedMovies.length === 0 && <p>Inga filmer kunde hittas</p>}

              {pagedMovies.length > 0 && (
                <div className="movie-list">
                  {pagedMovies.map((movie) => (
                    <Link
                      to={`/movieDetail/${movie.movieId}`}
                      className="movie-card"
                      key={movie.movieId}
                    >
                      <img
                        className="movie-card-poster"
                        src={getPosterForMovie(movie)}
                        alt={movie.title}
                      />
                      <p className="title-movie-card">{movie.title}</p>
                      <p className="genre-movie-card">
                        {movie.genres.join(", ")}
                      </p>
                    </Link>
                  ))}
                </div>
              )}
            </div>
          </div>
        </>
      )}
    </div>
  );
}

export default Home;
