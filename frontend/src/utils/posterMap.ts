import alien from "../assets/posters/Alien.png";
import allaHelgonsBlodigaNatt from "../assets/posters/AllaHelgonsBlodigaNatt.png";
import casper from "../assets/posters/Casper.png";
import exorcisten from "../assets/posters/Exorcisten.png";
import hereditary from "../assets/posters/Hereditary.png";
import psycho from "../assets/posters/Psycho.png";
import sinister from "../assets/posters/Sinister.png";
import theConjuring from "../assets/posters/TheConjuring.png";
import theShining from "../assets/posters/TheShining.png";
import theThing from "../assets/posters/TheThing.png";
import placeholderNull from "../assets/posters/Placeholder_Null.png";
import placeholderPsychological from "../assets/posters/Placeholder_Psychological.png";
import placeholderSciFi from "../assets/posters/Placeholder_Sci-Fi.png";
import placeholderSlasher from "../assets/posters/Placeholder_Slasher.png";
import placeholderSupernatural from "../assets/posters/Placeholder_Supernatural.png";
import type { Movie } from "../types/movie";

const postersByFileName: Record<string, string> = {
    "Alien.png": alien,
    "AllaHelgonsBlodigaNatt.png": allaHelgonsBlodigaNatt,
    "Casper.png": casper,
    "Exorcisten.png": exorcisten,
    "Hereditary.png": hereditary,
    "Psycho.png": psycho,
    "Sinister.png": sinister,
    "TheConjuring.png": theConjuring,
    "TheShining.png": theShining,
    "TheThing.png": theThing,
};

const placeholderByGenre: Record<string, string> = {
    "Psychological": placeholderPsychological,
    "Sci-Fi": placeholderSciFi,
    "Slasher": placeholderSlasher,
    "Supernatural": placeholderSupernatural,
};

export function getPosterForMovie(movie: Movie): string {
    if (movie.poster && postersByFileName[movie.poster]) {
        return postersByFileName[movie.poster];
    }

    const genre = movie.genres.find((g) => placeholderByGenre[g]);
    return genre ? placeholderByGenre[genre] : placeholderNull;
}