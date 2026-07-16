import { useAuth } from '../../context/AuthContext.jsx'

export default function ManageUserAccounts() {
  const { users, setUsers, user: currentUser } = useAuth()

  const adminCount = users.filter((u) => u.role === 'admin').length

  function toggleRole(target) {
    if (target.role === 'admin' && adminCount === 1) {
      window.alert('Impossible de retirer le dernier compte admin.')
      return
    }
    setUsers((prev) =>
      prev.map((u) => (u.id === target.id ? { ...u, role: u.role === 'admin' ? 'player' : 'admin' } : u))
    )
  }

  function deleteUser(target) {
    if (target.id === currentUser.id) {
      window.alert('Tu ne peux pas supprimer ton propre compte depuis cet écran.')
      return
    }
    if (window.confirm(`Supprimer le compte de "${target.name}" ?`)) {
      setUsers((prev) => prev.filter((u) => u.id !== target.id))
    }
  }

  return (
    <div>
      <div className="admin-header">
        <div>
          <h1>👥 Gérer les comptes utilisateurs</h1>
          <p>Liste des joueurs inscrits et des administrateurs de la plateforme.</p>
        </div>
      </div>

      <div className="table-wrap">
        <table>
          <thead>
            <tr>
              <th>Nom</th>
              <th>Email</th>
              <th>Rôle</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {users.map((u) => (
              <tr key={u.id}>
                <td>
                  {u.avatar} {u.name}
                </td>
                <td>{u.email}</td>
                <td>
                  <span className={`badge ${u.role === 'admin' ? 'badge-blue' : 'badge-gray'}`}>{u.role}</span>
                </td>
                <td>
                  <div className="row-actions">
                    <button type="button" className="btn btn-outline btn-sm" onClick={() => toggleRole(u)}>
                      {u.role === 'admin' ? 'Rétrograder en joueur' : 'Promouvoir admin'}
                    </button>
                    <button type="button" className="btn btn-danger btn-sm" onClick={() => deleteUser(u)}>
                      Supprimer
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
