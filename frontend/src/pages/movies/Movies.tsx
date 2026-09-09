import { useEffect, useMemo, useState } from "react";
import type { Movie } from "../../types/movie";
import { getAllMovies, deleteMovie } from "../../api/moviesApi";
import MovieForm from "./MovieForm";
import "./Movies.css";
import iconChevronLeft from "../../assets/icons/Icon_Chevron_Left.svg";
import iconChevronLeftDouble from "../../assets/icons/Icon_Chevron_Left_Double.svg";
import iconChevronRight from "../../assets/icons/Icon_Chevron_Right.svg";
import iconChevronRightDouble from "../../assets/icons/Icon_Chevron_Right_Double.svg";

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
  const [deletingId, setDeletingId] = useState<string | null>(null);

  function loadMovies() {
    getAllMovies()
      .then((data) => {
        setError(null);
        setMovies(data);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    loadMovies();
  }, []);

  function handleDelete(id: string) {
    setError(null);
    setDeletingId(id);
    deleteMovie(id)
      .then(() => {
        setConfirmingDeleteId(null);
        loadMovies();
      })
      .catch((err) => {
        setConfirmingDeleteId(null);
        setError(err.message);
      })
      .finally(() => setDeletingId(null));
  }

  const filteredMovies = useMemo(() => {
    const q = query.trim().toLowerCase();
    return movies.filter((movie) => movie.title.toLowerCase().includes(q));
  }, [movies, query]);

  const PAGE_SIZE = 12;

  const [page, setPage] = useState(1);

  const totalPages = Math.max(1, Math.ceil(filteredMovies.length / PAGE_SIZE));

  if (page > totalPages) {
    setPage(totalPages);
  }

  const pagedMovies = useMemo(() => {
    const start = (page - 1) * PAGE_SIZE;
    return filteredMovies.slice(start, start + PAGE_SIZE);
  }, [filteredMovies, page]);

  const [prevQuery, setPrevQuery] = useState(query);
  if (query !== prevQuery) {
    setPrevQuery(query);
    setPage(1);
  }

  const pageNumbers = useMemo(() => {
    let start = page - 1;
    let end = page + 1;
    if (start < 1) {
      end += 1 - start;
      start = 1;
    }
    if (end > totalPages) {
      start -= end - totalPages;
      end = totalPages;
    }
    start = Math.max(1, start);

    const numbers: number[] = [];
    for (let n = start; n <= end; n++) numbers.push(n);
    return numbers;
  }, [page, totalPages]);

  function goToFirst() {
    setPage(1);
  }
  function goToPrev() {
    setPage((p) => Math.max(1, p - 1));
  }
  function goToNext() {
    setPage((p) => Math.min(totalPages, p + 1));
  }
  function goToLast() {
    setPage(totalPages);
  }

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

      <div className="movies-toolbar">
        <button className="btn-pill" onClick={() => setShowAddForm(true)}>
          Lägg till film
        </button>

        <div className="pagination">
          <button onClick={goToFirst} disabled={page === 1}>
            <img src={iconChevronLeftDouble} alt="Första sidan" />
          </button>
          <button onClick={goToPrev} disabled={page === 1}>
            <img src={iconChevronLeft} alt="Föregående sida" />
          </button>
          {pageNumbers.map((n) => (
            <button
              key={n}
              className={n === page ? "page-number active" : "page-number"}
              onClick={() => setPage(n)}
            >
              {n}
            </button>
          ))}
          <button onClick={goToNext} disabled={page === totalPages}>
            <img src={iconChevronRight} alt="Nästa sida" />
          </button>
          <button onClick={goToLast} disabled={page === totalPages}>
            <img src={iconChevronRightDouble} alt="Sista sidan" />
          </button>
        </div>
      </div>

      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && !error && (
        <div className="movie-list">
          {pagedMovies.map((movie) => (
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
                    onClick={() => handleDelete(movie.movieId)}
                    disabled={deletingId === movie.movieId}
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
