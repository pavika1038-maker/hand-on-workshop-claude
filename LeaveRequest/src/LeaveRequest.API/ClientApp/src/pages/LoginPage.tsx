import { useState } from 'react'
import { useAuth } from '../context/AuthContext'

export default function LoginPage() {
  const { login } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError]       = useState('')
  const [loading, setLoading]   = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!username || !password) { setError('กรุณากรอก Username และ Password'); return }
    setLoading(true)
    setError('')
    const result = await login(username, password)
    if (!result.ok) setError(result.error ?? 'เข้าสู่ระบบไม่สำเร็จ')
    setLoading(false)
  }

  return (
    <div style={s.page}>
      <form style={s.card} onSubmit={handleSubmit}>
        <div style={s.logo}>🏢</div>
        <h1 style={s.title}>ระบบบริหารวันลา</h1>
        <p style={s.subtitle}>ABC Company — Leave Request and Approval</p>

        {error && <div style={s.errorBox}>{error}</div>}

        <div style={s.fieldGroup}>
          <label style={s.label}>Username (Email)</label>
          <input
            style={s.input}
            type="text"
            placeholder="เช่น somchai@abc.com"
            value={username}
            onChange={e => setUsername(e.target.value)}
            autoFocus
          />
        </div>

        <div style={s.fieldGroup}>
          <label style={s.label}>Password</label>
          <input
            style={s.input}
            type="password"
            placeholder="••••••"
            value={password}
            onChange={e => setPassword(e.target.value)}
          />
        </div>

        <div style={s.hintBox}>
          💡 <strong>Demo accounts</strong> — password ทุก account คือ <code>1234</code>
          <table style={s.table}>
            <tbody>
              <tr><td style={s.td}>somchai@abc.com</td><td style={s.tdRole}>Employee</td></tr>
              <tr><td style={s.td}>wipa@abc.com</td>   <td style={s.tdRole}>Manager</td></tr>
              <tr><td style={s.td}>nanta@abc.com</td>  <td style={s.tdRole}>HR</td></tr>
            </tbody>
          </table>
        </div>

        <button style={{ ...s.loginBtn, opacity: loading ? 0.7 : 1 }} type="submit" disabled={loading}>
          {loading ? 'กำลังเข้าสู่ระบบ...' : 'เข้าสู่ระบบ'}
        </button>
      </form>
    </div>
  )
}

const s: Record<string, React.CSSProperties> = {
  page: {
    minHeight: '100dvh',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    backgroundColor: '#f3f4f6',
  },
  card: {
    backgroundColor: '#fff',
    borderRadius: 12,
    padding: '40px 36px',
    width: 400,
    boxShadow: '0 4px 24px rgba(0,0,0,0.1)',
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    gap: 14,
  },
  logo: { fontSize: 48 },
  title: { fontSize: 22, fontWeight: 700, margin: 0, color: '#111' },
  subtitle: { fontSize: 13, color: '#666', margin: 0, textAlign: 'center' },
  errorBox: {
    width: '100%',
    backgroundColor: '#fef2f2',
    border: '1px solid #fca5a5',
    borderRadius: 6,
    padding: '10px 14px',
    fontSize: 13,
    color: '#b91c1c',
    boxSizing: 'border-box',
  },
  fieldGroup: { width: '100%', display: 'flex', flexDirection: 'column', gap: 4 },
  label: { fontSize: 13, fontWeight: 600, color: '#333' },
  input: {
    width: '100%',
    padding: '10px 12px',
    border: '1px solid #d1d5db',
    borderRadius: 6,
    fontSize: 14,
    outline: 'none',
    boxSizing: 'border-box',
  },
  hintBox: {
    width: '100%',
    backgroundColor: '#f0f9ff',
    border: '1px solid #bae6fd',
    borderRadius: 6,
    padding: '10px 14px',
    fontSize: 12,
    color: '#0369a1',
    boxSizing: 'border-box',
  },
  table: { marginTop: 8, width: '100%', borderCollapse: 'collapse' },
  td: { padding: '3px 8px 3px 0', fontSize: 12, color: '#0369a1' },
  tdRole: { padding: '3px 0', fontSize: 11, color: '#666' },
  loginBtn: {
    width: '100%',
    padding: '12px 0',
    backgroundColor: '#0078d4',
    color: '#fff',
    border: 'none',
    borderRadius: 6,
    fontSize: 14,
    fontWeight: 600,
    cursor: 'pointer',
  },
}
