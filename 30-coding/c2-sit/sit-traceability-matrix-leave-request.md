---
title: "SIT Traceability Matrix"
document_type: "Traceability Matrix"
version: "1.0"
date: "2026-06-24"
module: "Leave Request and Approval"
company: "ABC Company"
---

# SIT Traceability Matrix — Leave Request and Approval

**วันที่สร้าง:** 2026-06-24  
**Version:** 1.0  
**สร้างโดย:** QA Team (AI-assisted, Workshop Day 2)

**Input Artifacts ที่ใช้สร้างเอกสารนี้:**

| Artifact | ไฟล์ | Version |
|----------|------|---------|
| Business Requirement Summary | `leave-request-and-approval-requirement-summary.md` | 1.0 |
| Screen SRS | `leave-request-and-approval-screen-srs.md` | 1.0 |
| Report SRS | `leave-request-and-approval-report-srs.md` | 1.0 |
| Interface SRS | `leave-request-and-approval-interface-srs.md` | 1.0 |
| Method Signature & Design | `leave-request-and-approval-method-signature.md` | 1.0 |
| Unit Test List | `test-case-list-merged.md` | 1.0 (68 TCs) |
| Unit Test Issue Summary | `unit-test-issue-summary.md` | Session 4 |
| SIT Scenario | `sit-scenario-leave-request.md` | 1.0 (SIT-001–020) |
| SIT Coverage Matrix | `sit-coverage-matrix-leave-request.md` | 1.0 |

---

## 1. Traceability Matrix

### 1A. Submit Leave Request Flow

| FR ID | Requirement | SRS ID | Design (Class/Method) | UT Case | SIT Scenario | Gap / Note |
|-------|-------------|--------|-----------------------|---------|--------------|------------|
| SFR-003 | ยื่นคำขอลา (Submit Leave Request) | SF-003, BR-001–007 | `ILeaveRequestService.SubmitLeaveRequestAsync` / `LeaveRequestController` | TC-SPEC-001–015, TC-KB-001–017 (32 TCs) | SIT-001, SIT-002, SIT-003, SIT-012, SIT-013, SIT-019, SIT-020 | ✅ UT ครบ (9 TCs skipped เพราะ ISS-004); SIT ครอบคลุม happy path + validation |
| VR-001 | Outsource ลาได้เฉพาะ 3 ประเภท (ห้าม 4 ประเภทจำกัดสิทธิ์) | VR-001, BR-011 | `SubmitLeaveRequestAsync` — employee type eligibility check | TC-KB-001 | SIT-020 | ✅ UT Pass; SIT-020 ครอบคลุม |
| VR-002 | ประเภทการลาต้องตรงกับ employee type | VR-002 | `SubmitLeaveRequestAsync` — leave type eligibility | รวมใน TC-KB-001 | SIT-020 | ⚠️ UT ครอบคลุมเฉพาะ Outsource path; Regular employee explicit path ไม่มี TC |
| VR-003 | Start Date ≤ End Date | VR-003 | `SubmitLeaveRequestAsync` — date range validation | TC-SPEC-006 (**Skipped** — ISS-004) | — (Gap) | ❌ UT Skipped (service ไม่มี logic); ไม่มี SIT scenario → ต้องสร้าง SIT-022 |
| VR-004 | Start Date ≥ Today (ห้ามย้อนหลัง) | VR-004 | `SubmitLeaveRequestAsync` — date validation | TC-SPEC-007 (**Skipped** — ISS-004) | — (Gap) | ❌ UT Skipped; ไม่มี SIT scenario → ต้องสร้าง SIT-022 |
| VR-005 | Duration ≥ 0.5 วัน (ห้าม 0 วัน) | VR-005 | `SubmitLeaveRequestAsync` — duration validation | TC-SPEC-008 (**Skipped** — ISS-004) | — (Gap) | ❌ UT Skipped; ไม่มี SIT scenario |
| VR-006 | Leave balance เพียงพอก่อน submit | VR-006, BR-004 | `SubmitLeaveRequestAsync` — balance check | TC-SPEC-004, TC-KB-003 | SIT-012 | ✅ UT Pass; SIT-012 ครอบคลุม |
| VR-007 | ลาป่วย ≥ 3 วัน ต้องแนบใบรับรองแพทย์ | VR-007, BR-006, SIR-005 | `SubmitLeaveRequestAsync` — medical cert check + `IAttachmentService.UploadAsync` / `AttachmentController` / `IFileStorageAdapter` (IF-004) | TC-SPEC-009 (**Skipped** — ISS-004) | — (Gap, proposed SIT-023) | ❌ VR-007 ยังไม่ implement ใน service; UT Skipped; ไม่มี SIT |
| VR-008 | ห้ามวันที่ซ้อนทับกับคำขอที่มีอยู่แล้ว | VR-008, BR-003 | `SubmitLeaveRequestAsync` — overlap check | TC-SPEC-005, TC-KB-006 | — (Gap, proposed SIT-024) | ⚠️ UT Pass; ไม่มี SIT scenario ที่ทดสอบ overlap explicitly |
| VR-009 | Half-day ต้องระบุ period AM/PM | VR-009 | `SubmitLeaveRequestAsync` — half-day validation | TC-KB-011, TC-KB-012 (บางส่วน skipped) | SIT-019 | ⚠️ UT บางส่วน skipped; SIT-019 ครอบคลุม happy path |
| VR-011 | Cancel reason เป็น optional (SubmitCancel) | VR-011 | `ICancelRequestService.SubmitCancelAsync` | — (ไม่มี explicit TC) | — (ไม่มี explicit scenario) | ⚠️ ไม่มี UT / SIT ที่ทดสอบ optional reason path |
| VR-012 | ลากิจต้องยื่นล่วงหน้า ≥ 3 วันทำการ | VR-012, BR-008 | `SubmitLeaveRequestAsync` — advance notice check | TC-SPEC-010 (**Skipped** — ISS-004) | SIT-003 (happy path เท่านั้น) | ❌ UT Skipped; SIT-003 ทดสอบ happy path แต่ไม่มี scenario ทดสอบ violation |
| VR-013 | Excel Import validation (Outsource) | VR-013, SIR-003 | `IImportService.ImportAsync` / IF-003 | — (ไม่มี) | — (Gap, proposed SIT-021) | ❌ ไม่มี UT / SIT ทั้งคู่ |

### 1B. Approve / Reject Flow

| FR ID | Requirement | SRS ID | Design (Class/Method) | UT Case | SIT Scenario | Gap / Note |
|-------|-------------|--------|-----------------------|---------|--------------|------------|
| SFR-004 | ดูรายละเอียดคำขอลา (Leave Detail View) | SF-004, SCR-003 | `ILeaveRequestService.GetDetailAsync` / `LeaveRequestDetailDto` | — (**ISS-006** gap) | SIT-004, SIT-005 (implicit เมื่อดูก่อน approve/reject) | ⚠️ ไม่มี UT; SIT ทดสอบ implicit แต่ไม่ assert response schema |
| SFR-005 | อนุมัติ / ปฏิเสธคำขอลา (Approve / Reject) | SF-005, BR-012–014 | `IApprovalService.ApproveAsync`, `IApprovalService.RejectAsync` / `ApprovalController` / `ApproveRejectDto` | TC-SPEC-021–032, TC-KB-023–036 (26 TCs; TC-SPEC-026, 032 Skipped — ISS-005) | SIT-004, SIT-005, SIT-014, SIT-015 | ✅ UT ครบ (2 TCs skipped เรื่อง notification); SIT ครอบคลุมครบ |
| VR-010 | Rejection reason required เมื่อ Reject | VR-010, BR-013 | `IApprovalService.RejectAsync` | TC-KB-024, TC-KB-025 | SIT-014 | ✅ UT Pass; SIT-014 ครอบคลุม |

### 1C. Cancel Flow

| FR ID | Requirement | SRS ID | Design (Class/Method) | UT Case | SIT Scenario | Gap / Note |
|-------|-------------|--------|-----------------------|---------|--------------|------------|
| SFR-007 | ยกเลิกคำขอที่ยัง Pending (Cancel Pending — ทันที) | SF-007, BR-015 | `ICancelRequestService.CancelAsync` (direct cancel) / `CancelRequestController` | TC-SPEC-016–020, TC-KB-018–022 (10 TCs) | SIT-006, SIT-016 | ✅ UT ครบ; SIT-006 ครอบคลุม / **ISS-003:** TC-SPEC-020 conflict ยังไม่ resolve |
| SFR-008 | ขอยกเลิกคำขอที่ Approved แล้ว (Submit Cancel Request) | SF-008, BR-016 | `ICancelRequestService.SubmitCancelAsync` / `SubmitCancelRequestDto` | TC-SPEC-020 (**disputed** — ISS-003; spec บอก Approved→throw, impl สร้าง CancelRequest) | SIT-007 | ⚠️ UT conflict unresolved; SIT-007 รันได้แต่ความถูกต้องยังไม่ยืนยัน |
| SFR-009 | Manager อนุมัติ/ปฏิเสธคำขอยกเลิก (Approve/Reject Cancel) | SF-009, BR-017 | `ICancelRequestService.ApproveCancelAsync`, `ICancelRequestService.RejectCancelAsync` | — (**ISS-001, ISS-002** — P1 Blocking: ไม่มี UT เลย) | SIT-008, SIT-009 | ❌ **P1 Blocking:** ไม่มี UT แม้แต่ TC เดียว; SIT-008/009 รันได้แต่ขาด UT safety net |

### 1D. Dashboard / View Flow

| FR ID | Requirement | SRS ID | Design (Class/Method) | UT Case | SIT Scenario | Gap / Note |
|-------|-------------|--------|-----------------------|---------|--------------|------------|
| SFR-001 | Login / Authentication | SF-001, SCR-001 | `EmployeeController` / Mock Auth (`X-Employee-Id` header) | — (auth layer ไม่ใน UT scope) | Implicit ทุก scenario — ใช้ Mock Auth | ⚠️ ไม่มี explicit SIT scenario ทดสอบ login form / auth failure → ต้องสร้าง SIT-025 |
| SFR-002 | Leave Balance Dashboard | SF-002, SCR-002 | `ILeaveBalanceService.GetDashboardAsync` / `LeaveBalanceDashboardDto` | — (ISS-009 gap) | SIT-017, SIT-018 (ตรวจ balance accuracy หลัง action) | ⚠️ ไม่มี UT; SIT-017/018 ตรวจ balance indirect |
| SFR-006 | ดูรายการคำขอของตัวเอง (My Request List) | SF-006, SCR-005 | `ILeaveRequestService.GetMyRequestsAsync` / `LeaveRequestFilterDto` | — (ISS-009 gap) | SIT-006, SIT-007 (implicit navigation) | ⚠️ ไม่มี UT; SIT ทดสอบ implicit (navigate ไป list ก่อน cancel) |
| SFR-011 | HR Monitoring Dashboard | SF-011, SCR-008 | `HrController` / `ILeaveRequestService.GetAllForHrAsync` / `HrLeaveFilterDto` | — (ISS-009 gap) | SIT-010, SIT-011 | ⚠️ ไม่มี UT; SIT-010/011 ครอบคลุม happy path + filter |

### 1E. Integration / Interface

| FR ID | Requirement | SRS ID | Design (Class/Method) | UT Case | SIT Scenario | Gap / Note |
|-------|-------------|--------|-----------------------|---------|--------------|------------|
| SFR-010 / IF-005 | SLA Auto-reminder (Background Process) | SF-010, SIR-004 | `ISlaSchedulerService` (IHostedService) / IF-005 SLA Timer Event | — (ไม่มี) | — (ไม่อยู่ใน SIT scope — std-sit.md §7.2) | ⚠️ ไม่มี UT / SIT; ต้องมีแผนทดสอบ scheduler แยก |
| SFR-012 / IF-003 | Outsource Excel Import | SF-012, SIR-003, VR-013 | `IImportService.ImportAsync` / `HrController` / IF-003 | — (ไม่มี) | — (Gap, proposed SIT-021) | ❌ ไม่มี UT / SIT — Phase 1 feature แต่ยังไม่มี test coverage ใดเลย |
| SFR-013 / IF-002 | Email Notification (Phase 2) | SF-013, SIR-002 | `INotificationService` / `IMessagePublisher` / `IEmailConsumer` / IF-002 | TC-SPEC-015, 026, 032 (**Skipped** — ISS-005: dependency ไม่ inject) | — (ตรวจสอบผ่าน API-only ไม่ได้) | ⚠️ Phase 2; UT Skipped; SIT ไม่สามารถ verify email ผ่าน API execution |
| IF-001 | HRIS Employee Master Sync | SIR-001, TR-002 | `IHrisAdapter` | — (ไม่มี) | — (ไม่อยู่ใน SIT scope — external system) | ⚠️ ต้องมีแผน Integration Test กับ HRIS system แยก (Phase 2 / Staging) |
| IF-004 | Medical Certificate File Upload | SIR-005, VR-007 | `IAttachmentService.UploadAsync` / `AttachmentController` / `IFileStorageAdapter` | TC-SPEC-009 (**Skipped** — ISS-004) | — (Gap, proposed SIT-023) | ❌ VR-007 ยังไม่ implement ใน service; UT Skipped; ไม่มี SIT |

### 1F. Report (Phase 2 — Deferred)

| FR ID | Requirement | SRS ID | Design (Class/Method) | UT Case | SIT Scenario | Gap / Note |
|-------|-------------|--------|-----------------------|---------|--------------|------------|
| RFR-001 / RP-001 | Leave Summary Report | SFR-015, RFR-001, SCR-010 | `HrController` (report endpoint) / `LeaveReportFilterDto` / `ReportFormat` | — (ไม่มี) | — (Phase 2) | ⏸️ Phase 2 Deferred — ไม่ได้ทดสอบในรอบนี้ |
| RFR-002 / RP-002 | Leave Balance Report | SFR-015, RFR-002, SCR-010 | `HrController` / `LeaveReportFilterDto` | — (ไม่มี) | — (Phase 2) | ⏸️ Phase 2 Deferred |
| RFR-003 / RP-003 | Notification Log Report | SFR-015, RFR-003, SCR-011 | `HrController` (endpoint ยังไม่ระบุชัดเจนใน design) | — (ไม่มี) | — (Phase 2) | ⏸️ Phase 2 Deferred + Design endpoint ยังไม่ระบุ |

---

## 2. FR ที่ขาด Artifact ในบาง Phase

| FR ID | Requirement | Phase ที่ขาด | รายละเอียดช่องโหว่ |
|-------|-------------|-------------|-------------------|
| VR-003, VR-004, VR-005 | Date validation, Duration | **UT** (Skipped) + **SIT** (ไม่มี scenario) | ISS-004: service ยังไม่ implement logic → UT skipped, SIT ไม่สามารถทดสอบได้ |
| VR-007 | Medical certificate ≥ 3 days Sick | **Design** (IF-004 ยังไม่ implement ใน service) + **UT** (Skipped) + **SIT** | ทั้ง VR-007 และ IF-004 integration ยังไม่ครบ |
| VR-012 | Business Leave advance notice | **UT** (Skipped) + **SIT** (happy path เท่านั้น) | UT ทดสอบ violation ไม่ได้; SIT ไม่มี negative case |
| VR-013, SFR-012 | Outsource Excel Import | **UT** (ไม่มีเลย) + **SIT** (ไม่มีเลย) | มีเฉพาะ Design (IImportService defined) แต่ไม่มี test ใดๆ |
| SFR-009 | Manager approve/reject cancel request | **UT** (ไม่มีเลย — ISS-001, ISS-002) | มีเฉพาะ Design + SIT แต่ขาด UT safety net ที่ P1 Blocking |
| SFR-008 | Submit Cancel on Approved leave | **UT** (conflict unresolved — ISS-003) | TC-SPEC-020 vs TC-KB-020 ขัดแย้ง — ไม่มี FR ระบุ behavior ที่ถูกต้อง |
| SFR-001 (Login) | Authentication | **SIT** (ไม่มี explicit scenario) | ทุก scenario ใช้ Mock Auth; login form / auth failure ไม่ถูกทดสอบ |
| SFR-002, SFR-006 | Balance Dashboard, My Request List | **UT** (ไม่มี — ISS-009) | Query methods ไม่มี UT (P3 gap); tested only implicit ใน SIT |
| SFR-010, IF-005 | SLA Auto-reminder | **UT** (ไม่มีเลย) + **SIT** (excluded from scope) | ไม่มีแผนทดสอบ scheduler ชัดเจน |
| SFR-013, IF-002 | Email Notification | **UT** (Skipped — ISS-005) + **SIT** (ไม่สามารถ verify) | Phase 2 + dependency ยังไม่ inject + ไม่มี email inbox ใน SIT environment |
| IF-001 | HRIS Employee Sync | **UT** (ไม่มี) + **SIT** (ไม่ใน scope) | External integration ต้องมีแผน Integration Test แยก |
| RFR-001–003, RP-001–003 | Reports ทั้ง 3 รายการ | **UT** (ไม่มี) + **SIT** (ไม่มี) | Phase 2 Deferred — ต้องวาง plan แยก |

---

## 3. Orphan Artifacts (Artifact ที่ไม่มี FR รองรับโดยตรง)

| Artifact ID | ประเภท Artifact | รายละเอียด | สถานะ |
|-------------|----------------|-----------|-------|
| TC-SPEC-020 conflict กับ TC-KB-020 | UT — conflicting spec | TC-SPEC-020: `CancelLeaveRequestAsync` เมื่อ status=Approved → ควร throw; TC-KB-020: implementation สร้าง CancelRequest แทน — ไม่มี FR ระบุ behavior ที่ถูกต้องชัดเจน | **ต้อง resolve กับ PO/BA** (ISS-003) ก่อน SIT sign-off |
| OBS-001 Thai text encoding "???" | Observation (curl limitation) | ตัวอักษรไทยใน reason field แสดงเป็น "???" เมื่อส่งผ่าน curl บน Windows — ไม่มี VR ระบุ encoding spec อย่างชัดเจน | Trivial — ตรวจผ่าน Browser form แทน curl |
| TC-KB-010, TC-KB-011, TC-KB-012 (บางส่วน) | UT — FluentValidation scope | Test เหล่านี้ตรวจ validation ที่ FluentValidation/Controller layer แทน Service layer — ISS-008 ระบุว่าควรเป็น Integration Test แต่ยังไม่มี FR ระบุ validation ownership ชัดเจน | ⚠️ ต้องการ Integration Test ชุดใหม่ผ่าน HTTP layer |
| ISS-007 (null balance path ใน CancelAsync) | UT — unresolved edge case | CancelAsync ไม่มี test สำหรับ null balance path — edge case ที่อาจเกิดจาก data inconsistency; ไม่มี VR กำหนด behavior เมื่อ balance = null | ⚠️ ต้องเพิ่ม TC-KB และยืนยัน behavior กับ DEV |
| B-001 Antigravity MCP not installed | Tooling blocker | Browser screenshot evidence ยังไม่สามารถ capture ได้ (EVD-001–009 pending) — ไม่ใช่ FR gap แต่ทำให้ SIT evidence ไม่ครบตาม standard | **Blocker** — ต้อง install Antigravity MCP ก่อน SIT sign-off |
| ISS-010 (TC-KB-024/025 redundancy) | UT — refactor opportunity | TC-KB-024 (null reason) + TC-KB-025 (empty reason) ควรรวมเป็น `[Theory][InlineData]` — ไม่ใช่ bug แต่ code quality | P3 — refactor เมื่อมีเวลา |

---

## 4. Gap Analysis Summary — สิ่งที่ต้องปิดก่อน SIT Sign-off

### 4.1 Priority 1 — Blocking (ห้ามออก Production โดยไม่ปิด)

| Gap ID | FR ที่เกี่ยวข้อง | ปัญหา | Action Required | Owner |
|--------|----------------|-------|----------------|-------|
| **GAP-001** | SFR-009 | `ApproveCancelAsync` ไม่มี UT เลย (ISS-001) — business logic ซับซ้อนเท่ากับ `ApproveAsync` | เพิ่ม UT ครอบคลุม happy path + exception path | DEV |
| **GAP-002** | SFR-009 | `RejectCancelAsync` ไม่มี UT เลย (ISS-002) — มี status rollback ที่ต้องตรวจ | เพิ่ม UT ครอบคลุม reject + status restore | DEV |
| **GAP-003** | SFR-007, SFR-008 | TC-SPEC-020 vs TC-KB-020 conflict — ไม่รู้ว่า Cancel Approved behavior ที่ถูกต้องคืออะไร (throw vs create CancelRequest) | Resolve กับ PO/BA (ISS-003) → แก้ spec → แก้ UT ให้ตรงกับ behavior จริง | PO/BA + DEV |
| **GAP-004** | VR-003, VR-004, VR-005, VR-007, VR-012 | Service layer ไม่ implement validation logic → 9 UT cases skipped → SIT gap ตามมา | Implement VR-003–007 ใน service (ISS-004) → unskip UT → สร้าง SIT-022, SIT-023 | DEV |

### 4.2 Priority 2 — ควรปิดก่อน Go-live

| Gap ID | FR ที่เกี่ยวข้อง | ปัญหา | Action Required | Owner |
|--------|----------------|-------|----------------|-------|
| **GAP-005** | SFR-013, IF-002 | `INotificationService` ไม่ได้ inject → email notification ตรวจสอบไม่ได้ทั้ง UT และ SIT (ISS-005) | Inject INotificationService → unskip TC-SPEC-015/026/032 → วางแผน email verification ใน SIT (mock inbox หรือ real email) | DEV + QA |
| **GAP-006** | SFR-012, VR-013, IF-003 | Outsource Excel Import ไม่มี UT / SIT ทั้งคู่ (Phase 1 feature) | สร้าง UT สำหรับ `IImportService` → สร้าง SIT-021 | DEV + QA |
| **GAP-007** | SFR-004, SFR-002, SFR-006 | `GetDetailAsync`, `GetDashboardAsync`, `GetMyRequestsAsync` ไม่มี UT (ISS-006, ISS-009) — อย่างน้อย NotFoundException path เป็น bug-prone | เพิ่ม UT อย่างน้อย happy path + NotFoundException (ISS-006 P2) | DEV |
| **GAP-008** | VR-008 | Overlap check มี UT แต่ไม่มี SIT scenario ที่ทดสอบ date overlap explicitly | สร้าง SIT-024 ทดสอบ overlap error scenario | QA |
| **GAP-009** | SFR-001 | ไม่มี SIT scenario ทดสอบ Login / Auth failure / Session expiry | สร้าง SIT-025 ทดสอบ auth validation | QA |
| **GAP-010** | — (Tooling) | Antigravity MCP ไม่ได้ install → ไม่สามารถ capture browser screenshot (B-001) → EVD-001–009 pending | Install Antigravity Browser Integration MCP → re-execute SIT-001 และ SIT-002 เพื่อ capture evidence | QA Lead |

### 4.3 Priority 3 — Nice to Have / Phase 2 Planning

| Gap ID | FR ที่เกี่ยวข้อง | ปัญหา | Action Required |
|--------|----------------|-------|----------------|
| **GAP-011** | SFR-010, IF-005 | SLA scheduler ไม่มี UT / SIT ใดๆ | วางแผน Integration Test สำหรับ scheduler (ต้องการ test clock control) |
| **GAP-012** | RFR-001–003, RP-001–003, SFR-015 | Report functions ทั้งหมดเป็น Phase 2 ไม่มี test ใดๆ | วางแผน Phase 2 SIT round แยก (หลัง Phase 2 development เสร็จ) |
| **GAP-013** | IF-001 | HRIS Employee Sync ไม่มี test — external system dependency | วางแผน Integration Test กับ HRIS ใน Phase 2 / Staging environment |
| **GAP-014** | ISS-008 (FluentValidation) | Validation ที่ Controller/FluentValidation layer ไม่มี Integration Test | เพิ่ม Integration Test ผ่าน HTTP layer สำหรับ MaxLength, date format (VR-003 path) |

---

## 5. SIT Sign-off Readiness Summary

| Business Flow | Readiness | เงื่อนไข / สิ่งที่ต้องปิด |
|--------------|-----------|--------------------------|
| Submit Leave Request (Core) | ✅ API Pass — ⚠️ Browser pending | ต้อง install Antigravity MCP (GAP-010) สำหรับ screenshot evidence |
| Approve / Reject Leave | ✅ API Pass — ⚠️ Browser pending | เช่นเดียวกับ Submit; notification ยังตรวจไม่ได้ |
| Cancel Pending Leave | ✅ API Pass — ⚠️ ISS-003 conflict | ต้อง resolve TC-SPEC-020 conflict ก่อนถือว่า sign-off จริง (GAP-003) |
| Submit Cancel Approved | ⚠️ SIT รันได้ แต่ behavior ยังขัดแย้ง | ISS-003 ต้อง resolve ก่อน (GAP-003) |
| Approve/Reject Cancel Request | ⚠️ SIT รันได้ แต่ขาด UT safety net | ต้องสร้าง UT ก่อน (GAP-001, GAP-002) |
| Validation Rules (VR-003–007, VR-012) | ❌ ยังไม่พร้อม | service ยังไม่ implement → ต้อง implement + UT + SIT (GAP-004) |
| Outsource Import (SFR-012) | ❌ ยังไม่พร้อม | ไม่มี UT / SIT ใดเลย (GAP-006) |
| Email Notification (SFR-013) | ❌ Phase 2 / ยังไม่พร้อม | ต้อง inject dependency + setup email verification (GAP-005) |
| HR Monitoring (SFR-011) | ✅ SIT-010/011 ครอบคลุม | ไม่มี UT (P3 gap) — acceptable สำหรับ query wrapper |
| Reports (SFR-015, RP-001–003) | ⏸️ Phase 2 Deferred | ไม่ใช่ scope SIT รอบนี้ — ต้องวาง plan Phase 2 |

### ข้อสรุป

**Sign-off ได้สำหรับ:** Submit + Approve + Reject core flow (หลังปิด GAP-010 browser evidence)

**ห้าม Sign-off โดยไม่ปิด:** GAP-001, GAP-002 (UT ApproveCancelAsync/RejectCancelAsync), GAP-003 (ISS-003 conflict resolution), GAP-004 (VR-003–007 implementation)

**Deferred อย่างตั้งใจ:** Reports (Phase 2), SLA scheduler, HRIS sync, Email notification full verification
