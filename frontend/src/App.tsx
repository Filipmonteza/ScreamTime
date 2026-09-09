import { useAuth } from "./context/useAuth";
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
import ProtectedRoute from "./components/ProtectedRoute";

function App() {
  const { user } = useAuth();
  const role: Role = (user?.role as Role) ?? null;

  return (
    <>
      <Navbar role={role} />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/movieDetail/:id" element={<MovieDetail />} />
        <Route path="/movies" element={<ProtectedRoute roles={["Admin", "Staff"]}><Movies /></ProtectedRoute>} />
        <Route path="/auditoriums" element={<ProtectedRoute roles={["Admin", "Staff"]}><Auditoriums /></ProtectedRoute>} />
        <Route path="/bookings" element={<ProtectedRoute><Bookings /></ProtectedRoute>} />
        <Route path="/screenings" element={<ProtectedRoute roles={["Admin", "Staff"]}><Screenings /></ProtectedRoute>} />
        <Route path="/staff" element={<ProtectedRoute roles={["Admin", "Staff"]}><Staff /></ProtectedRoute>} />
        <Route path="/account" element={<ProtectedRoute><Account /></ProtectedRoute>} />
      </Routes>
    </>
  );
}

export default App;
