---
title: "Sequence Diagram — ยื่นคำร้องขอลา (Submit Leave Request)"
document_type: "Functional Design"
version: "1.0"
date: "2026-06-17"
project: "ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)"
company: "ABC Company"
use_case: "UC-Submit-Leave"
status: "Draft"
---

# Sequence Diagram: ยื่นคำร้องขอลา (Submit Leave Request)

## Change Log

| Version | Date | Section | Change Type | Description | Source |
|---------|------|---------|-------------|-------------|--------|
| 1.0 | 2026-06-17 | All | Created | สร้าง Sequence Diagram ครั้งแรก — 3 Scenarios: Happy Path, Balance Insufficient, HRIS API Timeout | Application Architecture v1.0, Method Signature v1.0, SRS Summary v1.0 |

---

## 1. Overview

| รายการ | รายละเอียด |
|--------|-----------|
| **Use Case** | SFR-003: ยื่นคำร้องขอลา (Submit Leave Request) |
| **Actor** | Employee (ประจำ หรือ Outsource) |
| **SRS Trace** | SFR-003, VR-001–007, VR-011, NFR-010, BR-003–007, BR-011 |
| **Scenarios** | Happy Path, Error: วันลาไม่พอ, Error: HRIS API Timeout |
| **Architecture Layer** | Angular SPA → ASP.NET Core API → EF Core (SQLite) → HRIS API (IF-001) |

### Component Legend

| Participant | ชื่อในไดอะแกรม | ชั้น | บทบาท |
|------------|--------------|-----|------|
| `EMP` | Employee | Actor | ผู้ใช้งานระบบ |
| `SPA` | Angular SPA | Frontend | UI, Form validation (client-side), Bearer Token |
| `CTRL` | LeaveRequestController | .NET API — Controller Layer | HTTP Request/Response, JWT check (via Middleware), FluentValidation |
| `SVC` | LeaveRequestService | .NET API — Application Layer | Business Logic, Transaction coordination, Validation orchestration |
| `DB` | EF Core / SQLite | Data Layer | Repository queries, DbContext.SaveChangesAsync() |
| `HRIS` | HRIS REST API | External (IF-001) | Employee Master Data — Pattern B (on-demand) + Polly resilience |
| `MQ` | NotificationService | .NET API / Azure Service Bus | Publish CloudEvent หลัง commit |

### Validation Sequence ก่อน SaveChangesAsync()

```mermaid
flowchart LR
    V1["VR-001\nEmployee Type\nEligibility"]
    V2["VR-003/004\nProbation /\nService Period"]
    V3["VR-005/006\nAdvance Notice\n(พักผ่อน/กิจ)"]
    V4["VR-007\nMedical Cert\n(ลาป่วย ≥3 วัน)"]
    V5["VR-002\nLeave Balance\nCheck"]
    V6["Date\nConflict\nCheck"]
    SAVE["DbContext\n.SaveChangesAsync()"]

    V1 --> V2 --> V3 --> V4 --> V5 --> V6 --> SAVE
```

> **กฎสำคัญ:** `SaveChangesAsync()` เรียกได้ก็ต่อเมื่อผ่าน validation ทั้งหมดแล้วเท่านั้น — ห้าม commit ก่อน validate

---

## 2. Scenario 1: Happy Path — ยื่นลาสำเร็จ

**เงื่อนไข:** พนักงานประจำ (EmployeeType=Regular) ยื่นลาพักผ่อน 3 วัน, สิทธิ์คงเหลือ 8 วัน, ไม่มี date conflict

```mermaid
sequenceDiagram
    autonumber
    participant EMP as Employee
    participant SPA as Angular SPA
    participant CTRL as LeaveRequestController
    participant SVC as LeaveRequestService
    participant DB as EF Core / SQLite
    participant MQ as NotificationService

    EMP->>SPA: กรอกฟอร์มลา<br/>(ลาพักผ่อน, 01-07-2026 ถึง 03-07-2026, เหตุผล)

    Note over SPA: Client-side Validation<br/>required fields, date format, StartDate > today
    SPA->>SPA: ตรวจสอบฟิลด์บังคับและรูปแบบวันที่ → ผ่าน

    SPA->>CTRL: POST /api/v1/leave-requests<br/>Authorization: Bearer JWT<br/>{ leaveTypeId:1, startDate, endDate, reason, attachmentIds:[] }

    Note over CTRL: Middleware Pipeline
    CTRL->>CTRL: [Auth MW] Validate JWT via Microsoft Entra ID
    CTRL->>CTRL: [Auth MW] Extract employeeId="EMP001", role="Employee"
    CTRL->>CTRL: [Log MW] Set correlationId, log request Info
    CTRL->>CTRL: [FluentValidation] DTO structure check → ผ่าน

    CTRL->>SVC: SubmitLeaveRequestAsync("EMP001", dto)

    Note over SVC,DB: ── Validation Phase (ก่อน SaveChangesAsync) ──

    SVC->>DB: IEmployeeRepository.GetByIdAsync("EMP001")
    DB-->>SVC: Employee { IsActive:true, EmployeeType:Regular,<br/>HireDate:2022-01-15, ManagerId:"MGR001" }

    SVC->>DB: ILeaveTypeRepository.GetByIdAsync(leaveTypeId=1)
    DB-->>SVC: LeaveType { TypeCode:"ANNUAL", MaxDaysPerYear:10,<br/>IsAvailableForOutsource:false, RequiresMedicalCert:false }

    SVC->>SVC: [VR-001] Regular employee + Annual Leave → ผ่าน
    SVC->>SVC: [VR-003] HireDate=2022 → อายุงาน > 3 เดือน → ผ่าน
    SVC->>SVC: [VR-004] อายุงาน > 1 ปี → มีสิทธิ์ลาพักผ่อน → ผ่าน
    SVC->>SVC: [VR-005] StartDate(01-07) > today(17-06) + 1 วัน → ผ่าน
    SVC->>SVC: [VR-007] RequiresMedicalCert=false → ไม่ต้องแนบเอกสาร → ผ่าน

    SVC->>DB: ILeaveBalanceRepository.GetAsync("EMP001", leaveTypeId=1, year=2026)
    DB-->>SVC: LeaveBalance { EntitledDays:10, UsedDays:2, PendingDays:0,<br/>CarriedForwardDays:0 } → remainingDays = 8.0

    SVC->>SVC: [VR-002] remainingDays(8.0) >= durationDays(3.0) → ผ่าน

    SVC->>DB: ILeaveRequestRepository.GetOverlappingAsync("EMP001", 01-07, 03-07)
    DB-->>SVC: [] (ไม่มี Pending/Approved request ซ้อนทับ)

    Note over SVC,DB: ── Transaction Block ──
    rect rgb(220, 252, 231)
        SVC->>DB: BEGIN TRANSACTION
        SVC->>DB: ILeaveRequestRepository.AddAsync<br/>{ LeaveRequestId: new Guid, Status: Pending,<br/>DurationDays: 3.0, CreatedAt: UtcNow, CreatedBy: "EMP001" }
        SVC->>DB: ILeaveBalanceRepository.UpdateAsync<br/>{ PendingDays: 0 → 3.0, UpdatedAt: UtcNow }
        SVC->>DB: DbContext.SaveChangesAsync()
        DB-->>SVC: 2 rows affected (OK)
        SVC->>DB: COMMIT TRANSACTION
    end

    Note over SVC,MQ: ── Post-Commit: Fire-and-Forget ──
    SVC--)MQ: PublishLeaveSubmittedAsync(leaveRequestId)
    Note over MQ: Build CloudEvent<br/>type: com.abccompany.leave.request.submitted<br/>→ Publish ไปยัง Azure Service Bus Topic "leave-events"<br/>→ Email จะส่งถึง Manager + HR (async)

    SVC-->>CTRL: LeaveRequestResult { leaveRequestId, ref:"LR-2026-00123", status:Pending }

    CTRL-->>SPA: 201 Created<br/>{ success:true, data:{ leaveRequestId, leaveRequestRef:"LR-2026-00123",<br/>status:"Pending", message:"ยื่นคำขอลาสำเร็จ" } }

    SPA-->>EMP: Toast สีเขียว: "ยื่นคำขอลาสำเร็จ"<br/>Redirect → /leave-requests/LR-2026-00123 (Status Tracking)
```

---

## 3. Scenario 2: Error — วันลาไม่พอ (InsufficientLeaveBalanceException)

**เงื่อนไข:** พนักงานขอลาพักผ่อน 3 วัน แต่สิทธิ์คงเหลือเพียง 1 วัน — VR-002 fail

```mermaid
sequenceDiagram
    autonumber
    participant EMP as Employee
    participant SPA as Angular SPA
    participant CTRL as LeaveRequestController
    participant SVC as LeaveRequestService
    participant DB as EF Core / SQLite

    EMP->>SPA: กรอกฟอร์มลา<br/>(ลาพักผ่อน 3 วัน: 01-07-2026 ถึง 03-07-2026)

    SPA->>SPA: Client-side Validation → ผ่าน (date format OK)

    SPA->>CTRL: POST /api/v1/leave-requests<br/>Authorization: Bearer JWT

    CTRL->>CTRL: [Auth MW] JWT Valid → employeeId="EMP001"
    CTRL->>CTRL: [FluentValidation] DTO → ผ่าน

    CTRL->>SVC: SubmitLeaveRequestAsync("EMP001", dto)

    Note over SVC,DB: ── Validation Phase ──

    SVC->>DB: IEmployeeRepository.GetByIdAsync("EMP001")
    DB-->>SVC: Employee { IsActive:true, EmployeeType:Regular }

    SVC->>DB: ILeaveTypeRepository.GetByIdAsync(leaveTypeId=1)
    DB-->>SVC: LeaveType { TypeCode:"ANNUAL" }

    SVC->>SVC: [VR-001] Regular + Annual → ผ่าน
    SVC->>SVC: [VR-003/004/005] Probation, Service, Notice → ผ่านทุกข้อ
    SVC->>SVC: [VR-007] RequiresMedicalCert=false → ผ่าน

    SVC->>DB: ILeaveBalanceRepository.GetAsync("EMP001", leaveTypeId=1, year=2026)
    DB-->>SVC: LeaveBalance { EntitledDays:10, UsedDays:9, PendingDays:0 }<br/>→ remainingDays = 1.0

    Note over SVC: [VR-002] FAIL<br/>remainingDays(1.0) < durationDays(3.0)
    SVC->>SVC: throw InsufficientLeaveBalanceException<br/>("ลาพักผ่อน", remaining=1.0, requested=3.0)

    Note over SVC: ❌ ไม่มีการเรียก SaveChangesAsync()<br/>ไม่มีการแก้ไขข้อมูลใด ๆ ใน DB

    Note over CTRL: Global Exception Handler Middleware
    CTRL->>CTRL: catch InsufficientLeaveBalanceException<br/>→ HTTP 400 Bad Request + Problem Details
    CTRL->>CTRL: Serilog.Warning("Insufficient balance",<br/>{ employeeId, leaveTypeId, remaining:1.0, requested:3.0, correlationId })

    CTRL-->>SPA: 400 Bad Request<br/>{ success:false, error:{ code:"INSUFFICIENT_BALANCE",<br/>message:"สิทธิ์วันลาไม่เพียงพอ",<br/>details:[{ field:"durationDays",<br/>message:"สิทธิ์ลาพักผ่อนคงเหลือ 1.0 วัน ต้องการ 3.0 วัน" }] } }

    SPA-->>EMP: Inline Error ใต้ช่อง EndDate<br/>"สิทธิ์ลาพักผ่อนคงเหลือ 1.0 วัน ไม่เพียงพอสำหรับการลา 3 วัน"<br/>(ฟอร์มยังแสดงอยู่ — ผู้ใช้แก้ไข EndDate ได้ทันที)
```

---

## 4. Scenario 3: Error — HRIS API Timeout (HrisAdapterException)

**เงื่อนไข:** พนักงานใหม่ที่ยังไม่ได้ sync ข้อมูลเข้า SQLite → ระบบ fallback ไป HRIS REST API (IF-001 Pattern B) → HRIS API ไม่ตอบสนอง → Polly retry 3 ครั้ง → Circuit Breaker OPEN → 503

```mermaid
sequenceDiagram
    autonumber
    participant EMP as Employee
    participant SPA as Angular SPA
    participant CTRL as LeaveRequestController
    participant SVC as LeaveRequestService
    participant DB as EF Core / SQLite
    participant HRIS as HRIS REST API

    EMP->>SPA: กรอกฟอร์มลา (พนักงานใหม่ EmployeeId="EMP999")

    SPA->>SPA: Client-side Validation → ผ่าน

    SPA->>CTRL: POST /api/v1/leave-requests<br/>Authorization: Bearer JWT

    CTRL->>CTRL: [Auth MW] JWT Valid → employeeId="EMP999"
    CTRL->>CTRL: [FluentValidation] DTO → ผ่าน

    CTRL->>SVC: SubmitLeaveRequestAsync("EMP999", dto)

    Note over SVC,DB: ── Validation Phase ──

    SVC->>DB: IEmployeeRepository.GetByIdAsync("EMP999")
    DB-->>SVC: null (พนักงานใหม่ ยังไม่ sync ลง SQLite)

    Note over SVC,HRIS: Employee ไม่พบใน local DB<br/>→ Fallback: on-demand sync จาก HRIS API (IF-001 Pattern B)<br/>Polly Pipeline: Retry(3) + Timeout(10s) + CircuitBreaker(5 fail → 30s open)

    rect rgb(255, 240, 240)
        Note over SVC,HRIS: Polly Attempt 1 / 3
        SVC->>HRIS: GET /api/v1/employees/EMP999<br/>Authorization: API Key (APIM)
        Note over HRIS: ⏱ No response within 10s (Read Timeout)
        HRIS--xSVC: TimeoutRejectedException (10s elapsed)
        Note over SVC: Polly: backoff 2s → retry attempt 2

        Note over SVC,HRIS: Polly Attempt 2 / 3
        SVC->>HRIS: GET /api/v1/employees/EMP999
        HRIS--xSVC: TimeoutRejectedException (10s elapsed)
        Note over SVC: Polly: backoff 4s → retry attempt 3

        Note over SVC,HRIS: Polly Attempt 3 / 3
        SVC->>HRIS: GET /api/v1/employees/EMP999
        HRIS--xSVC: TimeoutRejectedException (10s elapsed)
        Note over SVC: Polly: retry exhausted (3/3)<br/>Circuit Breaker: +1 failure count (toward threshold 5)<br/>→ throw HrisAdapterException
    end

    SVC->>SVC: throw HrisAdapterException<br/>("HRIS API unavailable after 3 retries: EMP999")
    SVC->>SVC: Serilog.Error("HRIS sync failed",<br/>{ employeeId:"EMP999", retries:3, correlationId })

    Note over SVC: ❌ ไม่มีการเรียก SaveChangesAsync()<br/>ไม่มีการแก้ไขข้อมูลใด ๆ ใน DB

    Note over CTRL: Global Exception Handler Middleware
    CTRL->>CTRL: catch HrisAdapterException<br/>→ HTTP 503 Service Unavailable

    CTRL-->>SPA: 503 Service Unavailable<br/>{ success:false, error:{ code:"HRIS_UNAVAILABLE",<br/>message:"ไม่สามารถตรวจสอบข้อมูลพนักงานได้ในขณะนี้",<br/>details:[{ message:"กรุณาลองใหม่อีกครั้งในภายหลัง หรือติดต่อ HR" }] } }

    SPA-->>EMP: Toast Error สีแดง<br/>"ระบบไม่สามารถยื่นคำขอลาได้ขณะนี้<br/>กรุณาลองใหม่ หรือติดต่อ HR"<br/>(ปุ่ม Retry แสดงด้วย)
```

---

## 5. Error Handling Summary

| Scenario | Exception | HTTP Status | Error Code | ผลต่อ DB | SRS Trace |
|----------|-----------|------------|-----------|---------|----------|
| Happy Path | — | 201 Created | — | INSERT LeaveRequest + UPDATE LeaveBalance (commit) | SFR-003, NFR-010 |
| DTO validation ไม่ผ่าน | `ValidationException` (FluentValidation) | 400 | `VALIDATION_ERROR` | ไม่มีการเปลี่ยนแปลง | VR-001–013 |
| Employee type ไม่มีสิทธิ์ | `InvalidLeaveTypeForEmployeeException` | 400 | `INVALID_LEAVE_TYPE` | ไม่มีการเปลี่ยนแปลง | VR-001 |
| อยู่ในช่วงทดลองงาน | `ProbationPeriodException` | 400 | `PROBATION_PERIOD` | ไม่มีการเปลี่ยนแปลง | VR-003 |
| วันลาไม่พอ | `InsufficientLeaveBalanceException` | 400 | `INSUFFICIENT_BALANCE` | ไม่มีการเปลี่ยนแปลง | VR-002 |
| วันซ้อนทับ | `DateConflictException` | 400 | `DATE_CONFLICT` | ไม่มีการเปลี่ยนแปลง | SFR-003 |
| ต้องแนบใบรับรองแพทย์ | `MedicalCertificateRequiredException` | 400 | `MEDICAL_CERT_REQUIRED` | ไม่มีการเปลี่ยนแปลง | VR-007 |
| HRIS API Timeout | `HrisAdapterException` | 503 | `HRIS_UNAVAILABLE` | ไม่มีการเปลี่ยนแปลง | IF-001, SIR-001 |
| Database error | `DbUpdateException` | 500 | `INTERNAL_ERROR` | Rollback transaction | NFR-010 |

**กฎสำคัญ: ห้าม SaveChangesAsync() ก่อนผ่าน validation ทุกข้อ**
- ทุก exception ที่เกิดในขั้น validation → ไม่มีข้อมูลใน DB เปลี่ยนแปลง
- Transaction Begin เฉพาะหลังผ่าน validation Phase ทั้งหมดเท่านั้น
- `SaveChangesAsync()` อยู่ใน transaction block — ถ้า fail → Rollback อัตโนมัติ

---

## 6. Polly Resilience Configuration (IF-001)

```csharp
// IHrisAdapter — Polly Pipeline Configuration
services.AddHttpClient<IHrisAdapter, HrisAdapter>()
    .AddResiliencePipeline(options =>
    {
        // Timeout per attempt
        options.AddTimeout(TimeSpan.FromSeconds(10));

        // Retry: 3 attempts, exponential backoff
        options.AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Exponential,   // 2s → 4s → 8s
            ShouldHandle = new PredicateBuilder()
                .Handle<TimeoutRejectedException>()
                .Handle<HttpRequestException>()
        });

        // Circuit Breaker: open after 5 failures, reset after 30s
        options.AddCircuitBreaker(new CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(30)
        });
    });
// อ้างอิง: Integration Architecture v1.0 §9.1 Resilience Pattern per Interface
```

---

## 7. Notification Flow (Post-Commit — Scenario 1)

```mermaid
sequenceDiagram
    autonumber
    participant SVC as LeaveRequestService
    participant NS as NotificationService
    participant PUB as IMessagePublisher
    participant SB as Azure Service Bus<br/>Topic: leave-events
    participant EA as EmailAdapter<br/>(Consumer: email-notify)
    participant LOG as NotificationLogs<br/>(EF Core / SQLite)
    participant EMAIL as Email Gateway

    Note over SVC,NS: หลัง SaveChangesAsync() COMMIT แล้ว (fire-and-forget)
    SVC--)NS: PublishLeaveSubmittedAsync(leaveRequestId)

    NS->>LOG: INSERT NotificationLog { Status:Pending, EventType:"LeaveSubmitted",<br/>Recipients:[Manager, HR], CreatedAt:UtcNow }
    NS->>NS: Build CloudEventDto<br/>{ type:"com.abccompany.leave.request.submitted",<br/>id: Guid.NewGuid(), correlationId, data:{ leaveRequestId, employeeName, ... } }
    NS->>PUB: PublishAsync(cloudEvent)
    PUB->>SB: Publish to Topic "leave-events"<br/>(AMQP over TLS 1.2+)

    Note over SB: At-least-once delivery<br/>Message TTL: 24h, MaxDeliveryCount: 10

    SB->>EA: Deliver to Subscription "email-notify"

    EA->>LOG: ExistsAsync(notificationLogId) → false (Idempotency check)
    EA->>EMAIL: Send Email to Manager + HR<br/>(TLS 1.2+, Subject: "คำขอลาใหม่ — EMP001")
    EMAIL-->>EA: 200 OK (Delivered)
    EA->>LOG: UpdateDeliveryStatusAsync(Status:Success, SentAt:UtcNow)
    EA->>SB: Complete message (ACK)
```

---

## 8. Component Responsibility Summary

| Component | Layer | ความรับผิดชอบใน Submit Leave Flow |
|-----------|-------|----------------------------------|
| **Angular SPA** | Frontend | Form rendering, client-side format validation, Bearer Token attach, error display (inline/toast) |
| **Middleware Pipeline** | .NET API | JWT validation, CorrelationId inject, Serilog request logging |
| **FluentValidation** | .NET API | DTO structure validation (required fields, type correctness) ก่อนเข้า Service |
| **LeaveRequestController** | .NET API | HTTP routing, call Service, map Exception → HTTP Status Code |
| **LeaveRequestService** | .NET API — Application | Business validation (VR-001–007, VR-011), Transaction scope, Post-commit notification |
| **LeaveBalanceService** | .NET API — Application | GetRemainingDays calculation (VR-002) |
| **EF Core / SQLite** | Infrastructure | Repository queries, DbContext.SaveChangesAsync(), Transaction BEGIN/COMMIT/ROLLBACK |
| **HRIS API** | External (IF-001) | Employee Master Data source — fallback เมื่อไม่พบใน local DB |
| **IHrisAdapter** | Infrastructure | Wrap HRIS HTTP call ด้วย Polly (Retry 3x + Timeout 10s + CircuitBreaker) |
| **NotificationService** | Application | Build CloudEvent, INSERT NotificationLog, call IMessagePublisher |
| **Azure Service Bus** | External (IF-002) | Async delivery ของ Email Notification Event |

---

## 9. Traceability

| Element ใน Diagram | SRS Requirement | Design Document Reference |
|-------------------|----------------|--------------------------|
| POST /api/v1/leave-requests | SFR-003, SCR-003 | Application Architecture §8.2 |
| JWT Validation (Middleware) | NFR-004, TR-008 | Application Architecture §9.1 |
| FluentValidation DTO | VR-001–013 | Method Signature §4.4, Application Architecture §9.3 |
| VR-001 Employee Type check | VR-001, BR-011 | Method Signature §8 |
| VR-002 Balance check | VR-002, BR-002 | Method Signature §4.3 |
| VR-003/004 Probation/Service | VR-003/004, BR-007/008 | Method Signature §4.4 |
| VR-005/006 Advance Notice | VR-005/006, BR-003/004 | Method Signature §4.4 |
| VR-007 Medical Certificate | VR-007, BR-006 | Method Signature §4.4 |
| GetOverlappingAsync() | SFR-003 (Date conflict) | Method Signature §3.3 |
| BEGIN TRANSACTION | NFR-010, BR-016 | Method Signature §7 |
| SaveChangesAsync() | NFR-010 | Method Signature §7 |
| POST-COMMIT Notification | SFR-013, NFR-007, BR-019 | Integration Architecture §7.2 |
| CloudEvent type: leave.request.submitted | IF-002 §2.2.5 | Integration Architecture §10.1 |
| Polly Retry 3x (2s/4s/8s) | knowledge.md §5.1, TR-002 | Integration Architecture §9.1 |
| Circuit Breaker (5 fail → 30s) | knowledge.md §5.1 | Integration Architecture §9.1 |
| 503 HRIS_UNAVAILABLE | SIR-001 (IF-001 Open Issue A1) | Integration Architecture §14.1 |
| InsufficientLeaveBalanceException → 400 | VR-002 | Method Signature §2.3 |
| Serilog.Warning / Error | ai-std-sdlc.md §6.2 | Application Architecture §9.2 |

---

## 10. Open Issues

| OI ID | ประเด็น | ผลกระทบต่อ Sequence | สิ่งที่ต้องทำ |
|-------|---------|---------------------|------------|
| **OI-001** | **HRIS API Pattern** ยังไม่ยืนยัน (Batch vs REST) — Scenario 3 สมมติว่าใช้ REST (Pattern B) | ถ้าใช้ Batch (Pattern A) → HRIS API ไม่ได้ถูกเรียกระหว่าง Submit Leave — flow จะแตกต่าง | IT + HRIS vendor ยืนยัน IF-001 pattern |
| **OI-002** | **Employee ใหม่ยัง sync ไม่เสร็จ** — ปัจจุบัน Scenario 3 fallback ไป HRIS API แต่อาจออกแบบเป็น "รอ batch sync วันถัดไป" แทน | กำหนด behavior เมื่อพนักงานใหม่ login ก่อน HRIS batch sync | HR + IT ยืนยัน onboarding process |
| **OI-003** | **DurationDays calculation** — วิธีคำนวณ 3 วันทำการ (ไม่รวมวันหยุด?) ยังไม่ระบุ | VR-002 และ VR-007 ใช้ durationDays — ถ้า logic ต่างกัน balance check อาจผิด | HR ยืนยัน working days calculation |
| **OI-004** | **Notification fire-and-forget** — ถ้า `PublishLeaveSubmittedAsync()` fail หลัง commit → Leave Request บันทึกแล้วแต่ Email ไม่ออก | NotificationLog อยู่ใน Status=Pending ตลอดไป — ไม่มี retry mechanism สำหรับ publish phase | พิจารณา Outbox Pattern สำหรับ guaranteed notification delivery |
| **OI-005** | **HalfDay leave duration** — `IsHalfDay=true` คิดเป็น 0.5 วัน แต่ logic คำนวณ DurationDays ยังไม่ระบุในทุก step | กระทบ VR-002 balance check และ LeaveBalance.PendingDays update | Business ยืนยัน half-day calculation |

---

## 11. Source Reference

- `20-system-design/a0-architecture-design/01-application-architecture/leave-request-and-approval-application-architecture-design.md` §7, §9, §11.3
- `20-system-design/b0-functional-design/leave-request-and-approval-method-signature.md` §4.4 (ILeaveRequestService), §5.1 (IHrisAdapter)
- `10-requirement-definition/b0-system-requriement/leave-request-and-approval-system-requirement-specification-summary.md` §4.1 SFR-003, §4.6 VR-001–013
- `20-system-design/a0-architecture-design/03-integration-architecture/leave-request-and-approval-integration-architecture-design.md` §9.1 Polly, §7.2 IF-002
- `80-knowledge-base/SDLC/ai-std-sdlc.md` §3.2 (Resilience), §6.2 (Logging)

---

*Sequence Diagram นี้ครอบคลุม 3 scenarios หลัก — ทุก validation step ระบุ VR reference ชัดเจน, `SaveChangesAsync()` เรียกได้ก็ต่อเมื่อผ่าน validation ทั้งหมดแล้วเท่านั้น, HRIS API timeout ใช้ Polly Retry 3x + CircuitBreaker ตามมาตรฐานองค์กร*
