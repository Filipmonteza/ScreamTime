import { useEffect, useState, type SubmitEvent } from "react";
import Form from "../../components/Form";
import type { Genre } from "../../types/genre";
import type { Movie } from "../../types/movie";
import { getAllGenres } from "../../api/genresApi";
import { createMovie, updateMovie } from "../../api/moviesApi";
import Message from "../../components/Message";

type MovieFormProps = {
  movie?: Movie;
  onClose: () => void;
  onSaved: (title: string) => void;
};

function MovieForm({ movie, onClose, onSaved }: MovieFormProps) {
  const [genres, setGenres] = useState<Genre[]>([]);
  const [title, setTitle] = useState(movie?.title ?? "");
  const [selectedGenreIds, setSelectedGenreIds] = useState<string[]>([]);
  const [ageRating, setAgeRating] = useState(movie?.ageRating ?? 0);
  const [runtimeMinutes, setRuntimeMinutes] = useState(
    movie?.runtimeMinutes ?? 90,
  );
  const [description, setDescription] = useState(movie?.description ?? "");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    getAllGenres().then(setGenres);
  }, []);

  const [genresAppliedTo, setGenresAppliedTo] = useState<Genre[] | null>(null);
  if (movie && genres.length > 0 && genresAppliedTo !== genres) {
    setGenresAppliedTo(genres);
    setSelectedGenreIds(
      genres
        .filter((genre) => movie.genres.includes(genre.name))
        .map((genre) => genre.id),
    );
  }

  function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);
    const dto = {
      title,
      ageRating,
      runtimeMinutes,
      description,
      genreIds: selectedGenreIds,
    };
    const request = movie
      ? updateMovie(movie.movieId, { ...dto, isActive: movie.isActive })
      : createMovie(dto);
    request
      .then(() => onSaved(title))
      .catch((err) =>
        setError(err instanceof Error ? err.message : "Något gick fel."),
      );
  }

  return (
    <Form
      title={movie ? "Redigera film" : "Lägg till film"}
      submitLabel={movie ? "Spara ändringar" : "Spara film"}
      onSubmit={handleSubmit}
      onCancel={onClose}
    >
      {error && <Message type="error">{error}</Message>}
      <div className="form-field">
        <label className="form-label">Titel</label>
        <input
          className="form-input"
          type="text"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          required
        />
      </div>

      <div className="form-field">
        <label className="form-label">Välj genre</label>
        <div className="pill-select-list">
          {genres.map((genre) => {
            const isSelected = selectedGenreIds.includes(genre.id);
            return (
              <button
                key={genre.id}
                type="button"
                className={isSelected ? "pill-select selected" : "pill-select"}
                onClick={() =>
                  setSelectedGenreIds((prev) =>
                    isSelected
                      ? prev.filter((id) => id !== genre.id)
                      : [...prev, genre.id],
                  )
                }
              >
                {genre.name}
              </button>
            );
          })}
        </div>
      </div>

      <div className="form-field">
        <label className="form-label">Åldersgräns</label>
        <select
          className="form-input"
          value={ageRating}
          onChange={(e) => setAgeRating(Number(e.target.value))}
        >
          <option value={0}>Barntillåten</option>
          <option value={7}>7 år</option>
          <option value={11}>11 år</option>
          <option value={15}>15 år</option>
        </select>
      </div>

      <div className="form-field">
        <label className="form-label">Speltid (min)</label>
        <input
          className="form-input"
          type="number"
          min={1}
          max={600}
          value={runtimeMinutes}
          onChange={(e) => setRuntimeMinutes(Number(e.target.value))}
          required
        />
      </div>

      <div className="form-field">
        <label className="form-label">Beskrivning</label>
        <textarea
          className="form-input"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          required
        />
      </div>
    </Form>
  );
}

export default MovieForm;
