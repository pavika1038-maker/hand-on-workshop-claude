# Run & Smoke Test Log — Leave Request and Approval System

> บันทึกตามแนวทาง 4.6 สิ่งที่ต้องบันทึกจากขั้นตอน Run และ Test  
> Feature: SCR-001 ถึง SCR-010  
> วันที่: 2026-06-19  
> ทดสอบโดย: Pavika (Workshop Day 2 Session 3)

---

## 1. Commands ที่ใช้ Run

### 1.1 Database Migration

```powershell
# อยู่ใน folder: LeaveRequest/

# สร้าง migration ครั้งแรก (schema ทั้งหมด)
dotnet ef migrations add InitialCreate --project src/LeaveRequest.Infrastructure --startup-project src/LeaveRequest.API

# สร้าง migration seed data พนักงานและวันลา
dotnet ef migrations add SeedEmployeesAndBalances --project src/LeaveRequest.Infrastructure --startup-project src/LeaveRequest.API

# สร้าง migration demo data (LeaveRequests)
dotnet ef migrations add AddDemoSeedData --project src/LeaveRequest.Infrastructure --startup-project src/LeaveRequest.API

# สร้าง migration fix ManagerId ของ EMP003
dotnet ef migrations add FixManagerId --project src/LeaveRequest.Infrastructure --startup-project src/LeaveRequest.API

# Apply migration ไปยัง database
dotnet ef database update --project src/LeaveRequest.Infrastructure --startup-project src/LeaveRequest.API

# ลบ database เก่าและ recreate (ใช้เมื่อมี seed data conflict)
Remove-Item "src\LeaveRequest.API\leave-request-dev.db" -ErrorAction SilentlyContinue
dotnet ef database update --project src/LeaveRequest.Infrastructure --startup-project src/LeaveRequest.API
```

### 1.2 Start Backend API

```powershell
cd "D:\Document Project\(5)LiXil\hand-on-workshop-claude\LeaveRequest\src\LeaveRequest.API"
dotnet run
# รอจนเห็น: Now listening on: http://localhost:5100
```

### 1.3 Start Frontend

```powershell
cd "D:\Document Project\(5)LiXil\hand-on-workshop-claude\LeaveRequest\src\LeaveRequest.API\ClientApp"
npm run dev
# รอจนเห็น: Local: http://localhost:5173
```

### 1.4 Kill Process ที่ค้างอยู่ (กรณี port ชน)

```powershell
Stop-Process -Name "dotnet" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "node" -Force -ErrorAction SilentlyContinue
```

---

## 2. Environment และ Service Dependency

| รายการ | ค่า | หมายเหตุ |
|--------|-----|---------|
| Backend URL | http://localhost:5100 | เปลี่ยนจาก 7082 เพราะ port ชน |
| Backend HTTPS | https://localhost:7082 | ยังไม่ได้ trust dev cert |
| Frontend URL | http://localhost:5173 | Vite dev server |
| Vite Proxy target | http://localhost:5100 | แก้จาก https://localhost:7082 |
| Database | SQLite (leave-request-dev.db) | อยู่ใน src/LeaveRequest.API/ |
| .NET version | .NET 10 | dotnet ef tools ต้องเป็น version เดียวกัน |
| Node version | ≥ 18 | npm run dev |
| Auth mechanism | X-Employee-Id header (Mock) | ไม่มี JWT — workshop เท่านั้น |
| Email service | Stub (IF-002) | ไม่ส่ง email จริง |
| SLA Scheduler | IF-005 — run ทุก 5 นาที | log เป็น background service |

---

## 3. Errors ที่พบจากการ Run จริง

| # | Error | สาเหตุ | Status |
|---|-------|--------|--------|
| E-001 | `Failed to bind to address http://127.0.0.1:5174: address already in use` | Vite dev server (npm run dev) จับ port 5174 ก่อน dotnet | ✅ Fixed |
| E-002 | `ไม่สามารถเชื่อมต่อ Server ได้` (Login page) | Vite proxy ชี้ไป https://localhost:7082 แต่ backend ไม่ได้ run บน port นั้น | ✅ Fixed |
| E-003 | `SQLite Error 1: 'no such table: CancelRequests'` | ยังไม่ได้ run database migration | ✅ Fixed |
| E-004 | `SQLite Error 19: 'UNIQUE constraint failed: LeaveTypes.LeaveTypeId'` | Database เก่ามี LeaveTypes อยู่แล้ว migration ใหม่ insert ซ้ำ | ✅ Fixed |
| E-005 | `The name 'InitialCreate' is used by an existing migration` | พยายาม add migration ชื่อซ้ำ | ✅ Fixed (ใช้ชื่ออื่น) |
| E-006 | HR (EMP003) ส่งคำขอลาแล้ว Manager อนุมัติไม่ได้ — ยังขึ้น "รอการอนุมัติ" | EMP003 มี `ManagerId = null` ทำให้ไม่ขึ้นใน approval queue ของ Manager ใด | ✅ Fixed |
| E-007 | PowerShell ไม่รองรับ `\` (backslash) สำหรับ line continuation | ใช้ syntax ของ bash มาใน PowerShell | ✅ Fixed (รันเป็นบรรทัดเดียว) |

---

## 4. Fixes ที่ทำแล้ว

### F-001: แก้ Port ชน
**ไฟล์:** `src/LeaveRequest.API/Properties/launchSettings.json`
```json
// เปลี่ยน applicationUrl จาก
"https://localhost:7082;http://localhost:5174"
// เป็น
"https://localhost:7082;http://localhost:5100"
```

### F-002: แก้ Vite Proxy Target
**ไฟล์:** `src/LeaveRequest.API/ClientApp/vite.config.ts`
```typescript
// เปลี่ยน target จาก
target: 'https://localhost:7082'
// เป็น
target: 'http://localhost:5100'
```

### F-003: แก้ Migration Conflict
ลบ database เก่าทิ้งก่อน แล้ว run `database update` ใหม่จะได้ database ที่สะอาด

### F-004: แก้ EMP003 ManagerId
**ไฟล์:** `src/LeaveRequest.Infrastructure/Data/Configurations/EmployeeConfiguration.cs`
```csharp
// EMP003: เปลี่ยน ManagerId จาก null เป็น "EMP002"
ManagerId = "EMP002"
```
สร้าง migration `FixManagerId` เพื่อ UPDATE ค่าใน database

### F-005: แก้ ApiResponse Structure
**ไฟล์:** `src/LeaveRequest.API/Models/ApiResponse.cs`  
ลบ nested `ApiError` class ออก เปลี่ยนเป็น flat fields (`ErrorCode`, `Message`, `Errors`) ให้ตรงกับ TypeScript interface ใน frontend

### F-006: แก้ LeaveStatus Enum Mismatch  
**ไฟล์:** `src/LeaveRequest.API/ClientApp/src/types/leaveRequest.ts`  
เปลี่ยน `'PendingApproval'` เป็น `'Pending'` ให้ตรงกับ C# `LeaveStatus.ToString()`

---

## 5. Smoke Test Results

| SCR | หน้า/Feature | ผล | หมายเหตุ |
|-----|-------------|-----|---------|
| SCR-001 | Login (Employee/Manager/HR) | ✅ Pass | session storage ทำงานถูกต้อง |
| SCR-002 | Leave Balance Dashboard | ✅ Pass | แสดง balance cards ครบ 4 ประเภท |
| SCR-003 | Submit Leave Request | ✅ Pass | ยื่นคำร้องและขึ้นใน list |
| SCR-004 | Manager Approval Queue | ✅ Pass | เห็น pending ของ EMP001 และ EMP003 หลัง fix ManagerId |
| SCR-005 | Leave History | ✅ Pass | แสดง history พร้อม status filter |
| SCR-006 | Cancel Leave Request | ⚠️ ทดสอบบางส่วน | Pending → cancel ทันที, Approved → CancelRequest ยังไม่ได้ทดสอบ E2E |
| SCR-007 | Approve/Reject Cancel | ⚠️ ทดสอบบางส่วน | tab "คำขอยกเลิก" ขึ้น แต่ยังไม่มีข้อมูลทดสอบ |
| SCR-008 | HR Dashboard | ✅ Pass | search + filter ทำงาน |
| SCR-009 | Settings / Import | ✅ Pass | upload ส่ง X-Employee-Id header |
| SCR-010 | Leave Report | ✅ Pass | filter + Export CSV ทำงาน |

---

## 6. ข้อจำกัดที่ต้องแจ้ง Reviewer

| # | ข้อจำกัด | ผลกระทบ | แนวทางแก้ไข |
|---|---------|---------|------------|
| L-001 | ใช้ Mock Auth (X-Employee-Id header) แทน JWT | ไม่มี token expiry, ไม่ secure สำหรับ production | ใส่ JWT ตาม SFR-015 ก่อน go-live |
| L-002 | Backend run บน HTTP ไม่ใช่ HTTPS | dev cert ยังไม่ได้ trust | รัน `dotnet dev-certs https --trust` หรือใช้ HTTP ใน dev |
| L-003 | Unit Test ยังไม่ได้เขียน | ไม่มี automated test coverage | ดู template ใน `80-knowledge-base/coding/03-unit-test/` |
| L-004 | SCR-006/007 (Cancel flow) ยังไม่ได้ทดสอบ E2E ครบ | Cancel Request → Manager approve/reject ยังไม่ได้ verify | ต้องทดสอบด้วย seed data ที่มี Approved leave request |
| L-005 | Email Notification เป็น Stub | ไม่ส่ง email จริง (IF-002) | ต่อ Azure Service Bus เมื่อ deploy |
| L-006 | SLA Scheduler (IF-005) error ตอน startup | เพราะ `IServiceProvider` ถูก dispose ก่อน scheduler ทำงานได้ | เป็น warning เท่านั้น ไม่กระทบ main flow |
| L-007 | ไม่มี role-based authorization บน API endpoint | HR สามารถเรียก Manager endpoint ได้ | ใส่ `[Authorize(Policy="ManagerOnly")]` ก่อน production |

---

## 7. Demo Accounts

| Email | Password | Role | EmployeeId |
|-------|----------|------|------------|
| somchai@abc.com | 1234 | Employee | EMP001 |
| wipa@abc.com | 1234 | Manager | EMP002 |
| nanta@abc.com | 1234 | HR | EMP003 |
