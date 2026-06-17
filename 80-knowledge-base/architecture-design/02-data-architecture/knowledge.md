# Data Architecture

## 1. ภาพรวม

Data Architecture คือการออกแบบโครงสร้างข้อมูลทั้งหมดขององค์กร ตั้งแต่การจัดเก็บ การเข้าถึง การรักษาคุณภาพ ไปจนถึงการจัดการวงจรชีวิตของข้อมูล โดยยึด Microsoft SQL Server และ Azure Data Services เป็นแกนหลัก

## 2. Database Architecture

### 2.1 Database Technology Stack

| ประเภทข้อมูล | เทคโนโลยีหลัก | เมื่อใดควรใช้ |
|-------------|--------------|-------------|
| Transactional (OLTP) | SQL Server 2022 / Azure SQL Database | ระบบธุรกรรมทั่วไป เช่น Leave Request, ERP |
| Analytical (OLAP) | Azure Synapse Analytics | Data warehouse, BI reporting |
| Document / JSON | Azure Cosmos DB | ข้อมูล semi-structured, high throughput |
| Cache | Azure Cache for Redis | Session cache, frequently accessed data |
| File / Blob | Azure Blob Storage | ไฟล์แนบ, เอกสาร, รูปภาพ |
| Search | Azure Cognitive Search | Full-text search, faceted search |

### 2.2 SQL Server Database Design Best Practice

#### 2.2.1 Naming Convention

| ประเภท | รูปแบบ | ตัวอย่าง |
|--------|--------|---------|
| Table | PascalCase, พหูพจน์ | `LeaveRequests`, `Employees`, `ApprovalHistories` |
| Column | PascalCase | `EmployeeId`, `StartDate`, `CreatedAt` |
| Primary Key | `{TableName}Id` | `LeaveRequestId`, `EmployeeId` |
| Foreign Key | `FK_{ChildTable}_{ParentTable}` | `FK_LeaveRequests_Employees` |
| Index | `IX_{TableName}_{Columns}` | `IX_LeaveRequests_Status_CreatedAt` |
| Unique Constraint | `UQ_{TableName}_{Columns}` | `UQ_Employees_EmployeeCode` |
| Check Constraint | `CK_{TableName}_{Column}` | `CK_LeaveRequests_Status` |
| Stored Procedure | `usp_{Action}{Entity}` | `usp_GetLeaveBalance`, `usp_SubmitLeaveRequest` |
| View | `vw_{Description}` | `vw_LeaveRequestSummary` |

#### 2.2.2 Data Type Standard

| ประเภทข้อมูล | SQL Server Data Type | หมายเหตุ |
|-------------|---------------------|---------|
| รหัสอ้างอิง (ID) | `UNIQUEIDENTIFIER` (GUID) หรือ `BIGINT` (auto-increment) | GUID สำหรับ distributed system, BIGINT สำหรับ single database |
| รหัสธุรกิจ (Code) | `NVARCHAR(20)` | เช่น EmployeeCode, DepartmentCode |
| ชื่อ (ภาษาไทย) | `NVARCHAR(200)` | ใช้ NVARCHAR เสมอสำหรับข้อมูลภาษาไทย |
| ชื่อ (ภาษาอังกฤษ) | `VARCHAR(200)` | ใช้ VARCHAR หากมั่นใจว่าเป็น ASCII เท่านั้น |
| วันที่ | `DATE` | เฉพาะวันที่ ไม่มีเวลา |
| วันที่เวลา | `DATETIME2(0)` | มีเวลาด้วย precision 0 (วินาที) |
| จำนวนวัน/เงิน | `DECIMAL(10,2)` | ห้ามใช้ `FLOAT` สำหรับข้อมูลที่ต้องการความแม่นยำ |
| สถานะ (Enum) | `TINYINT` + lookup table | ไม่ใช้ `VARCHAR` เก็บสถานะ |
| Flag (Y/N) | `BIT` | 0 = No, 1 = Yes |
| ข้อความยาว | `NVARCHAR(MAX)` | สำหรับ comment, reason |

#### 2.2.3 Audit Columns มาตรฐาน

ทุกตารางต้องมี audit columns ต่อไปนี้:

```sql
CreatedAt       DATETIME2(0)    NOT NULL DEFAULT GETUTCDATE(),
CreatedBy       NVARCHAR(100)   NOT NULL,
UpdatedAt       DATETIME2(0)    NULL,
UpdatedBy       NVARCHAR(100)   NULL,
IsDeleted       BIT             NOT NULL DEFAULT 0,
DeletedAt       DATETIME2(0)    NULL,
DeletedBy       NVARCHAR(100)   NULL
```

**Best Practice:**
- ใช้ soft delete (IsDeleted = 1) แทนการลบจริง
- เก็บเวลาเป็น UTC เสมอ แปลงเป็น local time ที่ Presentation Layer
- ทุก query ต้องมี `WHERE IsDeleted = 0` (ใช้ global query filter ใน EF Core)

### 2.3 Database Schema Design Pattern

#### 2.3.1 ตัวอย่าง Entity Relationship

```text
┌─────────────┐    ┌─────────────────┐    ┌──────────────────┐
│  Employees  │    │  LeaveRequests  │    │  ApprovalSteps   │
├─────────────┤    ├─────────────────┤    ├──────────────────┤
│ EmployeeId  │◄──┤ EmployeeId (FK) │    │ ApprovalStepId   │
│ EmployeeCode│    │ LeaveRequestId  │◄──┤ LeaveRequestId   │
│ FullNameTh  │    │ LeaveTypeId(FK) │    │ StepOrder        │
│ FullNameEn  │    │ StartDate       │    │ ActorId (FK)     │
│ DeptCode    │    │ EndDate         │    │ Action           │
│ ManagerId   │    │ Duration        │    │ Comment          │
│ RoleId      │    │ Reason          │    │ ActionAt         │
│ EmpType     │    │ Status          │    └──────────────────┘
│ AccountStat │    │ ContactDuring   │
└─────────────┘    │ DelegateTo      │    ┌──────────────────┐
                   └─────────────────┘    │  LeaveBalances   │
┌─────────────┐                           ├──────────────────┤
│  LeaveTypes │    ┌─────────────────┐    │ LeaveBalanceId   │
├─────────────┤    │  Attachments    │    │ EmployeeId (FK)  │
│ LeaveTypeId │    ├─────────────────┤    │ LeaveTypeId (FK) │
│ TypeCode    │    │ AttachmentId    │    │ LeaveYear        │
│ TypeNameTh  │    │ LeaveRequestId  │    │ EntitledDays     │
│ TypeNameEn  │    │ FileName        │    │ UsedDays         │
│ ApprovalFlow│    │ FilePath        │    │ PendingDays      │
│ MaxDaysPerYr│    │ FileSize        │    │ AvailableDays    │
└─────────────┘    │ MimeType        │    └──────────────────┘
                   └─────────────────┘
```

#### 2.3.2 Indexing Strategy

| ประเภท Index | เมื่อใดควรสร้าง | ตัวอย่าง |
|-------------|---------------|---------|
| Clustered Index | Primary Key (สร้างอัตโนมัติ) | `PK_LeaveRequests` on `LeaveRequestId` |
| Non-clustered Index | Column ที่ใช้ใน WHERE, JOIN, ORDER BY บ่อย | `IX_LeaveRequests_EmployeeId_Status` |
| Covering Index | Query ที่ต้องการ performance สูง | `IX_LeaveRequests_Status INCLUDE (StartDate, EndDate)` |
| Filtered Index | ข้อมูลที่ query เฉพาะบางเงื่อนไข | `IX_LeaveRequests_Pending WHERE Status = 1 AND IsDeleted = 0` |

**Best Practice:**
- ไม่สร้าง index เกิน 5-7 ตัวต่อตาราง (กระทบ write performance)
- ตรวจสอบ missing index ด้วย SQL Server DMV `sys.dm_db_missing_index_details`
- ใช้ Azure SQL Database Automatic Tuning สำหรับ index recommendation

## 3. Data Access Pattern

### 3.1 Entity Framework Core (ORM — แนะนำ)

**เมื่อใดควรใช้:** ระบบทั่วไปที่ไม่ต้อง optimize query ระดับลึก

```text
Application Layer → Repository Interface → EF Core DbContext → SQL Server
```

**Best Practice EF Core:**
- ใช้ Code-First approach พร้อม Migrations
- ใช้ `AsNoTracking()` สำหรับ read-only query
- ใช้ `IQueryable` ใน Repository ส่งกลับ เพื่อให้ Application Layer กำหนด filter/paging
- ห้ามใช้ Lazy Loading (ใช้ Eager Loading ด้วย `.Include()` หรือ Explicit Loading)
- ใช้ Global Query Filter สำหรับ soft delete: `modelBuilder.Entity<X>().HasQueryFilter(x => !x.IsDeleted)`

### 3.2 Dapper (Micro ORM — สำหรับ performance-critical)

**เมื่อใดควรใช้:** Report query ที่ซับซ้อน, Bulk operations, Query ที่ต้อง optimize ละเอียด

**Best Practice Dapper:**
- ใช้ parameterized query เสมอ (ป้องกัน SQL Injection)
- ใช้ `QueryMultiple` สำหรับ query หลาย result set ในรอบเดียว
- จัดเก็บ SQL query เป็น resource file หรือ constant class ไม่ฝังใน code

## 4. Data Migration & Versioning

### 4.1 Schema Migration

| เครื่องมือ | เมื่อใดควรใช้ |
|-----------|-------------|
| EF Core Migrations | เมื่อใช้ EF Core เป็น ORM หลัก |
| DbUp | เมื่อต้องการ SQL script-based migration |
| SQL Server Data Tools (SSDT) | เมื่อต้องการ state-based approach (dacpac) |

**Best Practice:**
- ทุก migration ต้องมี Up และ Down script
- ห้าม modify migration ที่ apply ไปแล้วใน production
- ใช้ idempotent migration (สามารถ run ซ้ำได้โดยไม่ error)
- Migration script ต้องอยู่ใน source control (Azure DevOps Git)

### 4.2 Data Seeding

- Master data (Leave Type, Status, Role) ต้อง seed ผ่าน migration script
- ใช้ `HasData()` ใน EF Core สำหรับ static lookup data
- Test data ต้องแยก seed script ไม่รวมกับ production seed

## 5. Data Quality & Governance

### 5.1 Data Quality Rules

| เกณฑ์ | คำอธิบาย | วิธีบังคับ |
|-------|----------|----------|
| Completeness | ข้อมูลต้องครบถ้วน | NOT NULL constraint + application validation |
| Uniqueness | ข้อมูลต้องไม่ซ้ำ | UNIQUE constraint + application check |
| Validity | ข้อมูลต้องอยู่ในขอบเขตที่กำหนด | CHECK constraint + ENUM validation |
| Consistency | ข้อมูลต้องสอดคล้องกัน | Foreign Key + business rule validation |
| Timeliness | ข้อมูลต้องเป็นปัจจุบัน | Audit columns (UpdatedAt) + sync schedule |

### 5.2 Data Classification (ISO 27001 Annex A 5.12)

| ระดับ | คำอธิบาย | ตัวอย่าง | การจัดการ |
|-------|----------|---------|----------|
| Public | ข้อมูลเปิดเผยได้ | ชื่อบริษัท, ที่อยู่สำนักงาน | ไม่จำกัดการเข้าถึง |
| Internal | ข้อมูลภายในองค์กร | รายชื่อพนักงาน, แผนกงาน | จำกัดเฉพาะพนักงาน |
| Confidential | ข้อมูลลับ | ข้อมูลการลา, เงินเดือน | จำกัดเฉพาะผู้มีสิทธิ์ + encrypt at rest |
| Restricted | ข้อมูลลับสูงสุด | ใบรับรองแพทย์, รหัสผ่าน | encrypt at rest + transit + audit log ทุก access |

## 6. Backup & Recovery

### 6.1 Backup Strategy

| ประเภท | ความถี่ | Retention | เครื่องมือ |
|--------|---------|-----------|-----------|
| Full Backup | ทุกวัน 01:00 ICT | 30 วัน | SQL Server Agent / Azure Automated Backup |
| Differential Backup | ทุก 6 ชั่วโมง | 7 วัน | SQL Server Agent |
| Transaction Log Backup | ทุก 15 นาที | 3 วัน | SQL Server Agent |
| Point-in-Time Restore | — | — | Azure SQL built-in (35 วัน) |

### 6.2 Recovery Objectives

| เกณฑ์ | ค่าเป้าหมาย | คำอธิบาย |
|-------|------------|---------|
| RPO (Recovery Point Objective) | 15 นาที | ข้อมูลสูญหายได้ไม่เกิน 15 นาที |
| RTO (Recovery Time Objective) | 4 ชั่วโมง | กู้คืนระบบใช้งานได้ภายใน 4 ชั่วโมง |

### 6.3 Disaster Recovery

- ใช้ SQL Server Always On Availability Groups สำหรับ high availability
- ใช้ Azure SQL Geo-Replication สำหรับ disaster recovery ข้าม region
- ทดสอบ restore จาก backup ทุกไตรมาส (quarterly DR drill)

## 7. Data Retention & Archival

| ประเภทข้อมูล | ระยะเวลาเก็บ (Active) | ระยะเวลาเก็บ (Archive) | วิธีจัดการ |
|-------------|----------------------|----------------------|----------|
| Transaction data (คำขอลา) | 2 ปี ใน production DB | 5 ปี ใน archive storage | ย้ายไป Azure Table Storage หลัง 2 ปี |
| Audit log | 1 ปี ใน production DB | 3 ปี ใน archive storage | ย้ายไป Azure Blob Storage หลัง 1 ปี |
| ไฟล์แนบ | 1 ปี ใน hot storage | 3 ปี ใน cool storage | ย้ายไป Azure Blob Cool Tier หลัง 1 ปี |
| Master data | ตลอดอายุการใช้งาน | — | Soft delete เมื่อไม่ใช้แล้ว |
| Backup files | 30 วัน | — | ลบอัตโนมัติ |

## 8. Checklist สำหรับ Data Architecture Review

- [ ] Naming convention สอดคล้องกับมาตรฐานองค์กร
- [ ] Data type เหมาะสม (NVARCHAR สำหรับภาษาไทย, DECIMAL สำหรับตัวเลขแม่นยำ)
- [ ] ทุกตารางมี audit columns (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
- [ ] Soft delete pattern ถูกนำไปใช้
- [ ] Index strategy ครอบคลุม query หลัก
- [ ] Foreign Key constraint ครบถ้วน
- [ ] Data classification ตาม ISO 27001
- [ ] Backup strategy ครอบคลุม RPO/RTO
- [ ] Data retention policy กำหนดชัดเจน
- [ ] Migration script อยู่ใน source control
- [ ] Sensitive data ถูก encrypt ทั้ง at rest และ in transit
