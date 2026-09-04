import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import type { Movie } from "../../types/movie";
import { getMovieById } from "../../api/moviesApi";
import "./MovieDetail.css";
import iconArrowBack from "../../assets/icons/Icon_Arrow_Back.svg";

const ageRatingLabels: Record<number, string> = {
  0: "Barntillåten",
  7: "7 år",
  11: "11 år",
  15: "15 år",
};

function MovieDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [movie, setMovie] = useState<Movie | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!id) return;
    getMovieById(id)
      .then(setMovie)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }, [id]);

  return (
    <div className="page movie-detail">
      <button className="movie-detail-back" onClick={() => navigate(-1)}>
        <img className="movie-detail-back-icon" src={iconArrowBack} alt="Tillbaka" />
      </button>
      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && !error && movie && (
        <div className="movie-detail-content">
          <div className="movie-detail-poster" />
          <div className="movie-detail-info">
            <h1>{movie.title}</h1>
            <p className="movie-detail-meta">
              {ageRatingLabels[movie.ageRating] ?? movie.ageRating} |{" "}
              {movie.runtimeMinutes} min | {movie.genres.join(", ")}
            </p>
            <p className="movie-detail-description">{movie.description}</p>
          </div>
        </div>
      )}
    </div>
  );
}

export default MovieDetail;
