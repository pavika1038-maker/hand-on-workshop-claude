---
title: "AI SDLC Standard — มาตรฐาน Software Development Life Cycle สำหรับองค์กร"
version: "1.0"
last_updated: "2026-06-16"
owner: "Architecture Team"
---

# AI SDLC Standard

เอกสารนี้คือมาตรฐาน SDLC ขององค์กรที่ใช้ inject เข้า AI context เมื่อต้องการ generate System Design และ Functional Design artifact โดยรวบรวมหลักการจาก knowledge base ทั้ง 5 ด้าน

---

## 1. Application Architecture Standard

### 1.1 Architecture Pattern ที่แนะนำ

| Pattern | เมื่อใดควรใช้ |
|---------|-------------|
| **Layered Architecture** | ระบบ CRUD ทั่วไป, ERP ภายในองค์กร, ทีมเล็ก (1–2 ทีม) — เริ่มที่นี่ก่อนเสมอ |
| **Hexagonal (Clean Architecture)** | ต้องการ testability สูง หรือมีแผนเปลี่ยน database/UI ในอนาคต |
| **Microservices** | ระบบขนาดใหญ่ หลายทีม ต้อง scale แยก component |
| **Event-Driven** | ต้องการ real-time processing, loose coupling, async workflow |
| **Plugin-Based** | ต้องเพิ่ม module/feature บ่อย หรือมี third-party extension |

**กฎหลัก:**
- แต่ละ layer สื่อสารผ่าน interface เท่านั้น ห้ามข้าม layer
- ใช้ Dependency Injection (DI) ทุก layer
- Business Logic ต้องอยู่ใน Domain/Application Layer เท่านั้น
- แยก project ใน Solution ตาม layer: `MyApp.Web`, `MyApp.Application`, `MyApp.Domain`, `MyApp.Infrastructure`

### 1.2 Frontend Framework Standard

| Framework | สถานะ | เมื่อใดใช้ |
|-----------|-------|-----------|
| **Angular** | แนะนำ | ระบบ enterprise ที่ต้องการ structure ชัดเจน, TypeScript built-in |
| **Vue.js** | ทางเลือกเสริม | Learning curve ต่ำ, flexible |
| **Blazor** | ทางเลือกสำหรับทีม .NET | Intranet, network latency ต่ำ, ทีมถนัด C# |

**กฎหลัก:**
- ใช้ TypeScript strict mode เสมอ (Angular / Vue.js)
- UI component library: Angular Material / PrimeNG (Angular), Vuetify / PrimeVue (Vue.js), Fluent UI Blazor / MudBlazor (Blazor)
- Authentication: MSAL Angular / MSAL Browser สำหรับ Microsoft Entra ID
- ไม่มี business logic ใน frontend ทุกกรณี

### 1.3 UX/UI Design System

ยึด **Microsoft Fluent Design System** เป็นมาตรฐาน

| Breakpoint | ขนาดหน้าจอ | Layout |
|------------|-----------|--------|
| Small | < 640px | Single column |
| Medium | 640–1023px | Two column |
| Large | 1024–1365px | Full navigation |
| X-Large | ≥ 1366px | Full layout |

- Accessibility: WCAG 2.1 Level AA ขั้นต่ำ, Color Contrast ≥ 4.5:1
- ใช้ Design Token จาก Fluent UI ห้าม hardcode ค่า color/spacing

### 1.4 API Design Standard

| เกณฑ์ | มาตรฐาน |
|-------|---------|
| Specification | OpenAPI 3.0 (Swagger) |
| Versioning | URL path: `/api/v1/`, `/api/v2/` |
| Naming | kebab-case: `/api/v1/leave-requests` |
| Response Format | JSON camelCase |
| Error Format | Problem Details (RFC 7807) |
| Pagination | `?page=1&pageSize=20` + `X-Total-Count` header |

**Response Standard:**
```json
{
  "success": true,
  "data": {},
  "error": null,
  "metadata": { "page": 1, "pageSize": 20, "totalCount": 150 }
}
```

---

## 2. Data Architecture Standard

### 2.1 Database Technology Stack

| ประเภทข้อมูล | เทคโนโลยี |
|-------------|-----------|
| Transactional (OLTP) | **SQL Server 2022 / Azure SQL Database** |
| Analytical (OLAP) | Azure Synapse Analytics |
| Cache | Azure Cache for Redis |
| File / Blob | Azure Blob Storage |

### 2.2 SQL Server Naming Convention

| ประเภท | รูปแบบ | ตัวอย่าง |
|--------|--------|---------|
| Table | PascalCase, พหูพจน์ | `LeaveRequests`, `Employees` |
| Column | PascalCase | `EmployeeId`, `StartDate` |
| Primary Key | `{TableName}Id` | `LeaveRequestId` |
| Foreign Key | `FK_{Child}_{Parent}` | `FK_LeaveRequests_Employees` |
| Index | `IX_{Table}_{Columns}` | `IX_LeaveRequests_Status` |
| View | `vw_{Description}` | `vw_LeaveRequestSummary` |

### 2.3 Data Type Standard

| ประเภท | SQL Type |
|--------|----------|
| ID (รหัส) | `UNIQUEIDENTIFIER` (distributed) หรือ `BIGINT` (single DB) |
| ชื่อ (ไทย) | `NVARCHAR(200)` |
| วันที่ | `DATE` |
| วันที่เวลา | `DATETIME2(0)` (UTC) |
| จำนวนเงิน/วัน | `DECIMAL(10,2)` ห้ามใช้ `FLOAT` |
| สถานะ | `TINYINT` + lookup table |

### 2.4 Audit Columns มาตรฐาน (ทุกตารางต้องมี)

```sql
CreatedAt   DATETIME2(0) NOT NULL DEFAULT GETUTCDATE(),
CreatedBy   NVARCHAR(100) NOT NULL,
UpdatedAt   DATETIME2(0) NULL,
UpdatedBy   NVARCHAR(100) NULL,
IsDeleted   BIT NOT NULL DEFAULT 0,
DeletedAt   DATETIME2(0) NULL,
DeletedBy   NVARCHAR(100) NULL
```

**กฎหลัก:**
- Soft delete เสมอ (`IsDeleted = 1`) ห้ามลบจริง
- เก็บเวลาเป็น UTC ทั้งหมด แปลง local time ที่ Presentation Layer
- ทุก query ต้องมี `WHERE IsDeleted = 0`

### 2.5 Data Access Pattern

| กรณี | เครื่องมือ |
|------|-----------|
| CRUD มาตรฐาน | Entity Framework Core (EF Core) |
| Query ที่ต้องการ performance สูง | Dapper |
| Report / read-heavy query | SQL View + Dapper |

---

## 3. Integration Architecture Standard

### 3.1 Integration Pattern ที่รองรับ

| Pattern | เมื่อใดใช้ | เทคโนโลยี |
|---------|----------|-----------|
| **Synchronous (REST)** | ต้องการผลลัพธ์ทันที | Azure API Management + REST API (HTTPS/TLS 1.2+) |
| **Asynchronous (Event)** | ไม่ต้องการผลลัพธ์ทันที, notify, audit | Azure Service Bus (Topic/Subscription) |
| **Batch (File)** | ส่งข้อมูลจำนวนมากตามรอบ | SFTP + checksum |

### 3.2 Synchronous Integration Standard

- Timeout: Connection 5 วินาที, Read 10 วินาที
- Retry: 2 ครั้ง (exponential backoff เริ่ม 2 วินาที)
- Circuit Breaker: ใช้ **Polly** (.NET) — 5 failures → open 30 วินาที
- ทุก external API call ต้องผ่าน **Azure API Management** เป็น gateway

### 3.3 Asynchronous Integration Standard

- Message Broker: **Azure Service Bus** (Topic + Subscription)
- Format: JSON (UTF-8) + CloudEvents specification
- Consumer ต้อง idempotent (process ซ้ำได้ไม่เกิด side effect)
- เปิด Dead Letter Queue เสมอ
- ใช้ `CorrelationId` ทุก event สำหรับ distributed tracing

### 3.4 Error Classification

| ประเภท | การจัดการ |
|--------|----------|
| Transient (network, timeout) | Retry ด้วย exponential backoff |
| Business Error (validation) | ส่งกลับ error response ทันที ไม่ retry |
| System Error (5xx) | Alert + Dead Letter + manual intervention |

---

## 4. Infrastructure & Cloud Architecture Standard

### 4.1 Deployment Model

| Model | เมื่อใดใช้ |
|-------|-----------|
| **On-Premise** | ข้อจำกัด compliance/data sovereignty, ต้อง low latency < 10ms, เชื่อมกับเครื่องจักรโรงงาน |
| **Azure Cloud (HQ owned)** | ต้องการ scalability, global access, ลด infra management |
| **Hybrid** | ย้ายระบบเป็นขั้นตอน |

### 4.2 Operating System & Middleware Standard

| รายการ | มาตรฐาน |
|--------|---------|
| OS | **Windows Server (Latest Version)** |
| Database | **SQL Server (Latest Version)** |
| Web Server | IIS |
| Runtime | **.NET Core / ASP.NET (Latest LTS)** |
| Backup | Azure Backup (HQ จัดเตรียม) |
| Antivirus | HQ provided Antivirus Tool |

### 4.3 Environment Strategy

| สภาพแวดล้อม | วัตถุประสงค์ |
|-------------|------------|
| DEV | พัฒนา, unit test |
| SIT | System Integration Test |
| UAT | User Acceptance Test |
| PROD | Production |

### 4.4 CI/CD Standard

- Platform: **Azure DevOps**
- Branch strategy: main (PROD), develop, feature/*, hotfix/*
- ทุก merge ต้องผ่าน Pull Request + code review
- Automated test ต้องผ่านก่อน deploy ทุกสภาพแวดล้อม

---

## 5. Security Architecture Standard

### 5.1 Security Principles (Defense in Depth)

| หลักการ | ความหมาย |
|---------|---------|
| **Least Privilege** | ให้สิทธิ์เฉพาะเท่าที่จำเป็น |
| **Separation of Duties** | แยกบทบาท user, admin, auditor |
| **Defense in Depth** | วาง control หลายชั้น |
| **Secure by Default** | ค่าเริ่มต้นต้องปลอดภัย ไม่เปิดสิทธิ์เกิน |
| **Assume Breach** | ออกแบบโดยถือว่ามีโอกาส compromise |

### 5.2 Identity & Access Standard

| รายการ | มาตรฐาน |
|--------|---------|
| Identity Provider | **Microsoft Entra ID** |
| MFA | บังคับสำหรับทุก user (**ไม่ใช้ SMS OTP**) |
| Access Model | **RBAC (Role-Based Access Control)** |
| Token | JWT (Access Token + Refresh Token) |
| Secret Management | **Azure Key Vault** ห้าม hardcode secret |

### 5.3 Network Security Standard

- แบ่ง network segment: Public Zone, Application Zone, Data Zone
- ใช้ **Azure Firewall / Network Security Group** ควบคุม traffic
- ทุก communication ใช้ **TLS 1.2 ขึ้นไป** เท่านั้น
- Database ต้องไม่ expose สู่ internet โดยตรง

### 5.4 Application Security Standard

ยึดตาม **OWASP Top 10** เป็นขั้นต่ำ:
- Validate input ทุก field (server-side เสมอ)
- ใช้ parameterized query ห้าม string concatenation ใน SQL
- ไม่ expose error detail / stack trace ให้ client
- Authorization enforce ที่ backend เท่านั้น

### 5.5 Data Security Standard

| ประเภท | การป้องกัน |
|--------|-----------|
| Data at rest | Azure SQL Transparent Data Encryption (TDE) |
| Data in transit | TLS 1.2+ |
| Sensitive data | classify ตาม ISO 27001: Public, Internal, Confidential, Restricted |
| PDPA | ข้อมูลส่วนบุคคลต้องได้รับความยินยอมก่อนเก็บ |

### 5.6 Audit & Logging Standard

- Log ทุก authentication attempt (success + failure)
- Log ทุก data access ที่ sensitive
- Log ทุก permission change
- Centralized logging: **Serilog → Azure Application Insights**
- เก็บ audit log ตามกำหนด ISO 27001 (ขั้นต่ำ 1 ปี)

---

## 6. Cross-Cutting Standard

### 6.1 Error Handling

| ชั้น | การจัดการ |
|-----|----------|
| Frontend | inline validation error, toast สำหรับ API error, error boundary page |
| Backend | Global Exception Handler middleware, Problem Details (RFC 7807) |
| Integration | Retry + Circuit Breaker (Polly) + Dead Letter Queue |

### 6.2 Logging Standard

- ใช้ **Serilog** เป็น logging framework หลัก
- ส่ง log ไปยัง **Azure Application Insights**
- ทุก log entry ต้องมี: timestamp (UTC), severity, correlation ID, service name
- ห้าม log ข้อมูลส่วนบุคคลหรือ sensitive data

### 6.3 Monitoring Standard

- ใช้ **Azure Monitor** + Application Insights
- กำหนด alert สำหรับ: error rate > 1%, response time > 3s, availability < 99.9%
- Health Check endpoint ทุก service

---

## 7. Functional Design Standard

### 7.1 หลักการพื้นฐาน

| หลักการ | รายละเอียด |
|---------|-----------|
| **1 Function = 1 Document** | ทุก function ต้องมีเอกสารแยกต่างหาก ห้ามรวมหลาย function ในไฟล์เดียว |
| **Function ID** | ใช้รูปแบบ `[TYPE]-[NNN]` เช่น `SCR-001`, `INT-IB-001` |
| **Source Traceability** | ทุก function ต้องอ้างอิง Requirement ID จาก SRS เสมอ |
| **Ownership** | ทุกเอกสารต้องระบุ Author และ Review Date |

### 7.2 ประเภท Function และ Prefix

| Prefix | ประเภท | คำอธิบาย | ตัวอย่าง Output |
|--------|--------|---------|----------------|
| **COM** | Common Functions | ฟังก์ชันที่ใช้ร่วมกัน เช่น Login, Notification | COM-001-login.md |
| **SCR** | Screen Functions | หน้าจอ UI ทุกหน้า | SCR-001-create-leave.md |
| **RPT** | Report Functions | รายงาน / Export | RPT-001-leave-summary.md |
| **INT** | Interface Functions | การเชื่อมต่อกับระบบภายนอก | INT-001-hris-sync.md |
| **BAT** | Batch Functions | งาน background / scheduler | BAT-001-leave-accrual.md |

### 7.3 Input → Output Flow

```
Input:
  - SRS (10-requirement-definition/b0-system-requriement/)
  - System Design (20-system-design/a0-architecture-design/)
  - Knowledge Base (80-knowledge-base/functional-design/)

Process:
  1. อ่าน SRS → ระบุ Functional Requirements
  2. อ่าน System Design → เข้าใจ component boundaries และ data model
  3. จัดกลุ่ม requirement → กำหนด Function ID ตาม COM/SCR/RPT/INT/BAT
  4. สร้างเอกสาร 1 ไฟล์ต่อ 1 function ตาม output-template ใน knowledge base

Output:
  - Functional Design documents (30-functional-design/ หรือ 20-system-design/b0-functional-design/)
```

### 7.4 Screen Function Standard (SCR)

สำหรับทุก Screen Function ต้องมีส่วนประกอบครบดังนี้:

| Section | รายการ | หมายเหตุ |
|---------|--------|---------|
| 1. Overview | Function ID, Name, Category, Actor, Related Req IDs | บังคับ |
| 2. Business Purpose | อธิบายว่า screen นี้แก้ปัญหาอะไร | บังคับ |
| 3. Screen Overview | Menu Path, Navigation In/Out, Pre/Postconditions | บังคับ |
| 4. Mockup / UI Layout | ASCII หรือ Wireframe reference | แนะนำ |
| 5. Fields Definition | ทุก field พร้อม validation rule | บังคับ |
| 6. Commands / Actions | ปุ่มและ trigger condition | บังคับ |
| 7. Screen Behavior | Event → Condition → Behavior table | บังคับ |
| 8. Business Rules | Rule ID ที่ reference จาก SRS | บังคับ |
| 9. Message List | Error + Success messages | บังคับ |
| 10. Exception Handling | System error scenarios | บังคับ |
| 11. Notes / Assumptions | ข้อสมมติที่ BA/SA กำหนด | บังคับ |
| Change Log | Version history | บังคับ |

**Screen Types ที่รองรับ:**
- Create Form, Edit Form, View/Detail, List/Search, Popup, Dashboard, Wizard

### 7.5 Interface Function Standard (INT)

| Interface Type | Trigger | ตัวอย่าง |
|---------------|---------|---------|
| Inbound API (REST) | External system calls our endpoint | HRIS → Leave System |
| Outbound API (REST) | We call external system | Leave System → Teams notification |
| File Inbound (SFTP) | Scheduler download file จาก external | HRIS file → Staging |
| File Outbound (SFTP) | Scheduler upload file ไป external | Leave data → Payroll |
| Message Queue | Event-driven via Azure Service Bus | Async notification |

ทุก Interface Function ต้องระบุ: Direction, Trigger, Error Handling (retry + alert), และ File/API Specification

### 7.6 Batch Function Standard (BAT)

ทุก Batch Function ต้องระบุ:
- **Schedule**: cron expression + timezone (ICT = UTC+7)
- **Idempotency**: ถ้า run ซ้ำจะเกิดอะไร → ต้องไม่เกิด double processing
- **Restart/Resume**: กรณี fail ต้อง retry หรือ resume ได้
- **Monitoring**: log ผล + alert ถ้า fail

### 7.7 Template และ Sample Reference

| ประเภท | Template | Sample |
|--------|---------|--------|
| Common | 80-knowledge-base/functional-design/01-common-functions/output-template.md | output-sample-001.md |
| Screen | 80-knowledge-base/functional-design/02-screen-functions/output-template.md | output-sample-001.md |
| Report | 80-knowledge-base/functional-design/03-report-functions/output-template.md | output-sample-001.md |
| Interface (API) | 80-knowledge-base/functional-design/04-interface-functions/output-template-api-ep.md | output-sample-api-ep-001.md |
| Interface (File In) | 80-knowledge-base/functional-design/04-interface-functions/output-template-file-ib.md | output-sample-file-ib-001.md |
| Interface (File Out) | 80-knowledge-base/functional-design/04-interface-functions/output-template-file-ob.md | output-sample-file-ob-001.md |
| Batch | 80-knowledge-base/functional-design/05-batch-functions/output-template.md | output-sample-001.md |

### 7.8 Function Registry

ทุก function ที่สร้างต้องลงทะเบียนใน `80-knowledge-base/functional-design/function-index.md`:

```markdown
| Function ID | Function Name | Type | Status | Document Path |
|------------|--------------|------|--------|---------------|
| SCR-001 | Create Leave Request | Screen | Draft | 30-functional-design/scr/... |
```

---

## 8. ข้อกำหนดสำหรับ AI ที่ใช้เอกสารนี้

เมื่อ AI สร้าง System Design หรือ Functional Design ให้ยึดตามเอกสารนี้เท่านั้น:

1. **ห้ามเสนอ technology นอกมาตรฐาน** เช่น ห้ามเสนอ MySQL, PostgreSQL, React, Next.js แทน tech stack ขององค์กร
2. **ระบุเหตุผลของทุก decision** โดย trace กลับมายัง section ในเอกสารนี้
3. **ระบุ assumption ทุกข้อ** หาก input ไม่ครบ
4. **ห้ามเลือก tech stack ที่ขัดกับ Infrastructure Architecture** (เช่น ห้ามเสนอ Linux/Apache ถ้า org ใช้ Windows/IIS)
5. **Output เป็น Markdown** พร้อม Mermaid diagram เมื่อเหมาะสม

---

*อ้างอิง: knowledge base จาก 80-knowledge-base/architecture-design/ ทั้ง 5 ด้าน และ 80-knowledge-base/functional-design/ ทุก function type*
