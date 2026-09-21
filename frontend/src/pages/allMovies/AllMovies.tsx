import { useEffect, useMemo, useState } from "react";
import type { Movie } from "../../types/movie";
import { getAllMovies } from "../../api/moviesApi";
import type { Screening } from "../../types/screening";
import { getAllScreenings } from "../../api/screeningsApi";
import "./AllMovies.css";
import MovieCard from "../../components/MovieCard";
import SearchBar from "../../components/SearchBar";
import Pagination from "../../components/Pagination";
import usePagination from "../../hooks/usePagination";
import FilterSelect from "../../components/FilterSelect";
import {
  dateKeyFromIso,
  nextDays,
  formatDateOptionLabel,
} from "../../utils/dateOptions";

const PAGE_SIZE = 10;
const dayOptions = nextDays(5);

function AllMovies() {
  const [movies, setMovies] = useState<Movie[]>([]);
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
      .then(setMovies)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => {
    getAllScreenings()
      .then(setScreenings)
      .catch((err) => setScreeningsError(err.message));
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
    <div className="page all-movies">
      <h1>Alla filmer</h1>
      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && !error && (
        <>
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
                  <MovieCard key={movie.movieId} movie={movie} />
                ))}
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}

export default AllMovies;
