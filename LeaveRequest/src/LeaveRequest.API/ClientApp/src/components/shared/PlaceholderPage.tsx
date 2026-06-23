import { Link } from 'react-router-dom'

interface PlaceholderPageProps {
  moduleName: string
  breadcrumb?: Array<{ label: string; path?: string }>
}

export default function PlaceholderPage({ moduleName, breadcrumb }: PlaceholderPageProps) {
  const crumbs = breadcrumb ?? [{ label: 'หน้าหลัก', path: '/' }, { label: moduleName }]

  return (
    <div style={styles.page}>
      {/* Breadcrumb */}
      <nav style={styles.breadcrumb} aria-label="breadcrumb">
        {crumbs.map((crumb, index) => (
          <span key={index} style={styles.crumbItem}>
            {index > 0 && <span style={styles.crumbSep}>›</span>}
            {crumb.path ? (
              <Link to={crumb.path} style={styles.crumbLink}>{crumb.label}</Link>
            ) : (
              <span style={styles.crumbCurrent}>{crumb.label}</span>
            )}
          </span>
        ))}
      </nav>

      {/* Placeholder card */}
      <div style={styles.card}>
        <div style={styles.iconWrap}>🚧</div>
        <h2 style={styles.title}>กำลังพัฒนา — {moduleName}</h2>
        <p style={styles.desc}>
          หน้านี้อยู่ระหว่างการพัฒนา จะเปิดใช้งานในเร็วๆ นี้
        </p>
        {/* TODO: Remove this status note when page is implemented */}
        <div style={styles.statusBadge}>กำลังพัฒนา</div>
      </div>
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  page: { padding: '0 0 24px' },
  breadcrumb: {
    display: 'flex',
    alignItems: 'center',
    gap: 4,
    marginBottom: 20,
    fontSize: 13,
    color: 'var(--color-text-muted)',
  },
  crumbItem: { display: 'flex', alignItems: 'center', gap: 4 },
  crumbSep: { color: 'var(--color-border)' },
  crumbLink: { color: 'var(--color-primary)', textDecoration: 'none' },
  crumbCurrent: { color: 'var(--color-text)', fontWeight: 500 },
  card: {
    backgroundColor: 'var(--color-surface)',
    border: '1px solid var(--color-border)',
    borderRadius: 8,
    padding: '48px 32px',
    textAlign: 'center',
    maxWidth: 480,
    margin: '0 auto',
  },
  iconWrap: { fontSize: 48, marginBottom: 16 },
  title: { fontSize: 20, fontWeight: 600, marginBottom: 8, color: 'var(--color-text)' },
  desc: { color: 'var(--color-text-muted)', fontSize: 14, marginBottom: 16 },
  statusBadge: {
    display: 'inline-block',
    backgroundColor: 'var(--color-primary-light)',
    color: 'var(--color-primary-dark)',
    borderRadius: 12,
    padding: '4px 14px',
    fontSize: 12,
    fontWeight: 500,
  },
}
