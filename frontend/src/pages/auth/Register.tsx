import { useState, type SubmitEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../context/useAuth";
import Message from "../../components/Message";
import "./Auth.css";

function Register() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [error, setError] = useState("");
  const { register } = useAuth();
  const navigate = useNavigate();

  async function handleSubmit(e: SubmitEvent<HTMLFormElement>) {
    e.preventDefault();
    setError("");
    if (password !== confirmPassword) {
      setError("Lösenorden måste matcha.");
      return;
    }
    try {
      await register(email, password, confirmPassword);
      navigate("/");
    } catch (err) {
      setError(
        err instanceof Error ? err.message : "Kunde inte registrera kontot.",
      );
    }
  }

  return (
    <div className="page auth-page">
      <p className="auth-welcome">Välkommen till</p>
      <p className="auth-title">ScreamTime</p>
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
        <div className="auth-field">
          <label className="auth-label" htmlFor="repeat-password">
            Upprepa lösenord
          </label>
          <input
            className="auth-input"
            type="password"
            id="repeat-password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
          />
        </div>
        {error && <Message type="error">{error}</Message>}
        <div className="auth-button-row">
          <button type="submit" className="btn-pill">
            Registrera
          </button>
        </div>
        <p className="auth-switch">
          <Link to="/login" className="auth-switch-link">
            Eller logga in här
          </Link>
        </p>
      </form>
    </div>
  );
}

export default Register;
