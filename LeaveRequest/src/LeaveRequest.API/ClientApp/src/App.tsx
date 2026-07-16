import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'
import MainLayout from './layouts/MainLayout'
import LoginPage from './pages/LoginPage'
import HomePage from './pages/HomePage'
import LeaveRequestListPage from './pages/leave-request/LeaveRequestListPage'
import LeaveRequestDetailPage from './pages/leave-request/LeaveRequestDetailPage'
import ApprovalListPage from './pages/approval/ApprovalListPage'
import LeaveHistoryPage from './pages/leave-history/LeaveHistoryPage'
import SettingsPage from './pages/settings/SettingsPage'
import HrDashboardPage from './pages/hr/HrDashboardPage'
import LeaveReportPage from './pages/reports/LeaveReportPage'
import NotificationLogPage from './pages/notification-log/NotificationLogPage'
import NotFoundPage from './pages/NotFoundPage'

function AppRoutes() {
  const { user } = useAuth()
  if (!user) return <LoginPage />
  return (
    <Routes>
      <Route path="/" element={<MainLayout />}>
        <Route index element={<HomePage />} />
        <Route path="leave-requests" element={<LeaveRequestListPage />} />
        <Route path="leave-requests/:id" element={<LeaveRequestDetailPage />} />
        <Route path="approvals"      element={<ApprovalListPage />} />
        <Route path="leave-history"  element={<LeaveHistoryPage />} />
        <Route path="hr-dashboard"   element={<HrDashboardPage />} />
        <Route path="leave-report"   element={<LeaveReportPage />} />
        <Route path="notification-log" element={<NotificationLogPage />} />
        <Route path="settings"       element={<SettingsPage />} />
        <Route path="*"              element={<NotFoundPage />} />
      </Route>
    </Routes>
  )
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </AuthProvider>
  )
}
