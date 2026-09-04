import { Link } from "react-router-dom";
import "./Auth.css";

function Register() {
  return (
    <div className="page auth-page">
      <p className="auth-welcome">Välkommen till</p>
      <p className="auth-title">ScreenTime</p>
      <div className="auth-card">
        <div className="auth-field">
          <label className="auth-label" htmlFor="email">
            Email
          </label>
          <input className="auth-input" type="email" id="email" />
        </div>
        <div className="auth-field">
          <label className="auth-label" htmlFor="password">
            Lösenord
          </label>
          <input className="auth-input" type="password" id="password" />
        </div>
        <div className="auth-field">
          <label className="auth-label" htmlFor="repeat-password">
            Upprepa lösenord
          </label>
          <input className="auth-input" type="password" id="repeat-password" />
        </div>
        <div className="auth-field">
          <label className="auth-label" htmlFor="birthdate">
            Födelsedatum
          </label>
          <input className="auth-input" type="date" id="birthdate" />
        </div>
        <div className="auth-button-row">
          <button type="button" className="btn-pill">
            Registrera
          </button>
        </div>
        <p className="auth-switch">
          <Link to="/login" className="auth-switch-link">
            Eller logga in här
          </Link>
        </p>
      </div>
    </div>
  );
}

export default Register;