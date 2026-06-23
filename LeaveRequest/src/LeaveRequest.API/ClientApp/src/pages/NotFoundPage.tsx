import { Link } from 'react-router-dom'

export default function NotFoundPage() {
  return (
    <div style={styles.page}>
      <div style={styles.code}>404</div>
      <h2 style={styles.title}>ไม่พบหน้าที่ต้องการ</h2>
      <p style={styles.desc}>URL ที่ระบุไม่มีอยู่ในระบบ</p>
      <Link to="/" style={styles.homeLink}>← กลับหน้าหลัก</Link>
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  page: {
    display: 'flex',
    flexDirection: 'column',
    alignItems: 'center',
    justifyContent: 'center',
    height: '60vh',
    gap: 12,
    textAlign: 'center',
  },
  code: { fontSize: 72, fontWeight: 700, color: 'var(--color-border)', lineHeight: 1 },
  title: { fontSize: 22, fontWeight: 600, color: 'var(--color-text)' },
  desc: { color: 'var(--color-text-muted)', fontSize: 14 },
  homeLink: {
    marginTop: 8,
    color: 'var(--color-primary)',
    fontSize: 14,
    textDecoration: 'none',
    fontWeight: 500,
  },
}
