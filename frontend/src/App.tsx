import { Routes, Route } from "react-router-dom";
import Navbar, { type Role } from "./components/Navbar";
import Home from "./pages/Home";
import Login from "./pages/auth/Login";
import Register from "./pages/auth/Register";
import MovieDetail from "./pages/movieDetail/MovieDetail";
import Movies from "./pages/movies/Movies";
import Auditoriums from "./pages/auditoriums/Auditoriums";
import Bookings from "./pages/bookings/Bookings";
import Screenings from "./pages/screenings/Screenings";
import Staff from "./pages/staff/Staff";
import Account from "./pages/account/Account";

function App() {
  // TODO: replace role with real auth state once login is implemented
  const role: Role = "Admin";
  return (
    <>
      <Navbar role={role} />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/movieDetail/:id" element={<MovieDetail />} />
        <Route path="/movies" element={<Movies />} />
        <Route path="/auditoriums" element={<Auditoriums />} />
        <Route path="/bookings" element={<Bookings />} />
        <Route path="/screenings" element={<Screenings />} />
        <Route path="/staff" element={<Staff />} />
        <Route path="/account" element={<Account />} />
      </Routes>
    </>
  );
}

export default App;
