import { useEffect, useState, type SubmitEvent } from "react";
import Form from "../../components/Form";
import type { Screening } from "../../types/screening";
import type { Movie } from "../../types/movie";
import type { Auditorium } from "../../types/auditorium";
import { getAllMovies } from "../../api/moviesApi";
import { getAllAuditoriums } from "../../api/auditoriumsApi";
import { createScreening, updateScreening } from "../../api/screeningsApi";
import { toDateTimeLocalValue } from "../../utils/dateOptions";
import Message from "../../components/Message";

type ScreeningFormProps = {
  screening?: Screening;
  onClose: () => void;
  onSaved: (title: string) => void;
};

function ScreeningForm({ screening, onClose, onSaved }: ScreeningFormProps) {
  const [movies, setMovies] = useState<Movie[]>([]);
  const [auditoriums, setAuditoriums] = useState<Auditorium[]>([]);

  const [movieId, setMovieId] = useState(screening?.movieId ?? "");
  const [auditoriumId, setAuditoriumId] = useState(
    screening?.auditoriumId ?? "",
  );
  const [startTime, setStartTime] = useState(
    screening ? toDateTimeLocalValue(screening.startTime) : "",
  );

  const [error, setError] = useState("");

  useEffect(() => {
    getAllMovies().then(setMovies);
    getAllAuditoriums().then(setAuditoriums);
  }, []);

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    setError("");

    // The input gives local wall-clock time; the API takes UTC.
    const dto = {
      movieId,
      auditoriumId,
      startTime: new Date(startTime).toISOString(),
    };

    const movieTitle = movies.find((m) => m.movieId === movieId)?.title ?? "";

    try {
      if (screening) {
        await updateScreening(screening.screeningId, dto);
      } else {
        await createScreening(dto);
      }

      onSaved(movieTitle);
    } catch (error) {
      if (error instanceof Error) {
        setError(error.message);
      } else {
        setError("Det gick inte att spara visningen.");
      }
    }
  }

  return (
    <Form
      title={screening ? "Redigera visning" : "Lägg till visning"}
      submitLabel={screening ? "Spara ändringar" : "Lägg till visning"}
      onSubmit={handleSubmit}
      onCancel={onClose}
    >
      {error && <Message type="error">{error}</Message>}

      <div className="form-field">
        <label className="form-label">Film</label>
        <select
          className="form-input"
          value={movieId}
          onChange={(e) => setMovieId(e.target.value)}
          required
        >
          <option value="" disabled>
            Välj film
          </option>

          {movies.map((movie) => (
            <option key={movie.movieId} value={movie.movieId}>
              {movie.title}
            </option>
          ))}
        </select>
      </div>

      <div className="form-field">
        <label className="form-label">Salong</label>
        <select
          className="form-input"
          value={auditoriumId}
          onChange={(e) => setAuditoriumId(e.target.value)}
          required
        >
          <option value="" disabled>
            Välj salong
          </option>

          {auditoriums.map((auditorium) => (
            <option
              key={auditorium.auditoriumId}
              value={auditorium.auditoriumId}
            >
              {auditorium.name}
            </option>
          ))}
        </select>
      </div>

      <div className="form-field">
        <label className="form-label">Datum och tid</label>
        <input
          className="form-input"
          type="datetime-local"
          value={startTime}
          onChange={(e) => setStartTime(e.target.value)}
          required
        />
      </div>
    </Form>
  );
}

export default ScreeningForm;