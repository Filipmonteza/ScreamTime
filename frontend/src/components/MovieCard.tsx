import { Link } from "react-router-dom";
import type { Movie } from "../types/movie";
import { getPosterForMovie } from "../utils/posterMap";
import "./MovieCard.css";

interface MovieCardProps {
  movie: Movie;
}

function MovieCard({ movie }: MovieCardProps) {
  return (
    <Link to={`/movieDetail/${movie.movieId}`} className="movie-card">
      <img
        className="movie-card-poster"
        src={getPosterForMovie(movie)}
        alt={movie.title}
      />
      <p className="title-movie-card">{movie.title}</p>
      <p className="genre-movie-card">{movie.genres.join(", ")}</p>
    </Link>
  );
}

export default MovieCard;
