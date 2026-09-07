import { useEffect, useMemo, useState } from "react";
import type { Movie } from "../../types/movie";
import { getAllMovies } from "../../api/moviesApi";
import MovieForm from "./MovieForm";
import "./Movies.css";

const ageRatingLabels: Record<number, string> = {
  0: "Barntillåten",
  7: "7 år",
  11: "11 år",
  15: "15 år",
};

function Movies() {
  const [movies, setMovies] = useState<Movie[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [confirmingDeleteId, setConfirmingDeleteId] = useState<string | null>(
    null,
  );
  const [showAddForm, setShowAddForm] = useState(false);
  const [editingMovie, setEditingMovie] = useState<Movie | null>(null);

  function loadMovies() {
    getAllMovies()
      .then(setMovies)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    loadMovies();
  }, []);

  const filteredMovies = useMemo(() => {
    const q = query.trim().toLowerCase();
    return movies.filter((movie) => movie.title.toLowerCase().includes(q));
  }, [movies, query]);

  return (
    <div className="page movies">
      <h1>Filmer</h1>

      <div className="search-bar">
        <input
          className="search-bar-input"
          type="text"
          placeholder="Sök film"
          value={query}
          onChange={(e) => setQuery(e.target.value)}
        />
      </div>
      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && !error && (
        <div className="movie-list">
          {filteredMovies.map((movie) => (
            <div className="movie-list-item" key={movie.movieId}>
              <div className="movie-list-item-info">
                <h2>{movie.title}</h2>
                <p>{movie.genres.join(", ")}</p>
                <p>{ageRatingLabels[movie.ageRating] ?? movie.ageRating}</p>
                <p>{movie.runtimeMinutes}min</p>
              </div>

              {confirmingDeleteId === movie.movieId ? (
                <div className="movie-list-item-actions">
                  <span>Är du säker?</span>
                  <button
                    className="btn-pill"
                    onClick={() => setConfirmingDeleteId(null)}
                  >
                    Ja
                  </button>
                  <button
                    className="btn-pill"
                    onClick={() => setConfirmingDeleteId(null)}
                  >
                    Nej
                  </button>
                </div>
              ) : (
                <div className="movie-list-item-actions">
                  <button
                    className="btn-pill"
                    onClick={() => setEditingMovie(movie)}
                  >
                    Redigera
                  </button>
                  <button
                    className="btn-pill"
                    onClick={() => setConfirmingDeleteId(movie.movieId)}
                  >
                    Ta bort
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      <button className="btn-pill" onClick={() => setShowAddForm(true)}>
        Lägg till film
      </button>

      {showAddForm && (
        <MovieForm
          onClose={() => setShowAddForm(false)}
          onSaved={() => {
            setShowAddForm(false);
            loadMovies();
          }}
        />
      )}
      {editingMovie && (
        <MovieForm
          movie={editingMovie}
          onClose={() => setEditingMovie(null)}
          onSaved={() => {
            setEditingMovie(null);
            loadMovies();
          }}
        />
      )}
    </div>
  );
}

export default Movies;
