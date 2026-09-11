import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import type { Movie } from "../types/movie";
import { getAllMovies } from "../api/moviesApi";
import "./Home.css";
import { getPosterForMovie } from "../utils/posterMap";
import SearchBar from "../components/SearchBar";
import Pagination from "../components/Pagination";
import usePagination from "../hooks/usePagination";
import iconChevronDown from "./../assets/icons/Icon_Chevron_Down.svg";

/* TODO: dayOptions och auditoriumOptions är hårdkodade platshållare, så detta måste vi lösa. */
const dayOptions = ["Måndag", "Tisdag"];
const auditoriumOptions = ["Salong 1", "Salong 2"];
const PAGE_SIZE = 5;

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
      return matchesQuery && matchesGenre;
    });
  }, [movies, query, genreFilter]);

  const genreOptions = useMemo(() => {
    const set = new Set<string>();
    movies.forEach((movie) => movie.genres.forEach((genre) => set.add(genre)));
    return Array.from(set).sort();
  }, [movies]);

  const filterKey = `${query}__${genreFilter}`;
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
            <SearchBar
              value={query}
              onChange={setQuery}
              placeholder="Sök efter film..."
            />
            <div className="toolbar">
              <div className="filter-row">
                <div className="filter-select-wrapper">
                  <span className="filter-label">{dayFilter || "Dag"}</span>
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
                    {dayOptions.map((day) => (
                      <option key={day} value={day}>
                        {day}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="filter-select-wrapper">
                  <span className="filter-label">{genreFilter || "Genre"}</span>
                  <img
                    src={iconChevronDown}
                    alt=""
                    className="filter-chevron"
                  />
                  <select
                    className="filter-select"
                    value={genreFilter}
                    onChange={(e) => setGenreFilter(e.target.value)}
                  >
                    <option value="">Genre</option>
                    {genreOptions.map((genre) => (
                      <option key={genre} value={genre}>
                        {genre}
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
                    {auditoriumOptions.map((room) => (
                      <option key={room} value={room}>
                        {room}
                      </option>
                    ))}
                  </select>
                </div>
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
