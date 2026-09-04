import { Link } from "react-router-dom";
import "./Auth.css";

function Login() {
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
            Password
          </label>
          <input className="auth-input" type="password" id="email" />
        </div>
        <div className="auth-button-row">
          <button type="button" className="btn-pill">
            Logga in
          </button>
        </div>
        <p className="auth-switch">
          <Link to="/register" className="auth-switch-link">
            Eller registrera här
          </Link>
        </p>
      </div>
    </div>
  );
}

export default Login;
