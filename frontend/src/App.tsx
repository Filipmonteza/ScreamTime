import { Routes, Route } from 'react-router-dom';
import Home from './pages/Home';
import Login from './pages/auth/Login';
import Register from './pages/auth/Register';
import MovieDetail from './pages/movieDetail/MovieDetail';
import Movies from './pages/movies/Movies';

function App() {
  return (
    <>
      <Routes>
        <Route path='/' element={<Home />} />
        <Route path='/login' element={<Login />} />
        <Route path='/register' element={<Register />} />
        <Route path='/movieDetail/:id' element={<MovieDetail />} />
        <Route path='/movies' element={<Movies />} />
      </Routes>
    </>
  )
}

export default App