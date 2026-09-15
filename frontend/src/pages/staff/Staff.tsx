import { useEffect, useMemo, useState } from "react";
import ListToolbar from "../../components/ListToolbar";
import ListCard from "../../components/ListCard";
import Form from "../../components/Form";
import SearchBar from "../../components/SearchBar";
import usePagination from "../../hooks/usePagination";
import {
  getAllAdminUsers,
  createAdminUser,
  updateAdminUser,
  updateAdminUserRoles,
  deleteAdminUser,
} from "../../api/adminApi";
import type { AdminUser } from "../../types/admin";

function Staff() {
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [confirmingDeleteId, setConfirmingDeleteId] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [editingUser, setEditingUser] = useState<AdminUser | null>(null);

  function loadUsers() {
    setLoading(true);
    getAllAdminUsers()
      .then((data) => {
        setUsers(data.map((u) => ({ id: u.id as unknown as string, email: u.email, roles: u.roles })));
        setError(null);
      })
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    loadUsers();
  }, []);

  function handleDelete(id: string) {
    setError(null);
    setDeletingId(id);
    deleteAdminUser(id)
      .then(() => {
        setConfirmingDeleteId(null);
        loadUsers();
      })
      .catch((err) => {
        setConfirmingDeleteId(null);
        setError(err.message);
      })
      .finally(() => setDeletingId(null));
  }

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    return users.filter((u) => u.email.toLowerCase().includes(q));
  }, [users, query]);

  const { page, setPage, totalPages, pagedItems } = usePagination(filtered, 12, query);

  return (
    <div className="page staff">
      <h1>Personal</h1>

      <SearchBar value={query} onChange={setQuery} placeholder="Sök e-post" />

      <ListToolbar
        addLabel="Skapa personal"
        onAdd={() => setShowCreate(true)}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
      />

      {loading && <p>Laddar...</p>}
      {error && <p>Fel: {error}</p>}

      {!loading && (
        <div className="card-grid">
          {pagedItems.map((user: any) => (
            <ListCard
              key={user.id}
              title={user.email}
              infoLines={[user.roles.join(", ")]}
              confirming={confirmingDeleteId === user.id}
              deleting={deletingId === user.id}
              onEdit={() => setEditingUser(user)}
              onRequestDelete={() => setConfirmingDeleteId(user.id)}
              onConfirmDelete={() => handleDelete(user.id)}
              onCancelDelete={() => setConfirmingDeleteId(null)}
            />
          ))}
        </div>
      )}

      {showCreate && (
        <CreateStaffForm
          onClose={() => setShowCreate(false)}
          onSaved={() => {
            setShowCreate(false);
            loadUsers();
          }}
        />
      )}

      {editingUser && (
        <EditStaffForm
          user={editingUser}
          onClose={() => setEditingUser(null)}
          onSaved={() => {
            setEditingUser(null);
            loadUsers();
          }}
        />
      )}
    </div>
  );
}

function CreateStaffForm({ onClose, onSaved }: { onClose: () => void; onSaved: () => void }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [role, setRole] = useState("Staff");
  const [error, setError] = useState<string | null>(null);

  function handleSubmit(e: any) {
    e.preventDefault();
    if (password !== confirmPassword) {
      setError("Lösenorden matchar inte");
      return;
    }
    setError(null);
    createAdminUser(email, password, [role])
      .then(onSaved)
      .catch((err) => setError(err.message));
  }

  return (
    <Form title="Skapa personal" submitLabel="Skapa" onSubmit={handleSubmit} onCancel={onClose}>
      {error && <p className="form-error">{error}</p>}
      <div className="form-field">
        <label className="form-label">E-post</label>
        <input className="form-input" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
      </div>

      <div className="form-field">
        <label className="form-label">Lösenord</label>
        <input className="form-input" type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
      </div>

      <div className="form-field">
        <label className="form-label">Bekräfta lösenord</label>
        <input className="form-input" type="password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} required />
      </div>

      <div className="form-field">
        <label className="form-label">Roll</label>
        <select className="form-input" value={role} onChange={(e) => setRole(e.target.value)}>
          <option value="Staff">Staff</option>
          <option value="Admin">Admin</option>
        </select>
      </div>
    </Form>
  );
}

function EditStaffForm({ user, onClose, onSaved }: { user: AdminUser; onClose: () => void; onSaved: () => void }) {
  const [email, setEmail] = useState(user.email);
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [role, setRole] = useState(user.roles.includes("Admin") ? "Admin" : "Staff");
  const [error, setError] = useState<string | null>(null);

  function handleSubmit(e: any) {
    e.preventDefault();
    if (password && password !== confirmPassword) {
      setError("Lösenorden matchar inte");
      return;
    }
    setError(null);

    (async () => {
      try {
        // First update email/password in a single request to avoid concurrent modifications
        const newEmail = email !== user.email ? email : undefined;
        const newPassword = password ? password : undefined;
        if (newEmail || newPassword) {
          await updateAdminUser(user.id, newEmail, newPassword);
        }

        // Then update roles — perform after user update to avoid optimistic concurrency conflicts
        await updateAdminUserRoles(user.id, [role]);

        onSaved();
      } catch (err: any) {
        setError(err.message ?? String(err));
      }
    })();
  }

  return (
    <Form title="Redigera personal" submitLabel="Spara" onSubmit={handleSubmit} onCancel={onClose}>
      {error && <p className="form-error">{error}</p>}
      <div className="form-field">
        <label className="form-label">E-post</label>
        <input className="form-input" type="email" value={email} onChange={(e) => setEmail(e.target.value)} required />
      </div>

      <div className="form-field">
        <label className="form-label">Nytt lösenord (lämna tomt för att behålla)</label>
        <input className="form-input" type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
      </div>

      <div className="form-field">
        <label className="form-label">Bekräfta lösenord</label>
        <input className="form-input" type="password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} />
      </div>

      <div className="form-field">
        <label className="form-label">Roll</label>
        <select className="form-input" value={role} onChange={(e) => setRole(e.target.value)}>
          <option value="Staff">Staff</option>
          <option value="Admin">Admin</option>
        </select>
      </div>
    </Form>
  );
}

export default Staff;
