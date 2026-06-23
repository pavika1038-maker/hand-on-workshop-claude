// SCR-002: Leave Balance Dashboard + หน้าหลัก
import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { apiGet } from '../api'
import type { ApiResponse, LeaveBalanceDashboard, LeaveBalanceItem } from '../types/leaveRequest'

function BalanceBar({ item }: { item: LeaveBalanceItem }) {
  const pct = item.entitledDays > 0
    ? Math.max(0, Math.min(100, (item.remainingDays / item.entitledDays) * 100))
    : 0
  const barColor = pct < 25 ? '#dc2626' : pct < 50 ? '#d97706' : '#15803d'
  const ICON: Record<string, string> = {
    ANNUAL: '🏖️', SICK: '🏥', PERSONAL: '📋', MATERNITY: '👶',
  }
  return (
    <div style={s.balCard}>
      <div style={s.balTop}>
        <span style={s.balIcon}>{ICON[item.typeCode] ?? '📅'}</span>
        <span style={s.balName}>{item.typeNameTh}</span>
        <span style={{ ...s.balRemain, color: barColor }}>
          {item.remainingDays} <small style={{ fontSize: 11, fontWeight: 400 }}>วัน</small>
        </span>
      </div>
      <div style={s.barTrack}>
        <div style={{ ...s.barFill, width: `${pct}%`, backgroundColor: barColor }} />
      </div>
      <div style={s.balDetail}>
        <span>สิทธิ์ {item.entitledDays} วัน</span>
        <span>ใช้ {item.usedDays} · รอ {item.pendingDays}</span>
      </div>
    </div>
  )
}

export default function HomePage() {
  const { user } = useAuth()
  const [dashboard, setDashboard] = useState<LeaveBalanceDashboard | null>(null)
  const [loadingBal, setLoadingBal] = useState(true)
  const [balError, setBalError] = useState<string | null>(null)

  useEffect(() => {
    if (!user?.employeeId) return
    let active = true
    setLoadingBal(true)
    apiGet<ApiResponse<LeaveBalanceDashboard>>(
      `/api/v1/leave-balances/dashboard?employeeId=${user.employeeId}`
    )
      .then(json => {
        if (!active) return
        if (json.success && json.data) setDashboard(json.data)
        else setBalError(json.message ?? 'ไม่สามารถโหลดข้อมูลวันลาได้')
      })
      .catch(() => { if (active) setBalError('เกิดข้อผิดพลาดในการเชื่อมต่อ') })
      .finally(() => { if (active) setLoadingBal(false) })
    return () => { active = false }
  }, [user?.employeeId])

  const quickLinks = [
    { path: '/leave-requests', label: 'ยื่นคำร้องขอลา', icon: '📋', show: true },
    { path: '/approvals',      label: 'อนุมัติ/ปฏิเสธ', icon: '✅', show: user?.role === 'Manager' || user?.role === 'HR' },
    { path: '/leave-history',  label: 'ประวัติการลา',   icon: '📅', show: true },
    { path: '/hr-dashboard',   label: 'HR Dashboard',    icon: '📊', show: user?.role === 'HR' },
    { path: '/settings',       label: 'ตั้งค่าระบบ',    icon: '⚙️', show: user?.role === 'HR' },
  ].filter(l => l.show)

  return (
    <div>
      {/* Hero */}
      <div style={s.hero}>
        <span style={{ fontSize: 36 }}>🏢</span>
        <div>
          <h1 style={s.heroTitle}>สวัสดี, {user?.name}</h1>
          <p style={s.heroSub}>ABC Company — Leave Request and Approval System · Role: <strong>{user?.role}</strong></p>
        </div>
      </div>

      {/* Balance Dashboard (SCR-002) */}
      <section style={s.section}>
        <div style={s.sectionHeader}>
          <h2 style={s.sectionTitle}>📊 วันลาคงเหลือของฉัน</h2>
          {dashboard && <span style={s.yearBadge}>ปี {dashboard.leaveYear}</span>}
        </div>
        {loadingBal ? (
          <p style={s.hint}>⏳ กำลังโหลด...</p>
        ) : balError ? (
          <div style={s.errorBox}>{balError}</div>
        ) : !dashboard?.balances.length ? (
          <p style={s.hint}>ยังไม่มีข้อมูลสิทธิ์วันลาในปีนี้</p>
        ) : (
          <div style={s.balGrid}>
            {dashboard.balances.map(b => <BalanceBar key={b.leaveTypeId} item={b} />)}
          </div>
        )}
      </section>

      {/* Quick nav */}
      <section style={s.section}>
        <h2 style={s.sectionTitle}>เมนูหลัก</h2>
        <div style={s.grid}>
          {quickLinks.map(l => (
            <Link to={l.path} key={l.path} style={s.card}>
              <span style={{ fontSize: 28 }}>{l.icon}</span>
              <span style={s.cardLabel}>{l.label}</span>
              <span style={s.cardArrow}>›</span>
            </Link>
          ))}
        </div>
      </section>
    </div>
  )
}

const s: Record<string, React.CSSProperties> = {
  hero: {
    display: 'flex', alignItems: 'center', gap: 16,
    backgroundColor: 'var(--color-surface)', border: '1px solid var(--color-border)',
    borderRadius: 8, padding: '20px 24px', marginBottom: 20,
  },
  heroTitle: { fontSize: 20, fontWeight: 700, margin: 0, color: 'var(--color-text)' },
  heroSub:   { fontSize: 13, color: 'var(--color-text-muted)', margin: '4px 0 0' },
  section:   { marginBottom: 24 },
  sectionHeader: { display: 'flex', alignItems: 'center', gap: 10, marginBottom: 12 },
  sectionTitle:  { fontSize: 15, fontWeight: 600, color: 'var(--color-text)', margin: 0 },
  yearBadge: {
    fontSize: 11, backgroundColor: 'var(--color-primary-light)',
    color: 'var(--color-primary)', borderRadius: 10, padding: '2px 10px',
  },
  balGrid: { display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))', gap: 12 },
  balCard: {
    backgroundColor: 'var(--color-surface)', border: '1px solid var(--color-border)',
    borderRadius: 8, padding: '14px 16px',
  },
  balTop:    { display: 'flex', alignItems: 'center', gap: 8, marginBottom: 8 },
  balIcon:   { fontSize: 20 },
  balName:   { fontSize: 13, fontWeight: 500, flex: 1, color: 'var(--color-text)' },
  balRemain: { fontSize: 22, fontWeight: 700 },
  barTrack:  { height: 6, borderRadius: 3, backgroundColor: '#e5e7eb', overflow: 'hidden' },
  barFill:   { height: '100%', borderRadius: 3, transition: 'width 0.3s' },
  balDetail: {
    display: 'flex', justifyContent: 'space-between',
    fontSize: 11, color: 'var(--color-text-muted)', marginTop: 6,
  },
  grid: { display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: 10 },
  card: {
    display: 'flex', alignItems: 'center', gap: 10,
    backgroundColor: 'var(--color-surface)', border: '1px solid var(--color-border)',
    borderRadius: 8, padding: '14px 18px', textDecoration: 'none', color: 'inherit',
  },
  cardLabel: { flex: 1, fontSize: 14, fontWeight: 500 },
  cardArrow: { color: 'var(--color-primary)', fontSize: 20, fontWeight: 700 },
  hint: { color: 'var(--color-text-muted)', fontSize: 13 },
  errorBox: {
    backgroundColor: '#fde8e8', border: '1px solid #fca5a5',
    borderRadius: 6, padding: '10px 14px', fontSize: 13, color: '#dc2626',
  },
}
