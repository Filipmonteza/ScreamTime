import { useState, type SubmitEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../context/useAuth";
import Message from "../../components/Message";
import "./Auth.css";

function Login() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const { login } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    setError("");
    try {
      await login(email, password);
      navigate("/");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Fel email eller lösenord");
    }
  }

  return (
    <div className="page auth-page">
      <p className="auth-welcome">Välkommen till</p>
      <p className="auth-title">ScreenTime</p>
      <form className="auth-card" onSubmit={handleSubmit}>
        <div className="auth-field">
          <label className="auth-label" htmlFor="email">
            Email
          </label>
          <input
            className="auth-input"
            type="email"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </div>
        <div className="auth-field">
          <label className="auth-label" htmlFor="password">
            Lösenord
          </label>
          <input
            className="auth-input"
            type="password"
            id="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </div>
        {error && <Message type="error">{error}</Message>}
        <div className="auth-button-row">
          <button type="submit" className="btn-pill">
            Logga in
          </button>
        </div>
        <p className="auth-switch">
          <Link to="/register" className="auth-switch-link">
            Eller registrera här
          </Link>
        </p>
      </form>
    </div>
  );
}

export default Login;
