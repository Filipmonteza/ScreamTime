import { useState } from "react"
import { useAuth } from "../../context/useAuth"
import { updateEmail, updatePassword } from "../../api/accountApi"
import "../auth/Auth.css"
import "./Account.css"

function Account() {
    const { user, updateAuth } = useAuth()

    const [editingEmail, setEditingEmail] = useState(false)
    const [newEmail, setNewEmail] = useState("")
    const [emailPassword, setEmailPassword] = useState("")
    const [emailError, setEmailError] = useState<string | null>(null)

    const [editingPassword, setEditingPassword] = useState(false)
    const [currentPassword, setCurrentPassword] = useState("")
    const [newPassword, setNewPassword] = useState("")
    const [passwordError, setPasswordError] = useState<string | null>(null)
    const [passwordSuccess, setPasswordSuccess] = useState(false)

    if (!user) {
        return <div className="page auth-page">Du måste vara inloggad.</div>
    }

    async function handleEmailSubmit(e: React.FormEvent) {
        e.preventDefault()
        setEmailError(null)
        try {
            const result = await updateEmail(newEmail, emailPassword)
            updateAuth(result)
            setEditingEmail(false)
            setNewEmail("")
            setEmailPassword("")
        } catch (err) {
            setEmailError(err instanceof Error ? err.message : "Något gick fel.")
        }
    }

    async function handlePasswordSubmit(e: React.FormEvent) {
        e.preventDefault()
        setPasswordError(null)
        try {
            await updatePassword(currentPassword, newPassword)
            setEditingPassword(false)
            setCurrentPassword("")
            setNewPassword("")
            setPasswordSuccess(true)
        } catch (err) {
            setPasswordError(err instanceof Error ? err.message : "Något gick fel.")
        }
    }

    return (
        <div className="page auth-page">
            <p className="auth-title">Mitt konto</p>
            <div className="auth-card">
                <p className="account-current-email">{user.email}</p>

                {!editingEmail && (
                    <div className="auth-button-row">
                        <button type="button" className="btn-pill" onClick={() => setEditingEmail(true)}>
                            Redigera mail
                        </button>
                    </div>
                )}

                {editingEmail && (
                    <form onSubmit={handleEmailSubmit}>
                        <div className="auth-field">
                            <label className="auth-label" htmlFor="newEmail">Ny e-post</label>
                            <input
                                className="auth-input"
                                type="email"
                                id="newEmail"
                                value={newEmail}
                                onChange={(e) => setNewEmail(e.target.value)}
                                required
                            />
                        </div>
                        <div className="auth-field">
                            <label className="auth-label" htmlFor="emailPassword">Nuvarande lösenord</label>
                            <input
                                className="auth-input"
                                type="password"
                                id="emailPassword"
                                value={emailPassword}
                                onChange={(e) => setEmailPassword(e.target.value)}
                                required
                            />
                        </div>
                        {emailError && <p className="account-error">{emailError}</p>}
                        <div className="auth-button-row">
                            <button type="submit" className="btn-pill">Spara</button>
                            <button type="button" className="btn-pill" onClick={() => setEditingEmail(false)}>
                                Avbryt
                            </button>
                        </div>
                    </form>
                )}

                <hr className="account-divider" />

                {!editingPassword && (
                    <div className="auth-button-row">
                        <button type="button" className="btn-pill" onClick={() => setEditingPassword(true)}>
                            Redigera lösenord
                        </button>
                    </div>
                )}
                {passwordSuccess && !editingPassword && (
                    <p className="account-success">Lösenord uppdaterat.</p>
                )}

                {editingPassword && (
                    <form onSubmit={handlePasswordSubmit}>
                        <div className="auth-field">
                            <label className="auth-label" htmlFor="currentPassword">Nuvarande lösenord</label>
                            <input
                                className="auth-input"
                                type="password"
                                id="currentPassword"
                                value={currentPassword}
                                onChange={(e) => setCurrentPassword(e.target.value)}
                                required
                            />
                        </div>
                        <div className="auth-field">
                            <label className="auth-label" htmlFor="newPassword">Nytt lösenord</label>
                            <input
                                className="auth-input"
                                type="password"
                                id="newPassword"
                                value={newPassword}
                                onChange={(e) => setNewPassword(e.target.value)}
                                required
                                minLength={8}
                            />
                        </div>
                        {passwordError && <p className="account-error">{passwordError}</p>}
                        <div className="auth-button-row">
                            <button type="submit" className="btn-pill">Spara</button>
                            <button type="button" className="btn-pill" onClick={() => setEditingPassword(false)}>
                                Avbryt
                            </button>
                        </div>
                    </form>
                )}
            </div>
        </div>
    )
}

export default Account