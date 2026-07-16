import { Outlet, NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function MainLayout() {
  const navigate = useNavigate()
  const { user, logout } = useAuth()

  const handleLogout = () => { logout(); navigate('/', { replace: true }) }

  const navItems = [
    { path: '/leave-requests', label: 'ยื่นคำร้องขอลา',     icon: '📋', roles: ['Employee','Manager','HR'] },
    { path: '/approvals',      label: 'อนุมัติ/ปฏิเสธการลา', icon: '✅', roles: ['Manager','HR'] },
    { path: '/leave-history',  label: 'ประวัติการลา',        icon: '📅', roles: ['Employee','Manager','HR'] },
    { path: '/hr-dashboard',   label: 'HR Dashboard',         icon: '📊', roles: ['HR'] },
    { path: '/leave-report',   label: 'รายงานการลา',          icon: '📈', roles: ['HR'] },
    { path: '/notification-log', label: 'Log การแจ้งเตือน',    icon: '📧', roles: ['HR'] },
    { path: '/settings',       label: 'ตั้งค่าระบบ',         icon: '⚙️', roles: ['HR'] },
  ].filter(item => !user?.role || item.roles.includes(user.role))

  return (
    <div style={styles.shell}>
      <header style={styles.topbar}>
        <button style={styles.logoBtn} onClick={() => navigate('/')}>
          <span style={styles.logoIcon}>🏢</span>
          <span style={styles.logoText}>ระบบบริหารวันลา</span>
          <span style={styles.companyBadge}>ABC Company</span>
        </button>
        <div style={styles.topbarRight}>
          <span style={styles.userInfo}>👤 {user?.name} ({user?.role})</span>
          <button style={styles.logoutBtn} onClick={handleLogout}>ออกจากระบบ</button>
        </div>
      </header>

      <div style={styles.body}>
        <nav style={styles.sidebar}>
          <ul style={styles.navList}>
            {navItems.map(item => (
              <li key={item.path}>
                <NavLink
                  to={item.path}
                  style={({ isActive }) => ({
                    ...styles.navLink,
                    ...(isActive ? styles.navLinkActive : {}),
                  })}
                >
                  <span style={styles.navIcon}>{item.icon}</span>
                  <span>{item.label}</span>
                </NavLink>
              </li>
            ))}
          </ul>
          <div style={styles.sidebarFooter}>
            <small style={{ color: 'var(--color-sidebar-text)', opacity: 0.5 }}>v0.2.0</small>
          </div>
        </nav>
        <main style={styles.content}>
          <Outlet />
        </main>
      </div>
    </div>
  )
}

const styles: Record<string, React.CSSProperties> = {
  shell:        { display: 'flex', flexDirection: 'column', height: '100dvh', overflow: 'hidden' },
  topbar:       { height: 'var(--topbar-height)', backgroundColor: 'var(--color-topbar-bg)', color: 'var(--color-topbar-text)', display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '0 16px', flexShrink: 0, boxShadow: '0 2px 4px rgba(0,0,0,0.2)', zIndex: 100 },
  logoBtn:      { display: 'flex', alignItems: 'center', gap: 8, background: 'none', border: 'none', color: '#fff', cursor: 'pointer', padding: 0 },
  logoIcon:     { fontSize: 20 },
  logoText:     { fontSize: 16, fontWeight: 600 },
  companyBadge: { fontSize: 11, background: 'rgba(255,255,255,0.2)', borderRadius: 4, padding: '2px 6px' },
  topbarRight:  { display: 'flex', alignItems: 'center', gap: 12 },
  userInfo:     { fontSize: 13 },
  logoutBtn:    { background: 'rgba(255,255,255,0.15)', border: '1px solid rgba(255,255,255,0.3)', color: '#fff', borderRadius: 4, padding: '4px 10px', fontSize: 12, cursor: 'pointer' },
  body:         { display: 'flex', flex: 1, overflow: 'hidden' },
  sidebar:      { width: 'var(--sidebar-width)', backgroundColor: 'var(--color-sidebar-bg)', display: 'flex', flexDirection: 'column', flexShrink: 0, overflowY: 'auto' },
  navList:      { listStyle: 'none', padding: '8px 0', flex: 1 },
  navLink:      { display: 'flex', alignItems: 'center', gap: 10, padding: '10px 16px', color: 'var(--color-sidebar-text)', fontSize: 13, borderLeft: '3px solid transparent', textDecoration: 'none' },
  navLinkActive:{ backgroundColor: 'rgba(0,120,212,0.2)', borderLeftColor: 'var(--color-sidebar-active)', color: '#fff', fontWeight: 600 },
  navIcon:      { fontSize: 16, width: 20, textAlign: 'center' },
  sidebarFooter:{ padding: '12px 16px', borderTop: '1px solid rgba(255,255,255,0.08)' },
  content:      { flex: 1, overflowY: 'auto', padding: 24, backgroundColor: 'var(--color-bg)' },
}
