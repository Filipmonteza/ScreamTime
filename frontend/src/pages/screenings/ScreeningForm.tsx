import { useEffect, useState, type SubmitEvent } from "react";
import Form from "../../components/Form";
import type { Screening } from "../../types/screening";
import type { Movie } from "../../types/movie";
import type { Auditorium } from "../../types/auditorium";
import { getAllMovies } from "../../api/moviesApi";
import { getAllAuditoriums } from "../../api/auditoriumsApi";
import { createScreening, updateScreening } from "../../api/screeningsApi";

type ScreeningFormProps = {
  screening?: Screening;
  onClose: () => void;
  onSaved: () => void;
};

function ScreeningForm({ screening, onClose, onSaved }: ScreeningFormProps) {
  const [movies, setMovies] = useState<Movie[]>([]);
  const [auditoriums, setAuditoriums] = useState<Auditorium[]>([]);

  const [movieId, setMovieId] = useState(screening?.movieId ?? "");
  const [auditoriumId, setAuditoriumId] = useState(screening?.auditoriumId ?? "");
  const [startTime, setStartTime] = useState(
    screening?.startTime.slice(0, 16) ?? "",
  );

  useEffect(() => {
    getAllMovies().then(setMovies);
    getAllAuditoriums().then(setAuditoriums);
  }, []);

  function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    const dto = { movieId, auditoriumId, startTime };
    const request = screening
      ? updateScreening(screening.screeningId, dto)
      : createScreening(dto);
    request.then(onSaved);
  }

  return (
    <Form
      title={screening ? "Redigera visning" : "Lägg till visning"}
      submitLabel={screening ? "Spara ändringar" : "Lägg till visning"}
      onSubmit={handleSubmit}
      onCancel={onClose}
    >
      <div className="form-field">
        <label className="form-label">Film</label>
        <select
          className="form-input"
          value={movieId}
          onChange={(e) => setMovieId(e.target.value)}
          required
        >
          <option value="" disabled>Välj film</option>
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
          <option value="" disabled>Välj salong</option>
          {auditoriums.map((auditorium) => (
            <option key={auditorium.auditoriumId} value={auditorium.auditoriumId}>
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