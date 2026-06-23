# SQL Draft — Leave Request and Approval System (Sample)

> ตัวอย่างนี้แสดง SQL DDL สำหรับ module "ยื่นคำร้องขอลา"  
> Tech Stack: SQLite ผ่าน EF Core Migrations  
> อ้างอิงจาก: `leave-request-and-approval-class-diagram.md`, `leave-request-and-approval-data-architecture-design.md`

---

## 1. Document Info

| รายการ | รายละเอียด |
|--------|-----------|
| Project | Leave Request and Approval System |
| Feature / Module | Leave Request Submission |
| Source Design | leave-request-and-approval-class-diagram.md |
| Generated Date | 2026-06-17 |
| Author | AI-generated, reviewed by team |

---

## 2. Assumptions

- ใช้ SQLite ผ่าน EF Core — schema ด้านล่างเป็น reference สำหรับ review เท่านั้น
- ไม่มี `DECIMAL` native ใน SQLite ใช้ `REAL` แทน
- DateTime เก็บเป็น TEXT รูปแบบ ISO 8601 UTC
- EF Core จัดการ AUTOINCREMENT และ FK ผ่าน Migration
- Foreign key enforcement ต้อง enable ผ่าน `PRAGMA foreign_keys = ON` ใน AppDbContext

---

## 3. Table Definitions

### 3.1 Employees

**Purpose:** เก็บข้อมูลพนักงานที่ใช้งานระบบ  
**Related Entity:** `Employee` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-001, SFR-002

```sql
CREATE TABLE IF NOT EXISTS Employees (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Identity
    EmployeeCode    TEXT            NOT NULL,   -- รหัสพนักงาน เช่น EMP001
    FirstName       TEXT            NOT NULL,
    LastName        TEXT            NOT NULL,
    Email           TEXT            NOT NULL,
    PhoneNumber     TEXT            NULL,

    -- Organization
    DepartmentId    INTEGER         NOT NULL,
    ManagerId       INTEGER         NULL,       -- FK → Employees.Id (self-reference)
    HrisEmployeeId  TEXT            NULL,       -- สำหรับ sync กับ HRIS API

    -- Status
    IsActive        INTEGER         NOT NULL DEFAULT 1,   -- 0 = inactive, 1 = active

    -- Audit Columns
    CreatedAt       TEXT            NOT NULL,
    CreatedBy       TEXT            NOT NULL,
    UpdatedAt       TEXT            NULL,
    UpdatedBy       TEXT            NULL
);
```

**Column Descriptions:**

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | INTEGER | NOT NULL | Auto-increment PK |
| EmployeeCode | TEXT | NOT NULL | รหัสพนักงาน unique ภายในระบบ |
| FirstName | TEXT | NOT NULL | ชื่อ |
| LastName | TEXT | NOT NULL | นามสกุล |
| Email | TEXT | NOT NULL | อีเมลสำหรับ login และ notification |
| DepartmentId | INTEGER | NOT NULL | FK → Departments.Id |
| ManagerId | INTEGER | NULL | FK → Employees.Id (self-reference หัวหน้า) |
| HrisEmployeeId | TEXT | NULL | รหัสใน HRIS สำหรับ sync balance |
| IsActive | INTEGER | NOT NULL | 1 = Active, 0 = Inactive |

---

### 3.2 Departments

**Purpose:** เก็บข้อมูลแผนกในองค์กร  
**Related Entity:** `Department` ใน `LeaveRequest.Domain.Entities`

```sql
CREATE TABLE IF NOT EXISTS Departments (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    DepartmentCode  TEXT            NOT NULL,
    DepartmentName  TEXT            NOT NULL,
    IsActive        INTEGER         NOT NULL DEFAULT 1,

    -- Audit Columns
    CreatedAt       TEXT            NOT NULL,
    CreatedBy       TEXT            NOT NULL,
    UpdatedAt       TEXT            NULL,
    UpdatedBy       TEXT            NULL
);
```

---

### 3.3 LeaveTypes

**Purpose:** เก็บประเภทการลา เช่น ลาพักร้อน, ลากิจ, ลาป่วย  
**Related Entity:** ใช้ร่วมกับ Enum `LeaveType` (อาจ normalize เป็น table หรือ enum ก็ได้ตาม design)

```sql
CREATE TABLE IF NOT EXISTS LeaveTypes (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Code            TEXT            NOT NULL,   -- ANNUAL, SICK, PERSONAL, MATERNITY
    Name            TEXT            NOT NULL,   -- ลาพักร้อน, ลาป่วย, ลากิจ, ลาคลอด
    MaxDaysPerYear  REAL            NOT NULL DEFAULT 0,
    IsActive        INTEGER         NOT NULL DEFAULT 1,

    -- Audit Columns
    CreatedAt       TEXT            NOT NULL,
    CreatedBy       TEXT            NOT NULL,
    UpdatedAt       TEXT            NULL,
    UpdatedBy       TEXT            NULL
);
```

---

### 3.4 LeaveBalances

**Purpose:** เก็บวันลาคงเหลือของพนักงานแต่ละคนในแต่ละปี  
**Related Entity:** `LeaveBalance` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-002 (ตรวจสอบวันลาคงเหลือ), VR-002

```sql
CREATE TABLE IF NOT EXISTS LeaveBalances (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    EmployeeId      INTEGER         NOT NULL,   -- FK → Employees.Id
    LeaveTypeId     INTEGER         NOT NULL,   -- FK → LeaveTypes.Id
    Year            INTEGER         NOT NULL,   -- ปี พ.ศ. หรือ ค.ศ. ตาม policy
    TotalDays       REAL            NOT NULL DEFAULT 0,
    UsedDays        REAL            NOT NULL DEFAULT 0,
    RemainingDays   REAL            NOT NULL DEFAULT 0,

    -- Audit Columns
    CreatedAt       TEXT            NOT NULL,
    CreatedBy       TEXT            NOT NULL,
    UpdatedAt       TEXT            NULL,
    UpdatedBy       TEXT            NULL
);
```

---

### 3.5 LeaveRequests

**Purpose:** เก็บคำร้องขอลาที่พนักงานยื่น  
**Related Entity:** `LeaveRequest` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-001 (ยื่นคำร้องขอลา), SFR-003 (อนุมัติ), SFR-004 (ปฏิเสธ)

```sql
CREATE TABLE IF NOT EXISTS LeaveRequests (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    EmployeeId      INTEGER         NOT NULL,   -- FK → Employees.Id (ผู้ยื่น)
    LeaveTypeId     INTEGER         NOT NULL,   -- FK → LeaveTypes.Id
    StartDate       TEXT            NOT NULL,   -- yyyy-MM-dd
    EndDate         TEXT            NOT NULL,   -- yyyy-MM-dd
    TotalDays       REAL            NOT NULL,
    Reason          TEXT            NOT NULL,
    Status          TEXT            NOT NULL DEFAULT 'Draft',
    -- Status values: Draft, PendingApproval, Approved, Rejected, Cancelled
    AttachmentPath  TEXT            NULL,       -- path ของไฟล์แนบ (ถ้ามี)

    -- Audit Columns
    CreatedAt       TEXT            NOT NULL,
    CreatedBy       TEXT            NOT NULL,
    UpdatedAt       TEXT            NULL,
    UpdatedBy       TEXT            NULL
);
```

**Allowed Status Values:** `Draft` → `PendingApproval` → `Approved` / `Rejected` → `Cancelled`

---

### 3.6 LeaveApprovals

**Purpose:** เก็บประวัติการอนุมัติ/ปฏิเสธของ approver แต่ละคน  
**Related Entity:** `LeaveApproval` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-003, SFR-004

```sql
CREATE TABLE IF NOT EXISTS LeaveApprovals (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    LeaveRequestId  INTEGER         NOT NULL,   -- FK → LeaveRequests.Id
    ApproverId      INTEGER         NOT NULL,   -- FK → Employees.Id
    Action          TEXT            NOT NULL,   -- Approved, Rejected
    Comment         TEXT            NULL,
    ActionAt        TEXT            NOT NULL,   -- UTC ISO 8601

    -- Audit Columns
    CreatedAt       TEXT            NOT NULL,
    CreatedBy       TEXT            NOT NULL,
    UpdatedAt       TEXT            NULL,
    UpdatedBy       TEXT            NULL
);
```

---

## 4. Foreign Key Constraints

```sql
-- LeaveBalances → Employees
-- LeaveBalances.EmployeeId → Employees.Id (RESTRICT on delete)

-- LeaveBalances → LeaveTypes
-- LeaveBalances.LeaveTypeId → LeaveTypes.Id (RESTRICT on delete)

-- LeaveRequests → Employees
-- LeaveRequests.EmployeeId → Employees.Id (RESTRICT on delete)

-- LeaveRequests → LeaveTypes
-- LeaveRequests.LeaveTypeId → LeaveTypes.Id (RESTRICT on delete)

-- LeaveApprovals → LeaveRequests
-- LeaveApprovals.LeaveRequestId → LeaveRequests.Id (CASCADE on delete)

-- LeaveApprovals → Employees (approver)
-- LeaveApprovals.ApproverId → Employees.Id (RESTRICT on delete)

-- Employees → Employees (self-reference manager)
-- Employees.ManagerId → Employees.Id (SET NULL on delete)

-- Note: EF Core จัดการ FK ผ่าน Migration
-- ใน AppDbContext ต้อง enable FK: PRAGMA foreign_keys = ON
```

---

## 5. Indexes

```sql
-- ค้นหา leave request ตาม employee (ใช้บ่อยใน "ดูประวัติการลาของฉัน")
CREATE INDEX IF NOT EXISTS IX_LeaveRequests_EmployeeId
    ON LeaveRequests (EmployeeId);

-- ค้นหา pending approval สำหรับ manager
CREATE INDEX IF NOT EXISTS IX_LeaveRequests_Status
    ON LeaveRequests (Status);

-- composite index สำหรับ leave balance lookup (ใช้บ่อยมาก)
CREATE UNIQUE INDEX IF NOT EXISTS UX_LeaveBalances_Employee_Type_Year
    ON LeaveBalances (EmployeeId, LeaveTypeId, Year);

-- ค้นหา approval history ตาม request
CREATE INDEX IF NOT EXISTS IX_LeaveApprovals_LeaveRequestId
    ON LeaveApprovals (LeaveRequestId);

-- employee lookup ด้วย email (สำหรับ login)
CREATE UNIQUE INDEX IF NOT EXISTS UX_Employees_Email
    ON Employees (Email);
```

---

## 6. Seed Data

```sql
-- LeaveTypes seed data
INSERT INTO LeaveTypes (Code, Name, MaxDaysPerYear, IsActive, CreatedAt, CreatedBy)
VALUES
    ('ANNUAL',     'ลาพักร้อน', 10.0, 1, datetime('now'), 'system'),
    ('SICK',       'ลาป่วย',    30.0, 1, datetime('now'), 'system'),
    ('PERSONAL',   'ลากิจ',      3.0, 1, datetime('now'), 'system'),
    ('MATERNITY',  'ลาคลอด',    90.0, 1, datetime('now'), 'system');

-- Departments seed data
INSERT INTO Departments (DepartmentCode, DepartmentName, IsActive, CreatedAt, CreatedBy)
VALUES
    ('IT',  'Information Technology', 1, datetime('now'), 'system'),
    ('HR',  'Human Resources',        1, datetime('now'), 'system'),
    ('FIN', 'Finance',                1, datetime('now'), 'system');
```

---

## 7. EF Core Migration Note

```bash
# สร้าง initial migration
dotnet ef migrations add InitialCreate \
  --project LeaveRequest.Infrastructure \
  --startup-project LeaveRequest.API

# Apply migration
dotnet ef database update \
  --project LeaveRequest.Infrastructure \
  --startup-project LeaveRequest.API
```

---

## 8. Open Issues

| # | Issue | Status |
|---|-------|--------|
| 1 | `TotalDays` ควร compute จาก StartDate/EndDate หรือ input จาก user — ต้องกำหนด rule ชัดเจน | Open |
| 2 | ระบบรองรับ multi-level approval (หลายคน approve ตามลำดับ) หรือ single approver — กระทบ `LeaveApprovals` schema | Open |
| 3 | Soft delete vs hard delete — ตอนนี้ใช้ `IsActive` สำหรับ Employee/Department เท่านั้น | Open |
