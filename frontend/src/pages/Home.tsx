import { useEffect, useMemo, useState } from "react";
import type { Movie } from "../types/movie";
import { getAllMovies } from "../api/moviesApi";
import "./Home.css";
import searchIcon from "../assets/icons/Icon_Search.svg";

function Home() {
  const [movies, setMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [query, setQuery] = useState("");

  useEffect(() => {
    getAllMovies()
      .then(setMovies)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, []);

  const filteredMovies = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return movies;
    return movies.filter(
      (movie) =>
        movie.title.toLowerCase().includes(q) ||
        movie.genres.some((genre) => genre.toLowerCase().includes(q)),
    );
  }, [movies, query]);

  return (
    <div className="page home">
      <h2>Hitta film</h2>
      <div className="search-bar">
        <img src={searchIcon} alt="Search icon" className="search-icon" />
        <input className="search-bar-input"
          type="text"
          placeholder="Sök efter film..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
        />
      </div>

      <div>
        <h2>Våra tips</h2>
        {loading && <p>Laddar...</p>}
        {error && <p>Fel: {error}</p>}
        {!loading && !error && filteredMovies.length === 0 && (
          <p>Inga filmer kunde hittas</p>
        )}

        {!loading && !error && filteredMovies.length > 0 && (
          <div className="movie-row">
            {filteredMovies.map((movie) => (
              <div className="movie-card" key={movie.movieId}>
                <div className="movie-card-poster" />
                <p className="title-movie-card">{movie.title}</p>
                <p className="genre-movie-card">{movie.genres.join(", ")}</p>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

export default Home;
