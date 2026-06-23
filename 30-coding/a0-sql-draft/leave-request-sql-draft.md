---
title: "SQL DDL Draft — Leave Request and Approval"
document_type: "SQL Draft"
version: "1.0"
date: "2026-06-18"
project: "ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)"
company: "ABC Company"
tech_stack: "SQLite ผ่าน EF Core (ห้ามใช้ SQL Server-specific syntax)"
status: "Draft"
---

# SQL DDL Draft: ระบบบริหารการลาและการอนุมัติ

---

## 1. Document Info

| รายการ | รายละเอียด |
|--------|-----------|
| Project | Leave Request and Approval System — ABC Company |
| Feature / Module | All modules (Full Schema) |
| Source Design | `leave-request-and-approval-class-diagram.md` v1.0, `leave-request-and-approval-data-architecture-design.md` v1.0 |
| Generated Date | 2026-06-18 |
| Author | AI-generated (Claude), reviewed by team |
| Tables | 9 tables (6 mutable + 3 immutable log) |

---

## 2. Assumptions

- **A-001:** SQLite ผ่าน EF Core — SQL นี้ใช้เพื่อ review และ reference เท่านั้น; schema จริง generate ผ่าน `dotnet ef migrations add`
- **A-002:** `enum` เก็บเป็น `TEXT` (ชื่อ enum value) ตาม data type rules — EF Core ใช้ `.HasConversion<string>()`
- **A-003:** `decimal` เก็บเป็น `REAL` ตาม data type rules; อาจเสีย precision สำหรับ 0.5 วัน — พิจารณาเปลี่ยนเป็น `TEXT` ถ้า precision สำคัญ (ดู OI-002)
- **A-004:** `DateTime` เก็บเป็น `TEXT` รูปแบบ ISO 8601 UTC (`2026-06-18T10:00:00Z`)
- **A-005:** `DateOnly` เก็บเป็น `TEXT` รูปแบบ `yyyy-MM-dd` (`2026-06-18`)
- **A-006:** `Guid` เก็บเป็น `TEXT` (36 chars, format `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`) — EF Core จัดการ auto-convert
- **A-007:** `Employee.EmployeeId` เป็น `TEXT PRIMARY KEY` (string จาก HRIS เช่น `EMP001`) — ไม่ใช่ AUTOINCREMENT
- **A-008:** `LeaveType.LeaveTypeId` เป็น `INTEGER PRIMARY KEY AUTOINCREMENT` (byte → INTEGER)
- **A-009:** FK enforcement ต้องเปิดผ่าน `PRAGMA foreign_keys = ON` ใน AppDbContext (SQLite default = OFF)
- **A-010:** Soft delete บน mutable entities — ใช้ Global Query Filter `WHERE IsDeleted = 0` ใน EF Core
- **A-011:** Immutable entities (ApprovalHistories, NotificationLogs, ImportLogs) ไม่มี UpdatedAt/IsDeleted — ห้าม UPDATE/DELETE row
- **A-012:** Seed data LeaveTypes ใช้ไทย/อังกฤษชื่อตาม SRS Summary — MaxDaysPerYear บางประเภทรอ HR ยืนยัน (OI-007)
- **A-013:** LeaveYear ใน LeaveBalances เป็น ค.ศ. (Gregorian) — รอ HR ยืนยัน (OI-008)

---

## 3. Table Definitions

### 3.1 LeaveTypes (Master Data)

**Purpose:** เก็บ 7 ประเภทการลา — lookup table สำหรับ validate LeaveRequest และ LeaveBalance  
**Related Entity:** `LeaveType` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-003, VR-001, BRD BR-011  
**EF Behavior:** Mutable, Soft Delete, Global Query Filter (`IsDeleted = 0`)

```sql
CREATE TABLE IF NOT EXISTS LeaveTypes (
    LeaveTypeId             INTEGER         PRIMARY KEY AUTOINCREMENT,

    -- Identity
    TypeCode                TEXT            NOT NULL,   -- ANNUAL, SICK, PERSONAL, MATERNITY, STERILIZATION, ORDINATION, MILITARY
    TypeNameTh              TEXT            NOT NULL,   -- ชื่อภาษาไทย
    TypeNameEn              TEXT            NOT NULL,   -- ชื่อภาษาอังกฤษ

    -- Config
    MaxDaysPerYear          REAL            NULL,       -- NULL = ไม่จำกัด หรือรอ HR ยืนยัน (OI-007)
    IsAvailableForOutsource INTEGER         NOT NULL DEFAULT 0,   -- 0=false, 1=true (VR-001)
    RequiresMedicalCert     INTEGER         NOT NULL DEFAULT 0,   -- 0=false, 1=true (VR-007)

    -- Audit Columns (Mutable)
    CreatedAt               TEXT            NOT NULL,   -- UTC ISO 8601
    CreatedBy               TEXT            NOT NULL,
    UpdatedAt               TEXT            NULL,
    UpdatedBy               TEXT            NULL,
    IsDeleted               INTEGER         NOT NULL DEFAULT 0,
    DeletedAt               TEXT            NULL,
    DeletedBy               TEXT            NULL
);
```

**Column Descriptions:**

| Column | SQLite Type | Nullable | Description |
|--------|-------------|----------|-------------|
| LeaveTypeId | INTEGER | NOT NULL | PK Auto-increment (byte → INTEGER) |
| TypeCode | TEXT | NOT NULL | รหัสประเภทลา — UNIQUE |
| TypeNameTh | TEXT | NOT NULL | ชื่อภาษาไทย เช่น "ลาพักผ่อน" |
| TypeNameEn | TEXT | NOT NULL | ชื่อภาษาอังกฤษ เช่น "Annual Leave" |
| MaxDaysPerYear | REAL | NULL | สิทธิ์สูงสุดต่อปี; NULL = ไม่จำกัด |
| IsAvailableForOutsource | INTEGER | NOT NULL | 1 = Outsource ใช้สิทธิ์นี้ได้ (VR-001) |
| RequiresMedicalCert | INTEGER | NOT NULL | 1 = ต้องแนบใบรับรองแพทย์ (VR-007) |
| IsDeleted | INTEGER | NOT NULL | 0 = active, 1 = soft-deleted |

---

### 3.2 Employees (Master Data)

**Purpose:** พนักงานทุกคน — Regular (จาก HRIS IF-001) และ Outsource (จาก Excel Import IF-003) รวมในตารางเดียว  
**Related Entity:** `Employee` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-001, SIR-001, SIR-003, BRD BR-011, NFR-005/006  
**EF Behavior:** Mutable, Soft Delete, Global Query Filter, Self-reference (ManagerId)

```sql
CREATE TABLE IF NOT EXISTS Employees (
    EmployeeId              TEXT            PRIMARY KEY,    -- string PK จาก HRIS เช่น "EMP001"

    -- Identity
    EmployeeCode            TEXT            NOT NULL,   -- รหัสพนักงาน (อาจซ้ำกับ EmployeeId หรือแยก)
    FullNameTh              TEXT            NOT NULL,
    FullNameEn              TEXT            NOT NULL,

    -- Organization
    Department              TEXT            NULL,       -- ชื่อแผนก (string, ไม่ใช่ FK — ดู Assumption A-014)
    Position                TEXT            NULL,
    Email                   TEXT            NOT NULL,   -- UNIQUE (UQ_Employees_Email)
    HireDate                TEXT            NOT NULL,   -- yyyy-MM-dd (DateOnly → TEXT)
    ManagerId               TEXT            NULL,       -- FK → Employees.EmployeeId (self-reference)

    -- Employee Type
    EmployeeType            TEXT            NOT NULL,   -- 'Regular' | 'Outsource' (enum → TEXT)
    AgencyCompany           TEXT            NULL,       -- สำหรับ Outsource เท่านั้น
    AbcStartDate            TEXT            NULL,       -- yyyy-MM-dd — วันเริ่มงานที่ ABC (Outsource)

    -- Status
    IsActive                INTEGER         NOT NULL DEFAULT 1,
    LastSyncedAt            TEXT            NULL,       -- UTC — วันที่ sync จาก HRIS ล่าสุด

    -- Audit Columns (Mutable)
    CreatedAt               TEXT            NOT NULL,
    CreatedBy               TEXT            NOT NULL,
    UpdatedAt               TEXT            NULL,
    UpdatedBy               TEXT            NULL,
    IsDeleted               INTEGER         NOT NULL DEFAULT 0,
    DeletedAt               TEXT            NULL,
    DeletedBy               TEXT            NULL,

    FOREIGN KEY (ManagerId) REFERENCES Employees(EmployeeId)
        ON DELETE RESTRICT ON UPDATE CASCADE
);
```

**Column Descriptions:**

| Column | SQLite Type | Nullable | Description |
|--------|-------------|----------|-------------|
| EmployeeId | TEXT | NOT NULL | PK string จาก HRIS เช่น "EMP001" |
| EmployeeCode | TEXT | NOT NULL | รหัสพนักงานภายใน (ซ้ำหรือแยกจาก EmployeeId) |
| FullNameTh | TEXT | NOT NULL | ชื่อ-นามสกุลภาษาไทย |
| FullNameEn | TEXT | NOT NULL | ชื่อ-นามสกุลภาษาอังกฤษ |
| Department | TEXT | NULL | ชื่อแผนก (string — ไม่มี Departments master table ตาม class diagram) |
| Email | TEXT | NOT NULL | UNIQUE — ใช้ notify และ login |
| HireDate | TEXT | NOT NULL | DateOnly → TEXT `yyyy-MM-dd` |
| ManagerId | TEXT | NULL | Self-reference FK → Employees.EmployeeId |
| EmployeeType | TEXT | NOT NULL | 'Regular' หรือ 'Outsource' |
| AgencyCompany | TEXT | NULL | ชื่อบริษัทของ Outsource |
| AbcStartDate | TEXT | NULL | DateOnly → TEXT; วันที่เริ่มงาน ABC (Outsource) |
| IsActive | INTEGER | NOT NULL | 0 = inactive, 1 = active |
| LastSyncedAt | TEXT | NULL | UTC timestamp ที่ sync ล่าสุดจาก HRIS |

---

### 3.3 LeaveRequests (Transaction)

**Purpose:** Entity หลัก — คำขอลาของพนักงาน ติดตาม Status ตลอด lifecycle  
**Related Entity:** `LeaveRequest` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-003/004/005/006/007/008, VR-001–007, NFR-010  
**EF Behavior:** Mutable, Soft Delete, Transactional update (พร้อมกับ LeaveBalance)

```sql
CREATE TABLE IF NOT EXISTS LeaveRequests (
    LeaveRequestId          TEXT            PRIMARY KEY,    -- Guid → TEXT

    -- Reference
    LeaveRequestRef         TEXT            NOT NULL,   -- รหัส ref เช่น "LR2026-0001" (UNIQUE)
    EmployeeId              TEXT            NOT NULL,   -- FK → Employees.EmployeeId
    LeaveTypeId             INTEGER         NOT NULL,   -- FK → LeaveTypes.LeaveTypeId

    -- Leave Period
    StartDate               TEXT            NOT NULL,   -- yyyy-MM-dd (DateOnly → TEXT)
    EndDate                 TEXT            NOT NULL,   -- yyyy-MM-dd (DateOnly → TEXT)
    DurationDays            REAL            NOT NULL,   -- จำนวนวันลา (0.5 = ครึ่งวัน)
    IsHalfDay               INTEGER         NOT NULL DEFAULT 0,   -- 0=false, 1=true
    HalfDayPeriod           TEXT            NULL,       -- 'Morning' | 'Afternoon' | NULL

    -- Content
    Reason                  TEXT            NULL,
    Status                  TEXT            NOT NULL DEFAULT 'Pending',
    -- Status values: 'Pending' | 'Approved' | 'Rejected' | 'Cancelled' | 'CancelRequested' | 'Escalated'

    -- Approval Tracking
    ApprovedBy              TEXT            NULL,       -- EmployeeId ผู้อนุมัติ
    ApprovedAt              TEXT            NULL,       -- UTC ISO 8601
    RejectedBy              TEXT            NULL,       -- EmployeeId ผู้ปฏิเสธ
    RejectedAt              TEXT            NULL,
    RejectionReason         TEXT            NULL,

    -- Audit Columns (Mutable)
    CreatedAt               TEXT            NOT NULL,
    CreatedBy               TEXT            NOT NULL,
    UpdatedAt               TEXT            NULL,
    UpdatedBy               TEXT            NULL,
    IsDeleted               INTEGER         NOT NULL DEFAULT 0,
    DeletedAt               TEXT            NULL,
    DeletedBy               TEXT            NULL,

    FOREIGN KEY (EmployeeId)  REFERENCES Employees(EmployeeId)  ON DELETE RESTRICT,
    FOREIGN KEY (LeaveTypeId) REFERENCES LeaveTypes(LeaveTypeId) ON DELETE RESTRICT
);
```

**Status Transition:**
```
Pending → Approved → CancelRequested → Cancelled
Pending → Rejected
Approved → Escalated  (via SLA Scheduler — SFR-010)
```

---

### 3.4 LeaveBalances (Transaction)

**Purpose:** สิทธิ์วันลาคงเหลือต่อพนักงาน / ประเภท / ปี — ต้อง update พร้อมกับ LeaveRequest ใน transaction เดียว  
**Related Entity:** `LeaveBalance` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-002, VR-002, NFR-010, BRD BR-002/016  
**EF Behavior:** Mutable, Soft Delete, Unique constraint (EmployeeId + LeaveTypeId + LeaveYear)

```sql
CREATE TABLE IF NOT EXISTS LeaveBalances (
    LeaveBalanceId          TEXT            PRIMARY KEY,    -- Guid → TEXT

    -- Key (UNIQUE combination)
    EmployeeId              TEXT            NOT NULL,   -- FK → Employees.EmployeeId
    LeaveTypeId             INTEGER         NOT NULL,   -- FK → LeaveTypes.LeaveTypeId
    LeaveYear               INTEGER         NOT NULL,   -- ค.ศ. เช่น 2026 (รอ HR ยืนยัน OI-008)

    -- Balance Breakdown
    EntitledDays            REAL            NOT NULL DEFAULT 0,   -- สิทธิ์ตามประเภท/อายุงาน
    UsedDays                REAL            NOT NULL DEFAULT 0,   -- วันที่ใช้ไปแล้ว (Approved)
    PendingDays             REAL            NOT NULL DEFAULT 0,   -- รออนุมัติ
    CarriedForwardDays      REAL            NOT NULL DEFAULT 0,   -- ยกยอดมาจากปีก่อน (VR-008 cap 30 วัน)

    -- Audit Columns (Mutable)
    CreatedAt               TEXT            NOT NULL,
    CreatedBy               TEXT            NOT NULL,
    UpdatedAt               TEXT            NULL,
    UpdatedBy               TEXT            NULL,
    IsDeleted               INTEGER         NOT NULL DEFAULT 0,
    DeletedAt               TEXT            NULL,
    DeletedBy               TEXT            NULL,

    FOREIGN KEY (EmployeeId)  REFERENCES Employees(EmployeeId)  ON DELETE RESTRICT,
    FOREIGN KEY (LeaveTypeId) REFERENCES LeaveTypes(LeaveTypeId) ON DELETE RESTRICT
);
```

**Derived Value:** `RemainingDays = EntitledDays + CarriedForwardDays - UsedDays - PendingDays` — คำนวณใน Service ไม่เก็บใน DB

---

### 3.5 CancelRequests (Transaction)

**Purpose:** คำขอยกเลิกสำหรับ Leave ที่ Approved แล้ว — มี SLA Deadline tracking สำหรับ `SlaSchedulerService`  
**Related Entity:** `CancelRequest` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-008/009/010, VR-012, NFR-011, SIR-004, BRD BR-014/015  
**EF Behavior:** Mutable, Soft Delete; SLA Scheduler queries `SlaDeadline` + `Status = 'Pending'`

```sql
CREATE TABLE IF NOT EXISTS CancelRequests (
    CancelRequestId         TEXT            PRIMARY KEY,    -- Guid → TEXT

    -- Reference
    CancelRequestRef        TEXT            NOT NULL,   -- รหัส ref เช่น "CR2026-0001" (UNIQUE)
    LeaveRequestId          TEXT            NOT NULL,   -- FK → LeaveRequests.LeaveRequestId
    RequestedBy             TEXT            NOT NULL,   -- EmployeeId ผู้ขอยกเลิก

    -- Content
    Reason                  TEXT            NULL,
    Status                  TEXT            NOT NULL DEFAULT 'Pending',
    -- Status values: 'Pending' | 'Approved' | 'Rejected' | 'Escalated'

    -- SLA Tracking (SFR-010, NFR-011)
    SlaDeadline             TEXT            NOT NULL,   -- UTC — deadline 1 วันทำการ
    SlaReminderSentAt       TEXT            NULL,       -- UTC — เมื่อส่ง reminder email (-4h)
    SlaEscalatedAt          TEXT            NULL,       -- UTC — เมื่อ escalate ไป HR

    -- Approval Result
    ApprovedBy              TEXT            NULL,
    ApprovedAt              TEXT            NULL,
    RejectedBy              TEXT            NULL,
    RejectedAt              TEXT            NULL,

    -- Audit Columns (Mutable)
    CreatedAt               TEXT            NOT NULL,
    CreatedBy               TEXT            NOT NULL,
    UpdatedAt               TEXT            NULL,
    UpdatedBy               TEXT            NULL,
    IsDeleted               INTEGER         NOT NULL DEFAULT 0,
    DeletedAt               TEXT            NULL,
    DeletedBy               TEXT            NULL,

    FOREIGN KEY (LeaveRequestId) REFERENCES LeaveRequests(LeaveRequestId) ON DELETE RESTRICT
);
```

---

### 3.6 Attachments (Transaction)

**Purpose:** Metadata ของใบรับรองแพทย์ — ไฟล์จริงเก็บใน Azure Blob Storage; StoragePath = Blob path/URL  
**Related Entity:** `Attachment` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SIR-005, IF-004, VR-007, TR-005  
**EF Behavior:** Mutable, Soft Delete

```sql
CREATE TABLE IF NOT EXISTS Attachments (
    AttachmentId            TEXT            PRIMARY KEY,    -- Guid → TEXT

    LeaveRequestId          TEXT            NOT NULL,   -- FK → LeaveRequests.LeaveRequestId
    FileName                TEXT            NOT NULL,   -- ชื่อไฟล์เดิม เช่น "medical_cert.pdf"
    StoragePath             TEXT            NOT NULL,   -- Azure Blob path/URL
    FileType                TEXT            NOT NULL,   -- MIME type เช่น "application/pdf"
    FileSizeBytes           INTEGER         NOT NULL,   -- ขนาดไฟล์ (long → INTEGER)
    UploadedBy              TEXT            NOT NULL,   -- EmployeeId

    -- Audit Columns (Mutable)
    CreatedAt               TEXT            NOT NULL,
    CreatedBy               TEXT            NOT NULL,
    UpdatedAt               TEXT            NULL,
    UpdatedBy               TEXT            NULL,
    IsDeleted               INTEGER         NOT NULL DEFAULT 0,
    DeletedAt               TEXT            NULL,
    DeletedBy               TEXT            NULL,

    FOREIGN KEY (LeaveRequestId) REFERENCES LeaveRequests(LeaveRequestId) ON DELETE RESTRICT
);
```

---

### 3.7 ApprovalHistories (Immutable Log)

**Purpose:** Immutable log ของ Approve/Reject action ทั้ง LeaveRequest และ CancelRequest — ห้าม UPDATE/DELETE  
**Related Entity:** `ApprovalHistory` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-005/009, BRD BR-012  
**EF Behavior:** **Immutable** — CreatedAt/CreatedBy เท่านั้น; ไม่มี UpdatedAt/IsDeleted

```sql
CREATE TABLE IF NOT EXISTS ApprovalHistories (
    ApprovalHistoryId       TEXT            PRIMARY KEY,    -- Guid → TEXT

    -- XOR FK: ต้องมีแค่ FK เดียวใน แต่ละ record (enforce ที่ Application Layer — OI-004)
    LeaveRequestId          TEXT            NULL,       -- FK → LeaveRequests.LeaveRequestId (nullable)
    CancelRequestId         TEXT            NULL,       -- FK → CancelRequests.CancelRequestId (nullable)

    -- Action
    ApproverId              TEXT            NOT NULL,   -- EmployeeId ผู้ approve/reject
    Action                  TEXT            NOT NULL,   -- 'Approved' | 'Rejected' (enum → TEXT)
    Reason                  TEXT            NULL,       -- เหตุผล Reject
    ActionAt                TEXT            NOT NULL,   -- UTC ISO 8601

    -- Audit Columns (Immutable — CreatedAt/CreatedBy เท่านั้น)
    CreatedAt               TEXT            NOT NULL,
    CreatedBy               TEXT            NOT NULL,

    FOREIGN KEY (LeaveRequestId)  REFERENCES LeaveRequests(LeaveRequestId)  ON DELETE RESTRICT,
    FOREIGN KEY (CancelRequestId) REFERENCES CancelRequests(CancelRequestId) ON DELETE RESTRICT
);
```

> **หมายเหตุ XOR FK:** `LeaveRequestId` และ `CancelRequestId` nullable ทั้งคู่ — Application Layer ต้อง enforce ว่าต้องมีค่าแค่ FK เดียว (SQLite ไม่มี CHECK constraint แบบ XOR — OI-004)

---

### 3.8 NotificationLogs (Immutable Log)

**Purpose:** Immutable log การส่ง Email notification ทุก event — ติดตาม DeliveryStatus, RetryCount  
**Related Entity:** `NotificationLog` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-013, NFR-007, IF-002, RFR-003  
**EF Behavior:** **Immutable** — CreatedAt/CreatedBy เท่านั้น

```sql
CREATE TABLE IF NOT EXISTS NotificationLogs (
    NotificationLogId       TEXT            PRIMARY KEY,    -- Guid → TEXT

    -- XOR FK: ต้องมีแค่ FK เดียวใน แต่ละ record (enforce ที่ Application Layer — OI-005)
    LeaveRequestId          TEXT            NULL,       -- FK → LeaveRequests.LeaveRequestId (nullable)
    CancelRequestId         TEXT            NULL,       -- FK → CancelRequests.CancelRequestId (nullable)

    -- Notification Content
    EventType               TEXT            NOT NULL,   -- เช่น 'LeaveSubmitted', 'LeaveApproved', 'SlaReminder', 'SlaEscalated'
    RecipientEmail          TEXT            NOT NULL,
    RecipientRole           TEXT            NOT NULL,   -- 'Employee' | 'Manager' | 'HR'

    -- Delivery Tracking (NFR-007: retry ≥ 3 ครั้ง)
    DeliveryStatus          TEXT            NOT NULL DEFAULT 'Pending',
    -- Status values: 'Pending' | 'Success' | 'Failed'
    RetryCount              INTEGER         NOT NULL DEFAULT 0,
    SentAt                  TEXT            NULL,       -- UTC — เมื่อส่งสำเร็จ
    FailureReason           TEXT            NULL,       -- รายละเอียด error ถ้า Failed

    -- Audit Columns (Immutable)
    CreatedAt               TEXT            NOT NULL,
    CreatedBy               TEXT            NOT NULL,

    FOREIGN KEY (LeaveRequestId)  REFERENCES LeaveRequests(LeaveRequestId)  ON DELETE RESTRICT,
    FOREIGN KEY (CancelRequestId) REFERENCES CancelRequests(CancelRequestId) ON DELETE RESTRICT
);
```

---

### 3.9 ImportLogs (Immutable Log)

**Purpose:** Immutable log ผล Excel import ทุกครั้ง (Outsource onboarding) — ErrorDetails เก็บ JSON  
**Related Entity:** `ImportLog` ใน `LeaveRequest.Domain.Entities`  
**Related SRS:** SFR-012, IF-003, TR-006, VR-013  
**EF Behavior:** **Immutable** — CreatedAt/CreatedBy เท่านั้น; ไม่มี FK (standalone log)

```sql
CREATE TABLE IF NOT EXISTS ImportLogs (
    ImportLogId             TEXT            PRIMARY KEY,    -- Guid → TEXT

    -- Import Metadata
    ImportedBy              TEXT            NOT NULL,   -- EmployeeId (HR ผู้ import)
    FileName                TEXT            NOT NULL,   -- ชื่อไฟล์ .xlsx ที่ import

    -- Result Summary
    TotalRecords            INTEGER         NOT NULL DEFAULT 0,
    SuccessRecords          INTEGER         NOT NULL DEFAULT 0,
    FailedRecords           INTEGER         NOT NULL DEFAULT 0,
    ErrorDetails            TEXT            NULL,       -- JSON array ของ error รายแถว

    -- Audit Columns (Immutable)
    CreatedAt               TEXT            NOT NULL,
    CreatedBy               TEXT            NOT NULL
);
```

---

## 4. Foreign Key Constraints

```sql
-- หมายเหตุ: FK ทุกตัวระบุใน CREATE TABLE ข้างต้นแล้ว
-- ต้อง enable FK enforcement ใน AppDbContext:

-- AppDbContext.cs OnConfiguring:
-- optionsBuilder.UseSqlite(connectionString, x => x.CommandTimeout(30));
-- และใน OnModelCreating หรือผ่าน event:
-- Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");

-- Summary ของ FK ทั้งหมด:
-- Employees.ManagerId              → Employees.EmployeeId          (RESTRICT, self-ref)
-- LeaveRequests.EmployeeId         → Employees.EmployeeId          (RESTRICT)
-- LeaveRequests.LeaveTypeId        → LeaveTypes.LeaveTypeId        (RESTRICT)
-- LeaveBalances.EmployeeId         → Employees.EmployeeId          (RESTRICT)
-- LeaveBalances.LeaveTypeId        → LeaveTypes.LeaveTypeId        (RESTRICT)
-- CancelRequests.LeaveRequestId    → LeaveRequests.LeaveRequestId  (RESTRICT)
-- Attachments.LeaveRequestId       → LeaveRequests.LeaveRequestId  (RESTRICT)
-- ApprovalHistories.LeaveRequestId → LeaveRequests.LeaveRequestId  (RESTRICT, nullable)
-- ApprovalHistories.CancelRequestId→ CancelRequests.CancelRequestId(RESTRICT, nullable)
-- NotificationLogs.LeaveRequestId  → LeaveRequests.LeaveRequestId  (RESTRICT, nullable)
-- NotificationLogs.CancelRequestId → CancelRequests.CancelRequestId(RESTRICT, nullable)
```

---

## 5. Indexes

```sql
-- ═══ LeaveTypes ═══════════════════════════════════════════════════════════════
-- TypeCode เป็น lookup key — query บ่อยมาก
CREATE UNIQUE INDEX IF NOT EXISTS UQ_LeaveTypes_TypeCode
    ON LeaveTypes (TypeCode);


-- ═══ Employees ════════════════════════════════════════════════════════════════
-- Email เป็น UNIQUE — login/notification lookup
CREATE UNIQUE INDEX IF NOT EXISTS UQ_Employees_Email
    ON Employees (Email);

-- EmployeeCode unique ภายในระบบ
CREATE UNIQUE INDEX IF NOT EXISTS UQ_Employees_EmployeeCode
    ON Employees (EmployeeCode);

-- Manager lookup (self-reference traversal)
CREATE INDEX IF NOT EXISTS IX_Employees_ManagerId
    ON Employees (ManagerId);

-- EmployeeType filter (Regular/Outsource) — VR-001
CREATE INDEX IF NOT EXISTS IX_Employees_EmployeeType
    ON Employees (EmployeeType);


-- ═══ LeaveRequests ════════════════════════════════════════════════════════════
-- Employee ดูประวัติการลาของตัวเอง — SFR-006, SCR-005
CREATE INDEX IF NOT EXISTS IX_LeaveRequests_EmployeeId
    ON LeaveRequests (EmployeeId);

-- Manager ดู pending approval inbox — SFR-004, SCR-004
CREATE INDEX IF NOT EXISTS IX_LeaveRequests_Status
    ON LeaveRequests (Status);

-- HR Monitor กรองตาม Status + วันที่ — SFR-011, SCR-008
CREATE INDEX IF NOT EXISTS IX_LeaveRequests_Status_StartDate
    ON LeaveRequests (Status, StartDate);

-- LeaveRequestRef สำหรับ lookup แบบ reference number
CREATE UNIQUE INDEX IF NOT EXISTS UQ_LeaveRequests_LeaveRequestRef
    ON LeaveRequests (LeaveRequestRef);

-- Overlap check: EmployeeId + วันที่ — VR-004 (ตรวจสอบวันลาซ้อนทับ)
CREATE INDEX IF NOT EXISTS IX_LeaveRequests_EmployeeId_StartDate_EndDate
    ON LeaveRequests (EmployeeId, StartDate, EndDate);


-- ═══ LeaveBalances ════════════════════════════════════════════════════════════
-- UNIQUE: พนักงาน 1 คน/ประเภท/ปี มีแค่ 1 record — NFR-010
CREATE UNIQUE INDEX IF NOT EXISTS UQ_LeaveBalances_Employee_Type_Year
    ON LeaveBalances (EmployeeId, LeaveTypeId, LeaveYear);


-- ═══ CancelRequests ═══════════════════════════════════════════════════════════
-- SLA Scheduler query: Pending + SlaDeadline ≤ NOW — SFR-010, NFR-011
CREATE INDEX IF NOT EXISTS IX_CancelRequests_Status_SlaDeadline
    ON CancelRequests (Status, SlaDeadline);

-- Parent LeaveRequest lookup
CREATE INDEX IF NOT EXISTS IX_CancelRequests_LeaveRequestId
    ON CancelRequests (LeaveRequestId);

-- CancelRequestRef unique
CREATE UNIQUE INDEX IF NOT EXISTS UQ_CancelRequests_CancelRequestRef
    ON CancelRequests (CancelRequestRef);


-- ═══ Attachments ══════════════════════════════════════════════════════════════
-- ดึง attachment ตาม LeaveRequest
CREATE INDEX IF NOT EXISTS IX_Attachments_LeaveRequestId
    ON Attachments (LeaveRequestId);


-- ═══ ApprovalHistories ════════════════════════════════════════════════════════
-- ดูประวัติ approval ของ LeaveRequest
CREATE INDEX IF NOT EXISTS IX_ApprovalHistories_LeaveRequestId
    ON ApprovalHistories (LeaveRequestId);

-- ดูประวัติ approval ของ CancelRequest
CREATE INDEX IF NOT EXISTS IX_ApprovalHistories_CancelRequestId
    ON ApprovalHistories (CancelRequestId);


-- ═══ NotificationLogs ═════════════════════════════════════════════════════════
-- Idempotent check: query log ตาม LeaveRequestId + EventType (IF-002)
CREATE INDEX IF NOT EXISTS IX_NotificationLogs_LeaveRequestId_EventType
    ON NotificationLogs (LeaveRequestId, EventType);

-- Retry: query Failed notifications — NFR-007
CREATE INDEX IF NOT EXISTS IX_NotificationLogs_DeliveryStatus
    ON NotificationLogs (DeliveryStatus);


-- ═══ ImportLogs ═══════════════════════════════════════════════════════════════
-- HR ดู import history ตามวันที่
CREATE INDEX IF NOT EXISTS IX_ImportLogs_CreatedAt
    ON ImportLogs (CreatedAt);
```

---

## 6. Seed Data

```sql
-- ═══ LeaveTypes: 7 ประเภทลาตาม SRS Summary ══════════════════════════════════
-- หมายเหตุ: MaxDaysPerYear ของ LeaveType 4-7 รอ HR ยืนยัน (OI-007)

INSERT INTO LeaveTypes (
    TypeCode, TypeNameTh, TypeNameEn,
    MaxDaysPerYear, IsAvailableForOutsource, RequiresMedicalCert,
    CreatedAt, CreatedBy, IsDeleted
)
VALUES
    -- LT-01: ลาพักผ่อน (Regular only, ไม่ต้องใบรับรองแพทย์)
    ('ANNUAL',         'ลาพักผ่อน',        'Annual Leave',        10.0, 0, 0, datetime('now'), 'SYSTEM', 0),
    -- LT-02: ลาป่วย (ทั้ง Regular และ Outsource, ป่วย ≥ 3 วันต้องใบรับรองแพทย์ — VR-007)
    ('SICK',           'ลาป่วย',            'Sick Leave',          30.0, 1, 0, datetime('now'), 'SYSTEM', 0),
    -- LT-03: ลากิจส่วนตัว (Regular only, ไม่ต้องใบรับรองแพทย์)
    ('PERSONAL',       'ลากิจส่วนตัว',      'Personal Leave',       3.0, 0, 0, datetime('now'), 'SYSTEM', 0),
    -- LT-04: ลาคลอด (Regular female only — VR-001 เพิ่มเติม; รอ HR ยืนยัน MaxDays — OI-007)
    ('MATERNITY',      'ลาคลอด',            'Maternity Leave',     98.0, 0, 1, datetime('now'), 'SYSTEM', 0),
    -- LT-05: ลาทำหมัน (รอ HR ยืนยัน MaxDays — OI-007)
    ('STERILIZATION',  'ลาทำหมัน',          'Sterilization Leave',  NULL, 0, 1, datetime('now'), 'SYSTEM', 0),
    -- LT-06: ลาอุปสมบท (Regular male only; รอ HR ยืนยัน MaxDays — OI-007)
    ('ORDINATION',     'ลาอุปสมบท',         'Ordination Leave',    15.0, 0, 0, datetime('now'), 'SYSTEM', 0),
    -- LT-07: ลารับราชการทหาร (Regular male only; MaxDays = NULL = ตามที่ราชการกำหนด — OI-007)
    ('MILITARY',       'ลารับราชการทหาร',   'Military Service Leave', NULL, 0, 0, datetime('now'), 'SYSTEM', 0);
```

---

## 7. EF Core Migration Note

> SQL Draft นี้ใช้เพื่อ review และ reference เท่านั้น  
> Schema จริงให้ generate ผ่าน EF Core Migration:

```bash
# สร้าง initial migration (รันจาก root solution directory)
dotnet ef migrations add InitialCreate \
  --project src/LeaveRequest.Infrastructure \
  --startup-project src/LeaveRequest.API

# Apply migration ไปยัง SQLite DB
dotnet ef database update \
  --project src/LeaveRequest.Infrastructure \
  --startup-project src/LeaveRequest.API
```

**EF Core Configuration ที่ต้องเพิ่มใน AppDbContext:**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply all IEntityTypeConfiguration<T> from Infrastructure assembly
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

    // Global Query Filters (Soft Delete) — Mutable entities เท่านั้น
    modelBuilder.Entity<LeaveType>().HasQueryFilter(e => !e.IsDeleted);
    modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
    modelBuilder.Entity<LeaveRequest>().HasQueryFilter(e => !e.IsDeleted);
    modelBuilder.Entity<LeaveBalance>().HasQueryFilter(e => !e.IsDeleted);
    modelBuilder.Entity<CancelRequest>().HasQueryFilter(e => !e.IsDeleted);
    modelBuilder.Entity<Attachment>().HasQueryFilter(e => !e.IsDeleted);
    // ApprovalHistory, NotificationLog, ImportLog: ไม่มี IsDeleted
}

// เปิด FK enforcement (เพิ่มใน OnConfiguring หรือ SaveChanges override)
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder.UseSqlite(connectionString)
                  .AddInterceptors(new SqliteForeignKeyInterceptor());
}
// หรือใช้ approach อื่น: Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;")
```

---

## 8. Open Issues / Deviation

| # | Issue | Class Diagram Ref | SRS Ref | Status |
|---|-------|------------------|---------|--------|
| OI-001 | **Guid Storage** — EF Core 7+ เก็บ Guid เป็น BLOB อัตโนมัติ แต่ SQL Draft นี้ใช้ TEXT; ต้องยืนยัน EF Core version และ test migration | OI-001 | — | Open |
| OI-002 | **decimal precision** — ใช้ `REAL` ตาม data type rules แต่ REAL ไม่ precise สำหรับ `0.5` วัน; พิจารณาเปลี่ยน DurationDays, EntitledDays, UsedDays, PendingDays, CarriedForwardDays เป็น TEXT | OI-002 | NFR-010 | Open |
| OI-003 | **DateOnly support** — EF Core 7.0+ เท่านั้น (project ใช้ .NET 10 → EF Core 9+, น่าจะรองรับ) | OI-003 | — | Low Risk |
| OI-004 | **ApprovalHistories XOR FK** — `LeaveRequestId` XOR `CancelRequestId` ต้อง enforce ที่ Application Layer; SQLite ไม่มี CHECK XOR | OI-004 | — | Open |
| OI-005 | **NotificationLogs XOR FK** — เช่นเดียวกับ OI-004 | OI-005 | — | Open |
| OI-006 | **Employee string PK performance** — TEXT PK ช้ากว่า INTEGER; พิจารณาเพิ่ม surrogate INTEGER PK + unique index บน EmployeeId | OI-006 | — | Open |
| OI-007 | **MaxDaysPerYear ของ LeaveType 5,7** — STERILIZATION และ MILITARY ใช้ NULL (ไม่จำกัด); HR ต้องยืนยัน | OI-007 | VR-001 | Open |
| OI-008 | **LeaveYear เป็น ค.ศ. หรือ พ.ศ.** — Draft ใช้ ค.ศ.; HR ต้องยืนยัน | OI-008 | — | Open |
| OI-009 | **Carry-forward formula** — Draft เตรียม `CarriedForwardDays` column แต่ยังไม่มี calculation logic | OI-009 | VR-008 | Open |
| OI-010 | **SLA Working Hours** — `SlaDeadline` คำนวณจาก "1 วันทำการ" แต่ยังไม่ชัดว่ากี่ชั่วโมง; `SlaSchedulerService` ต้องการ working calendar | OI-010 | NFR-011 | Open |
| **DEV-001** | **Assumption: Department เป็น string field** — class diagram ไม่มี Department entity แยก; เก็บเป็น TEXT ใน Employees.Department | — | — | Assumption |
| **DEV-002** | **SQL Draft ใช้ REAL สำหรับ decimal** — ตาม data type rules ของ prompt แต่ขัดกับ recommendation ใน class diagram ที่ให้ใช้ TEXT เพื่อ precision | OI-002 | — | Noted |

---

## 9. SQLite Data Type Reference

| C# Type | SQLite Type | EF Core Config | ตัวอย่าง |
|---------|-------------|----------------|---------|
| `int` / `byte` | `INTEGER` | (default) | `LeaveTypeId`, `RetryCount` |
| `long` | `INTEGER` | (default) | `FileSizeBytes` |
| `string` | `TEXT` | `.HasMaxLength(n)` | `EmployeeId`, `Email` |
| `bool` | `INTEGER` (0/1) | (default) | `IsDeleted`, `IsActive` |
| `DateTime` | `TEXT` | UTC ISO 8601 | `CreatedAt`, `SlaDeadline` |
| `DateOnly` | `TEXT` | `yyyy-MM-dd` | `StartDate`, `HireDate` |
| `decimal` | `REAL` | `.HasColumnType("REAL")` | `DurationDays`, `EntitledDays` |
| `Guid` | `TEXT` | auto / `.HasConversion<string>()` | `LeaveRequestId` |
| `enum` | `TEXT` | `.HasConversion<string>()` | `Status`, `EmployeeType` |

---

*SQL Draft นี้ generate จาก class diagram v1.0 และ data architecture design v1.0 — schema จริงต้อง generate ผ่าน EF Core Migration ซึ่งอาจต่างจาก Draft นี้เล็กน้อย*
