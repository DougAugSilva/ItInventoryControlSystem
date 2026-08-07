import { useEffect, useState } from 'react';
import { apiJson } from '../services/api';
import './Users.css';

const EMPTY_FORM = { username: '', fullName: '', password: '', isAdmin: false };

// Default administrator account: cannot be edited or removed, to guarantee there
// is always administrative access to the application.
const PROTECTED_ADMIN_USERNAME = 'admin.besttechti';

function formatDate(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString('en-US');
}

export default function Users() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [form, setForm] = useState(EMPTY_FORM);
  const [editingId, setEditingId] = useState(null);
  const [saving, setSaving] = useState(false);

  function load() {
    setLoading(true);
    apiJson('/api/users')
      .then(setUsers)
      .catch(() => setError('Could not load users.'))
      .finally(() => setLoading(false));
  }

  useEffect(load, []);

  function updateField(field, value) {
    setForm((current) => ({ ...current, [field]: value }));
  }

  function startEditing(user) {
    setEditingId(user.id);
    setForm({ username: user.username, fullName: user.fullName, password: '', isAdmin: user.isAdmin });
    setError('');
    setMessage('');
  }

  function cancelEditing() {
    setEditingId(null);
    setForm(EMPTY_FORM);
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setError('');
    setMessage('');

    if (!form.username.trim() || (!editingId && (!form.fullName.trim() || !form.password.trim()))) {
      setError('Fill in username, full name, and password.');
      return;
    }

    setSaving(true);
    try {
      if (editingId) {
        await apiJson(`/api/users/${editingId}`, {
          method: 'PUT',
          body: { username: form.username, password: form.password || null },
        });
        setMessage('User updated successfully.');
      } else {
        await apiJson('/api/users', {
          method: 'POST',
          body: {
            username: form.username,
            fullName: form.fullName,
            password: form.password,
            isAdmin: form.isAdmin,
          },
        });
        setMessage('User registered successfully.');
      }

      setForm(EMPTY_FORM);
      setEditingId(null);
      load();
    } catch (err) {
      setError(err.message);
    } finally {
      setSaving(false);
    }
  }

  async function removeUser(user) {
    if (!window.confirm(`Remove user "${user.username}"?`)) {
      return;
    }

    setError('');
    setMessage('');
    try {
      await apiJson(`/api/users/${user.id}`, { method: 'DELETE' });
      setMessage('User removed successfully.');
      load();
    } catch (err) {
      setError(err.message);
    }
  }

  return (
    <div className="users-page page-box">
      <div className="page-box-header">
        <h1>User Management</h1>
      </div>

      <div className="page-box-body">
        <form className="users-form" onSubmit={handleSubmit}>
          <h2>{editingId ? 'Edit user' : 'New user'}</h2>

          <label>
            Username
            <input
              type="text"
              value={form.username}
              onChange={(event) => updateField('username', event.target.value)}
              required
            />
          </label>

          {!editingId && (
            <label>
              Full name
              <input
                type="text"
                value={form.fullName}
                onChange={(event) => updateField('fullName', event.target.value)}
                required
              />
            </label>
          )}

          <label>
            {editingId ? 'New password (leave blank to keep current)' : 'Password'}
            <input
              type="password"
              value={form.password}
              onChange={(event) => updateField('password', event.target.value)}
              required={!editingId}
              minLength={12}
            />
            <span className="users-password-hint">
              At least 12 characters, with uppercase, lowercase, and a number.
            </span>
          </label>

          {!editingId && (
            <label className="users-checkbox">
              <input
                type="checkbox"
                checked={form.isAdmin}
                onChange={(event) => updateField('isAdmin', event.target.checked)}
              />
              Administrator
            </label>
          )}

          {error && <p className="users-error">{error}</p>}
          {message && <p className="users-success">{message}</p>}

          <div className="users-form-actions">
            <button type="submit" disabled={saving}>
              {saving ? 'Saving...' : editingId ? 'Save changes' : 'Register user'}
            </button>
            {editingId && (
              <button type="button" className="users-cancel" onClick={cancelEditing}>
                Cancel
              </button>
            )}
          </div>
        </form>

        {!loading && users.length === 0 && <p className="users-empty">No users registered.</p>}

        {users.length > 0 && (
          <table className="users-table">
            <thead>
              <tr>
                <th>Username</th>
                <th>Full name</th>
                <th>Administrator</th>
                <th>Created at</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.map((user) => {
                const protectedUser = user.username === PROTECTED_ADMIN_USERNAME;
                return (
                  <tr key={user.id}>
                    <td>{user.username}</td>
                    <td>{user.fullName}</td>
                    <td>{user.isAdmin ? 'Yes' : 'No'}</td>
                    <td>{formatDate(user.createdAt)}</td>
                    <td className="users-actions">
                      <button
                        type="button"
                        disabled={protectedUser}
                        title={protectedUser ? 'The default administrator account cannot be edited.' : undefined}
                        onClick={() => startEditing(user)}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="users-remove"
                        disabled={protectedUser}
                        title={protectedUser ? 'The default administrator account cannot be removed.' : undefined}
                        onClick={() => removeUser(user)}
                      >
                        Remove
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
