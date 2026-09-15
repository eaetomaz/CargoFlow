import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      await login(username, password)
      navigate('/')
    } catch {
      setError('Usuário ou senha inválidos.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="login-shell">
      <form className="login-form" onSubmit={handleSubmit}>
        <h1>CargoFlow</h1>
        <p className="login-subtitle">TMS de portfólio -- entre com um dos usuários seedados.</p>

        {error && <p className="login-error">{error}</p>}

        <label>
          Usuário
          <input value={username} onChange={(e) => setUsername(e.target.value)} autoFocus />
        </label>
        <label>
          Senha
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </label>

        <button type="submit" disabled={submitting || !username || !password}>
          {submitting ? 'Entrando...' : 'Entrar'}
        </button>

        <p className="login-hint">
          admin / operacional / financeiro / manutencao / motorista — senha <code>CargoFlow@123</code>
        </p>
      </form>
    </div>
  )
}
