import { Routes, Route } from 'react-router-dom';
import Home from './pages/Home';
import Login from './pages/auth/Login';
import MovieDetail from './pages/movieDetail/MovieDetail';

function App() {
  return (
    <>
      <Routes>
        <Route path='/' element={<Home />} />
        <Route path='/login' element={<Login />} />
        <Route path='/movieDetail/:id' element={<MovieDetail />} />
      </Routes>
    </>
  )
}

export default App