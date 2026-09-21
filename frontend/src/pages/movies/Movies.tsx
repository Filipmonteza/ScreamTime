import { useEffect, useMemo, useState } from "react";
import type { Movie } from "../../types/movie";
import { getAllMovies, deleteMovie } from "../../api/moviesApi";
import MovieForm from "./MovieForm";
import SearchBar from "../../components/SearchBar";
import ListToolbar from "../../components/ListToolbar";
import usePagination from "../../hooks/usePagination";
import ListCard from "../../components/ListCard";

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

  const {
    page,
    setPage,
    totalPages,
    pagedItems: pagedMovies,
  } = usePagination(filteredMovies, 12, query);

  return (
    <div className="page movies">
      <h1>Filmer</h1>

      <SearchBar value={query} onChange={setQuery} placeholder="Sök film" />

      <ListToolbar
        addLabel="Lägg till film"
        onAdd={() => setShowAddForm(true)}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && !error && filteredMovies.length === 0 && (
        <p>
          {movies.length === 0
            ? "Det finns inga filmer ännu."
            : "Inga filmer matchade sökningen."}
        </p>
      )}

      {!loading && !error && (
        <div className="card-grid">
          {pagedMovies.map((movie) => (
            <ListCard
              key={movie.movieId}
              title={movie.title}
              infoLines={[
                movie.genres.join(", "),
                ageRatingLabels[movie.ageRating] ?? movie.ageRating,
                `${movie.runtimeMinutes}min`,
              ]}
              confirming={confirmingDeleteId === movie.movieId}
              deleting={deletingId === movie.movieId}
              onEdit={() => setEditingMovie(movie)}
              onRequestDelete={() => setConfirmingDeleteId(movie.movieId)}
              onConfirmDelete={() => handleDelete(movie.movieId)}
              onCancelDelete={() => setConfirmingDeleteId(null)}
            />
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
