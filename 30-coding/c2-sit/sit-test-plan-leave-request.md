# SIT Test Plan — Leave Request and Approval

**Module:** Leave Request and Approval  
**Version:** 1.0  
**วันที่สร้าง:** 2026-06-16  
**สร้างโดย:** AI-assisted (Claude)  
**Source Artifacts:** Screen SRS v1.1, Report SRS v1.0, Interface SRS v1.0, SIT Scenario v1.0, Coverage Matrix v1.0, Unit Test Issue Summary, std-sit.md, lesson-learned-sit-defects.md

---

## 1. สรุปภาพรวม Test Plan

Test Plan นี้จัดลำดับการทดสอบ SIT ตามหลักการ **dependency ต่ำ → สูง** เพื่อให้ทีมสามารถตรวจพบ defect ได้เร็วและแยก root cause ได้ชัด โดยแบ่งเป็น 7 Wave รวม 25 แผนการทดสอบ

### 1.1 สรุป Wave Overview

| Wave | ชื่อ Wave | Plan Count | Dependency | Priority Focus |
|------|---------|-----------|-----------|---------------|
| W1 | Screen Validation & Form Behavior | 7 | None | High |
| W2 | Core Submit Transaction | 4 | Low | High |
| W3 | Approval & Rejection Flow | 3 | Medium | High |
| W4 | Cancel & State Transition Flow | 5 | Medium–High | Medium–High |
| W5 | Report & HR Monitoring Verification | 3 | High | Medium |
| W6 | Interface & API Integration | 4 | Low–Medium | Medium |
| W7 | End-to-End Cross-Component Verification | 2 | Very High | High |

### 1.2 Entry Criteria (ตาม std-sit.md §2)

- [ ] Unit Test ผ่านไม่ต่ำกว่า 70% (ปัจจุบัน 76% — ผ่าน)
- [ ] ไม่มี P1 defect ค้างที่ยัง block ภาค UT (ISS-001/002 ApproveCancelAsync/RejectCancelAsync ต้อง resolve ก่อน W4)
- [ ] Application build สำเร็จ deploy บน SIT environment (http://localhost:5173)
- [ ] Seed data พร้อม: EMP001/EMP002/EMP003, LeaveBalances, LeaveTypes, Holiday Calendar 2026
- [ ] SIT Scenario, Test Data, Automation Draft ได้รับ review แล้ว

### 1.3 Exit Criteria (ตาม std-sit.md §3)

- [ ] ทุก scenario priority High ผ่าน 100%
- [ ] ทุก scenario priority Medium ผ่าน ≥ 90%
- [ ] ไม่มี Critical defect ค้าง
- [ ] High defect ค้างไม่เกิน 2 รายการ และมี workaround ชัดเจน
- [ ] Evidence (screenshot) ครบทุก scenario ที่ผ่าน

---

## 2. Test Wave Table

### Wave 1 — Screen Validation & Form Behavior
**หลักการ:** ทดสอบ validation rule และ form behavior ที่ไม่ต้องพึ่ง state จาก scenario อื่น — สามารถรันแยกกันได้ทั้งหมด  
**วิธีเตรียม:** ใช้ seed data เริ่มต้น ไม่ต้องรัน scenario ก่อนหน้า

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W1 | TP-W1-01 | **Required Fields Validation (SF-003)** — ทดสอบ 3 กรณี: (1) ไม่เลือกประเภทลา, (2) ไม่ระบุวันที่, (3) ไม่กรอกเหตุผล → ระบบ block พร้อม error message ชัดเจน | None | SF-003 (SCR-003), SPA Client Validation, VR-002 | SIT-013 | EMP001 login สำเร็จ | ทุกกรณี: error message แสดงบนหน้าจอ (ไม่ใช่ spinner ค้าง) — ไม่มี API call ออก — DB ไม่สร้าง record | DEF-003: ถ้า spinner ค้างแทน error = bug; ตรวจ Network tab ว่าไม่มี POST /api/v1/leave-requests ออก |
| W1 | TP-W1-02 | **Reject Without Reason Blocked (SF-005)** — Manager กด Reject แต่ไม่กรอก rejection reason → ระบบ block | None (ต้องการ Pending request ของ EMP001 ใน seed) | SF-005 (SCR-004), Modal, VR-010 | SIT-014 | EMP002 (Manager) login, มีคำขอ Pending ใน seed data | UI block: error "กรุณาระบุเหตุผลการปฏิเสธ" — DB status ไม่เปลี่ยน — API bypass test → HTTP 422 | Note: ในระบบจริง modal ใน ApprovalListPage ตรวจ `if (modalAction === 'reject' && !comment.trim())` ก่อน submit |
| W1 | TP-W1-03 | **Half-Day Leave Form Behavior (SF-003)** — ตรวจ field `half_day_period` แสดง/ซ่อน ตาม toggle + ตรวจ DurationDays = 0.5 | None | SF-003 (SCR-003), SFR-003, VR-011 | SIT-019 | EMP001 login, Annual balance ≥ 0.5 วัน | Field `half_day_period` แสดงเมื่อ start=end + เลือก half-day — Total Days = 0.5 — DB: DurationDays=0.5, IsHalfDay=true, PendingDays+0.5 | SRS SF-003: field ซ่อนอัตโนมัติเมื่อ start_date ≠ end_date — ตรวจ edge case นี้ด้วย |
| W1 | TP-W1-04 | **Outsource Leave Type Restriction (SF-003)** — Outsource employee ไม่เห็น 4 ประเภทลาที่จำกัด ใน dropdown, API bypass → HTTP 422 | None (ต้องมี Outsource account ใน seed) | SF-003, BR-011, VR-001, SFR-003 | SIT-020 | EMP_OUT (Outsource) login | UI: dropdown ไม่แสดง ลาคลอด/ทำหมัน/รับราชการ/อุปสมบท — API bypass → errorCode VR-001 | ต้องเพิ่ม Outsource test account ใน seed data (ยังไม่มีใน test-data YAML ปัจจุบัน) |
| W1 | TP-W1-05 | **Sick Leave Form — Medical Certificate Field (SF-003)** — ตรวจ field แสดง/ซ่อนตามจำนวนวัน: (1) ≤ 2 วัน: field ซ่อน, (2) ≥ 3 วัน: field แสดงเป็น required | None | SF-003, VR-007, BR-006, IF-004 | SIT-002 (extend กรณี ≥ 3 วัน) | EMP001 login | (1) ลาป่วย 1 วัน: ไม่มี field แนบใบแพทย์ — (2) ลาป่วย 3 วัน: field `medical_certificate` แสดง + Submit ไม่ได้ถ้าไม่แนบ | Coverage Matrix: VR-007 มีเฉพาะ ≤ 2 วัน case — ต้องทดสอบ ≥ 3 วัน เพิ่มใน test run นี้ |
| W1 | TP-W1-06 | **Business Leave Advance Notice Validation (SF-003)** — (1) ยื่นล่วงหน้า < 3 วันทำการ → ระบบ block, (2) ยื่นล่วงหน้า ≥ 3 วันทำการ → pass | None | SF-003, VR-006, BR-004, SFR-003 | SIT-003 (happy path) + error case เพิ่ม | EMP001 login, Business Leave balance ≥ 1 วัน | Error case: ERR-LR-006 แสดง "ลากิจต้องแจ้งล่วงหน้าอย่างน้อย 3 วันทำการ" — Happy case: Submit สำเร็จ | Coverage Matrix: VR-006 ยังไม่มี error case — จัดการในรอบเดียวกัน |
| W1 | TP-W1-07 | **Probation Employee Blocked from Annual Leave (SF-003)** — พนักงานที่อายุงาน < 3 เดือน ยื่น Annual Leave → ระบบ block พร้อม error | None (ต้องมี Probation account ใน seed) | SF-003, VR-003, BR-007, M2 (QA v3) | SIT-023 (เพิ่มใหม่ — ❌ ใน Coverage Matrix) | EMP_PROB (อายุงาน < 3 เดือน) login | ERR-LR-003 แสดง "ยังอยู่ในช่วงทดลองงาน ไม่มีสิทธิ์ลาพักผ่อน" — DB: ไม่สร้าง record | **P1 Risk:** ISS-004 — service ยังไม่ implement VR-003 → ถ้า test fail แสดงว่า UT gap confirmed ที่ระดับ SIT; ต้องเพิ่ม Probation account ใน seed |

---

### Wave 2 — Core Submit Transaction
**หลักการ:** ทดสอบ happy path การยื่นคำขอลา และ boundary validation ที่ require state ของ balance  
**วิธีเตรียม:** Seed data ใหม่ (reset balance), ไม่ depend on Wave 1 output

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W2 | TP-W2-01 | **Submit Annual Leave — Happy Path (SF-003)** — EMP001 ยื่นลาพักผ่อน 3 วัน (2026-07-01 ถึง 2026-07-03) → Status=Pending, PendingDays+3 | Low (seed data พร้อม) | SF-003 (SCR-003), API /api/v1/leave-requests, DB LeaveRequests + LeaveBalances, IF-002 (Email) | SIT-001 | EMP001 login, Annual balance remaining ≥ 3 วัน | UI: "ยื่นคำร้องสำเร็จ (LR-2026-xxxxx)" — DB: Status=Pending, DurationDays=3, PendingDays=3 — Row ปรากฏใน My Requests list | **Base scenario สำหรับ Wave 3–4** — หากล้มเหลว Wave 3 ไม่สามารถดำเนินต่อ; บันทึก leaveRequestId สำหรับใช้ใน Wave 3 |
| W2 | TP-W2-02 | **Balance Insufficient Blocks Submit (SF-003)** — EMP001 ยื่น Annual Leave 5 วัน ทั้งที่ Remaining = 1 วัน | Low (seed data ต้องกำหนด remaining = 1) | SF-003, VR-002, API error response, UI error display | SIT-012 | EMP001 login, Annual remaining = 1 วัน (seed ค่าพิเศษ) | **DEF-003 check:** UI แสดง "สิทธิ์วันลาไม่เพียงพอ คงเหลือ 1 วัน" ชัดเจน — ไม่ใช่ spinner ค้าง — DB: ไม่สร้าง record | ต้องใช้ environment แยกหรือ reset balance ก่อนรัน test นี้ (ไม่ควรรัน state เดียวกับ TP-W2-01) |
| W2 | TP-W2-03 | **Self-Approve Forbidden (SF-005)** — EMP002 พยายาม approve คำขอของตัวเองผ่าน API โดยตรง | Low (API-only test) | API /api/v1/approvals/{id}/approve, Authorization, VR-010 | SIT-015 | มีคำขอ Pending ของ EMP002 (ต้องสร้างใน seed หรือ submit ก่อน), API test โดยตรง | HTTP 403 หรือ 422 + errorCode=FORBIDDEN — DB: Status ไม่เปลี่ยน | UI layer: ปุ่ม Approve ไม่แสดงในหน้าของตัวเอง (Manager เห็นแค่ทีมตัวเอง) — ตรวจทั้ง API + UI |
| W2 | TP-W2-04 | **HR Submits Leave → Appears in Manager Queue (DEF-002)** — EMP003 (HR, ManagerId=EMP002) ยื่นลา → EMP002 เห็นใน Approval Inbox | Low (seed: EMP003.ManagerId = EMP002) | SF-003, SF-004, SIT DEF-002, BR-012 | SIT-011 | EMP003 login; EMP002 login ต่อ | EMP003 submit สำเร็จ — EMP002 เข้า Approval Inbox เห็น row ของ EMP003 (ไม่หายเพราะ ManagerId=null) | **DEF-002 risk:** ถ้า EMP003.ManagerId ไม่ถูก seed → queue ว่าง = defect confirmed |

---

### Wave 3 — Approval & Rejection Flow
**หลักการ:** ทดสอบ Manager action บน Pending request ที่สร้างจาก Wave 2  
**วิธีเตรียม:** ต้องมีผล TP-W2-01 PASS และ leaveRequestId ของ EMP001

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W3 | TP-W3-01 | **Manager Approves Leave → Balance Deducted (SF-005)** — EMP002 approve คำขอของ EMP001 → Status=Approved, balance deduct | Medium (TP-W2-01 ผ่าน) | SF-004, SF-005, SCR-004, API /api/v1/approvals/{id}/approve, DB LeaveBalances | SIT-004 | EMP002 login; มี Pending request จาก TP-W2-01 | Modal "✅ ยืนยันการอนุมัติ" → "อนุมัติสำเร็จ" banner — DB: Status=Approved, ApprovedBy=EMP002 — **DEF-001 check:** PendingDays→0, UsedDays เพิ่ม | **DEF-001 Risk:** ถ้า UsedDays ไม่เพิ่ม = critical defect; บันทึก leaveRequestId(Approved) สำหรับ Wave 4 |
| W3 | TP-W3-02 | **Balance Accuracy Immediately After Approve (SF-002)** — EMP001 login ทันทีหลัง TP-W3-01 → ตรวจ balance dashboard + DB query | Medium (TP-W3-01 ผ่าน) | SF-002 (SCR-002), DB LeaveBalances, VR-008 | SIT-017 | EMP001 login ทันทีหลัง approve | **DEF-001 deep verify:** Dashboard: Annual UsedDays=5, PendingDays=0, Remaining=7 — DB: ตรงกับ UI ทุก field | Race condition risk (EC-SIT-002): ถ้า UI ยัง stale → refresh และตรวจอีกครั้ง |
| W3 | TP-W3-03 | **Manager Rejects + Employee Sees Rejection Reason (SF-005, SF-006)** — EMP002 reject คำขอใหม่พร้อมระบุเหตุผล → EMP001 เห็น rejection reason ใน Leave History | Medium (ต้องมี Pending request ใหม่ — สร้างในรอบเดียวกัน) | SF-005, SF-006, SCR-004, SCR-005, LeaveHistory | SIT-005 | EMP001 ยื่นคำขอใหม่ (Annual หรือ Sick), EMP002 reject พร้อมกรอก "ช่วงนั้นคนน้อยเกินไป" | **DEF-006 check:** EMP001 เห็น rejection reason ชัดเจนใน Leave History — DB: Status=Rejected, RejectionReason ไม่ว่าง, PendingDays คืน | DEF-006: API อาจ return reason แต่ Frontend ไม่ render — ตรวจทั้ง API response + UI text |

---

### Wave 4 — Cancel & State Transition Flow
**หลักการ:** ทดสอบ cancel flow ทั้ง 2 กรณี (Pending / Approved) รวม re-approve Cancel Request  
**วิธีเตรียม:** บาง plan ต้องมี Approved request จาก Wave 3; บาง plan ต้องสร้าง Pending ใหม่

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W4 | TP-W4-01 | **Cancel Pending Leave → Instant Cancel (SF-007)** — EMP001 ยกเลิกคำขอที่ Status=Pending ทันที ไม่ต้องรอ Manager | Medium (ต้องมี Pending request) | SF-007 (SCR-006), BR-014, WRN-CAN-001 | SIT-006 | EMP001 login; มีคำขอ Status=Pending (สร้างใหม่หรือยืมจาก Wave 2 ถ้ายังไม่ถูก approve) | UI: Status→Cancelled ทันที — DB: PendingDays คืนกลับ (balance restore) — ไม่มี Email (per SFR-007) | EC-SIT-003: ต้องตรวจ DB ว่า PendingDays คืนจริง ไม่ใช่แค่ UI; บันทึก leaveRequestId(Cancelled) สำหรับ TP-W4-02 |
| W4 | TP-W4-02 | **Double-Cancel Guard (DEF-005)** — EMP001 พยายาม cancel คำขอที่ Cancelled แล้ว → ระบบ block | High (TP-W4-01 ผ่าน) | SF-007, VR-009, API error handling | SIT-016 | มีคำขอ Status=Cancelled จาก TP-W4-01 | UI: ปุ่ม "ยกเลิก" ไม่แสดงสำหรับ Cancelled request — API bypass: HTTP 422 + errorCode=INVALID_STATUS | DEF-005 risk: ถ้า API ไม่ guard transition → double cancel ได้ |
| W4 | TP-W4-03 | **Cancel Approved Leave → CancelRequest Created (SF-008)** — EMP001 ขอยกเลิกคำขอ Approved → Status=CancelRequested + CancelRequest record ใน DB | High (TP-W3-01 ผ่าน) | SF-008 (SCR-006), SFR-008, DB CancelRequests, IF-002 (Email), SLA Timer | SIT-007 | EMP001 login; มีคำขอ Status=Approved จาก TP-W3-01 | UI: Status=CancelRequested — **DEF-004 check:** DB CancelRequests มี row ใหม่ — Manager cancel queue มี entry — Email: EMP002+EMP003 ได้รับแจ้ง — SlaDeadline = UtcNow+1 วันทำการ | DEF-004: ถ้า CancelRequests ไม่มี INSERT → Manager queue ว่าง = defect; บันทึก cancelRequestId |
| W4 | TP-W4-04 | **Manager Approves Cancel → Balance Restored (SF-009)** + **Balance Restore Verification** — EMP002 approve cancel → UsedDays ลดลง Remaining เพิ่ม | High (TP-W4-03 ผ่าน) | SF-009 (SCR-007), SFR-009, DB LeaveBalances + CancelRequests, IF-002 | SIT-008 + SIT-018 | EMP002 login; มี CancelRequest Pending จาก TP-W4-03 | DB: Status→Cancelled — **DEF-007 check:** UsedDays ลดลง (= ค่าก่อน TP-W3-01), Remaining เพิ่ม — Email: EMP001+EMP002+EMP003 ได้รับแจ้ง | DEF-007 risk: ถ้า balance ไม่คืน = critical defect; ISS-001 (ApproveCancelAsync ไม่มี UT) → SIT เป็น first real test |
| W4 | TP-W4-05 | **Manager Rejects Cancel → Status Reverts to Approved (SF-009)** — EMP002 reject cancel request → LeaveRequest กลับเป็น Approved | High (ต้องมี CancelRequest ใหม่ — สร้างชุดข้อมูลเพิ่ม) | SF-009, SFR-009, DB LeaveRequests | SIT-009 | EMP002 login; มี CancelRequest Pending (สร้างใหม่) | **DEF-008 check:** DB: CancelRequests.Status=Rejected, LeaveRequests.Status=Approved (restore) — LeaveBalances ไม่เปลี่ยน | DEF-008 risk: ISS-002 (RejectCancelAsync ไม่มี UT) — ถ้า status ไม่กลับ Approved = defect |

---

### Wave 5 — Report & HR Monitoring Verification
**หลักการ:** ทดสอบ HR visibility layer — ต้องมีข้อมูลหลาย status ในระบบจาก Wave 1–4  
**วิธีเตรียม:** ทำหลังจาก Wave 1–4 มีข้อมูลหลายสถานะ (Pending, Approved, Rejected, Cancelled)

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W5 | TP-W5-01 | **HR Monitoring Dashboard — All Requests Visible (SF-011)** — EMP003 (HR) เห็นคำขอทั้งองค์กรทุก status ครบ | High (ข้อมูลหลาย status จาก Wave 1–4) | SF-011 (SCR-008), SFR-011, RBAC | SIT-010 | EMP003 login; มีคำขอ ≥ 3 รายการจากหลาย status | UI: เห็นคำขอของ EMP001, EMP002, EMP003 — ไม่ถูกจำกัดแค่ subordinate — ข้อมูล: EmployeeId, ชื่อ, ประเภทลา, วันที่, Status ครบ | HR ต้องเห็นทั้งหมด ≠ Manager (เห็นแค่ทีม) |
| W5 | TP-W5-02 | **HR Monitoring — Filter Functionality** — ทดสอบ filter ตาม Status / Department / Employee Type / Leave Type | High (TP-W5-01 ผ่าน) | SF-011, SFR-011, Filter fields | SIT-010 (extend filter cases) | EMP003 login; ข้อมูลหลาย status/dept | Filter Status="Pending" → แสดงเฉพาะ Pending — Filter dept → แสดงเฉพาะ dept นั้น — Filter Employee Type=Outsource → แสดงเฉพาะ Outsource | Coverage Matrix: SFR-011 ยัง ⚠️ — ต้องครอบคลุม filter ครบ 4 criteria |
| W5 | TP-W5-03 | **Leave Balance Dashboard — Tier Entitlement Accuracy (SF-002)** — ตรวจ entitled_days ถูกต้องตาม service tier ของแต่ละพนักงาน | High (ต้องมีพนักงานหลาย tier ใน seed) | SF-002 (SCR-002), VR-008, BR-008, R6 (QA v2) | SIT-024 (เพิ่มใหม่ — ❌ ใน Coverage Matrix) | พนักงาน 3 tier (< 1yr / 1–3yr / 3–5yr) login แต่ละคน | Tier < 1yr: entitledDays=0 — Tier 1–3yr: entitledDays=10 — Tier 3–5yr: entitledDays=12 — ตรงกับตารางอายุงาน ทุก tier | Coverage Matrix: VR-004/VR-008 ยัง ❌ — ต้องเพิ่ม seed data หลาย tier |

---

### Wave 6 — Interface & API Integration
**หลักการ:** ทดสอบ interface ทั้ง 5 รายการ (IF-001–IF-005) ในมุม business impact — บาง interface ทดสอบแยกได้ บางอย่างต้องการ state จาก Wave ก่อน  
**หมายเหตุ:** IF-001 (HRIS Sync) และ IF-005 (SLA Timer) ไม่อยู่ใน SIT scope ตาม std-sit.md §7.2

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W6 | TP-W6-01 | **Outsource Excel Import — Valid File (SF-012, IF-003)** — HR upload Excel template ที่ถูกต้อง 3 rows → import สำเร็จ + Outsource accounts ใช้งานได้ | Low | SF-012 (SCR-009), IF-003, SFR-012, VR-013, BR-020 | SIT-021 (เพิ่มใหม่ — ❌ ใน Coverage Matrix) | EMP003 (HR) login; Excel template ที่กรอกข้อมูลครบ 3 rows | SUC-IMP-001: "นำเข้าข้อมูลสำเร็จ 3 รายการ" — DB: Employees + LeaveBalances สร้างถูกต้อง — Outsource accounts login ได้ | ต้องเตรียม Excel template ตาม IF-003 spec (8 columns) |
| W6 | TP-W6-02 | **Outsource Excel Import — Invalid Rows Partial Import (SF-012, IF-003, VR-013)** — HR upload Excel ที่มี row ผิด (email ซ้ำ, Manager ID ไม่มีในระบบ, field ว่าง) → partial import + error report | Low | SF-012, IF-003, VR-013, ERR-IMP-001–004 | SIT-022 (เพิ่มใหม่ — ❌ ใน Coverage Matrix) | EMP003 (HR) login; Excel มีทั้ง valid rows และ invalid rows | Valid rows import สำเร็จ — Invalid rows: error report ระบุ row/field/reason ชัดเจน — ไม่ import row ที่ผิด | Coverage Matrix: SFR-012 + VR-013 ยัง ❌ — นี่คือ first SIT coverage |
| W6 | TP-W6-03 | **Medical Certificate Upload (SF-003, IF-004)** — ลาป่วย ≥ 3 วัน: upload ไฟล์ PDF → link กับ Leave Request + Manager/HR เข้าถึงได้ | Low (ใช้ employee account ใหม่ หรือรัน independent) | SF-003, IF-004, SIR-005, VR-007, TR-005 | SIT-002 (extend: ลาป่วย ≥ 3 วัน + แนบไฟล์) | EMP001 login | Upload PDF ≤ limit: success → ไฟล์ link กับ leaveRequestId — UploadError ถ้า file type ไม่รองรับ: ERR-IF004-001 แสดง | IF-004 Open Issue: max file size ยังไม่ยืนยัน — test ด้วยไฟล์ขนาดปกติก่อน |
| W6 | TP-W6-04 | **Email Notification Delivery Verification (IF-002, RP-003 preview)** — ตรวจ notification_log ว่า Email ถูกส่งสำหรับทุก event ที่ผ่านมาใน Wave 2–4 (Submit, Approve, Reject, Cancel) | Medium (ต้องมี event จาก Wave 2–4) | IF-002, SFR-013, BR-019, DB NotificationLogs | ตรวจ delivery log หลัง Wave 2–4 | notification_log มี records จาก Wave 2–4 | ทุก event: delivery_status=Success (หรือ Retry ≤ 3 ครั้ง) — Recipients ถูกต้องตาม Notification Events Matrix — RP-003 format: success rate ≥ 99% | RP-003 เป็น Phase 2 — ในขั้นนี้ตรวจ DB โดยตรง ไม่ผ่าน UI report |

---

### Wave 7 — End-to-End Cross-Component Verification
**หลักการ:** ทดสอบ business scenario สมบูรณ์แบบที่เชื่อม Screen + DB + Notification + Balance — เหมาะสำหรับ regression test  
**วิธีเตรียม:** ต้องการ clean state — reset DB และ seed data ใหม่, รันต่อจาก Wave 6 หรือ environment ใหม่

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W7 | TP-W7-01 | **Full Happy Path Chain** — EMP001 Submit Annual → EMP002 Approve → EMP001 ตรวจ balance → EMP001 Cancel Approved → EMP002 Approve Cancel → ตรวจ balance restore → EMP003 ดูใน HR Dashboard | Very High (Waves 1–6 ผ่าน) | SF-001–009, SF-011, IF-002, DB ทุก table | SIT-001 → SIT-004 → SIT-017 → SIT-007 → SIT-008 → SIT-018 → SIT-010 | Clean seed data; Waves 1–6 ผ่านทั้งหมด; EMP001/EMP002/EMP003 พร้อม | Balance lifecycle สมบูรณ์: +3 PendingDays → 0 Pending+3 Used → 0 UsedDelta restore → HR เห็น Cancelled status — DEF-001/004/007 ผ่านพร้อมกัน | เป็น acceptance test ระดับ business; ถ้า fail ต้องแยก defect ว่า Wave ใด |
| W7 | TP-W7-02 | **Rejection Path + Resubmit Chain** — EMP001 Submit → EMP002 Reject (พร้อม reason) → EMP001 เห็น rejection reason + resubmit ใหม่ → EMP002 Approve ครั้งที่ 2 → ตรวจ balance | Very High (Waves 1–6 ผ่าน) | SF-003–006, SF-002, IF-002, LeaveHistory | SIT-001 → SIT-005 → SIT-001 (resubmit) → SIT-004 → SIT-017 | ต่อจาก TP-W7-01 หรือ clean state ใหม่ | Rejection reason แสดงชัดเจน (DEF-006) — Resubmit สำเร็จ (ไม่ต้องยกเลิกก่อน per BR-014) — Approve ครั้งที่ 2: balance deduct ถูกต้อง (DEF-001) | ตรวจว่า "ยื่นใหม่" หลัง Reject ไม่ต้อง Cancel ก่อน — กรณีนี้มักเป็น edge case ที่ UI ซ่อนปุ่ม Submit |

---

## 3. Scenarios ที่ยังขาด (❌ ใน Coverage Matrix) — ต้องสร้างก่อน Execute

| Plan ID | Scenario ที่ต้องเพิ่ม | FR ที่ครอบคลุม | Wave | Priority | หมายเหตุ |
|---------|---------------------|--------------|------|---------|---------|
| TP-W1-07 | **SIT-023**: Probation employee ยื่น Annual Leave → ERR-LR-003 block | VR-003, BR-007 | W1 | **High** | ISS-004: service ยังไม่ implement VR-003 |
| TP-W5-03 | **SIT-024**: Employee หลาย tier → EntitledDays ถูกตาม table | VR-004, BR-008 | W5 | Medium | ต้องเพิ่ม seed data หลาย tier |
| TP-W6-01 | **SIT-021**: HR upload valid Excel → import success | SFR-012, IF-003 | W6 | Medium | |
| TP-W6-02 | **SIT-022**: HR upload Excel ผิด row → partial import | SFR-012, VR-013 | W6 | Medium | |
| TP-W1-06 | **SIT-025** (รวมใน TP-W1-06): Annual Leave < advance notice → ERR-LR-005 | VR-005, BR-003 | W1 | **High** | ISS-004: VR-005 ยังไม่ implement ใน service |

---

## 4. UT Issues ที่กระทบ SIT — ต้องติดตามก่อน Execute

| Issue ID | รายละเอียด | กระทบ Wave | Priority | สถานะ |
|----------|-----------|-----------|---------|-------|
| ISS-001 | ApproveCancelAsync ไม่มี UT เลย — W4 TP-W4-04 เป็น first real test | W4 | P1 | Carry forward — ต้องเพิ่ม UT ก่อน W4 |
| ISS-002 | RejectCancelAsync ไม่มี UT เลย — W4 TP-W4-05 เป็น first real test | W4 | P1 | Carry forward — ต้องเพิ่ม UT ก่อน W4 |
| ISS-003 | Conflict: TC-SPEC-020 vs TC-KB-020 (Approved → throw หรือสร้าง CancelRequest?) — กระทบ SIT-007 behavior | W4 | P1 | Resolve กับ PO ก่อน execute W4 |
| ISS-004 | VR-003~007 ยังไม่ implement ใน service — TP-W1-06/07 จะ fail ถ้า service ไม่ block | W1 | P2 | Fix ใน service → unskip UT → rerun → SIT |
| ISS-005 | INotificationService ไม่ inject → Email notification event อาจไม่ถูกส่ง | W6 (TP-W6-04) | P2 | Fix → unskip TC-SPEC-015/026/032 → SIT |

---

## 5. Risk Summary

| Risk | Wave ที่กระทบ | Severity | Mitigation |
|------|-------------|---------|-----------|
| DEF-001: Balance ไม่ deduct หลัง Approve | W3 TP-W3-01, TP-W3-02 | Critical | ตรวจ DB ทันทีหลัง approve + ตรวจ UI balance |
| DEF-002: Manager Queue ว่างเมื่อ ManagerId=null | W2 TP-W2-04 | High | ตรวจ seed ว่า EMP003.ManagerId=EMP002 ก่อนรัน |
| DEF-003: Spinner ค้างแทน error | W1 TP-W1-01, W2 TP-W2-02 | Medium | ตรวจ Network tab ว่า API response ถูกต้อง |
| DEF-004: CancelRequest ไม่ INSERT | W4 TP-W4-03 | High | Query CancelRequests ทันทีหลัง cancel approved leave |
| DEF-005: Cancel ซ้ำได้ | W4 TP-W4-02 | Medium | API bypass test เพิ่มเติม |
| DEF-006: Rejection reason ไม่แสดง | W3 TP-W3-03 | Medium | ตรวจ UI Leave History + API response fields |
| DEF-007: Balance ไม่คืนหลัง Approve Cancel | W4 TP-W4-04 | High | Query DB ก่อน/หลัง approve cancel |
| DEF-008: Status ไม่กลับ Approved หลัง Reject Cancel | W4 TP-W4-05 | High | ตรวจ LeaveRequests.Status ทันทีหลัง reject cancel |
| ISS-001/002: ApproveCancelAsync/RejectCancelAsync ไม่มี UT | W4 ทั้ง Wave | High | ต้องเพิ่ม UT ก่อน execute W4 |
| ISS-004: VR-003–007 ไม่ implement | W1 TP-W1-06/07 | High | พร้อม fail → แจ้งทีม dev implement ก่อน rerun |

---

## 6. Execution Order Summary

```
Wave 1 (Validation) ──────────────────────────────────────── รันแบบ Parallel ได้ทุก Plan
     │
     ▼
Wave 2 (Submit) ──────────────────────────────────────────── Seed data reset → รัน Sequential
     │
     ├─→ TP-W2-01 [EMP001 Submit Annual] ──────────────────── ต้องผ่านก่อนถึง Wave 3
     ├─→ TP-W2-02 [Balance insufficient] ─────────────────── Independent (ใช้ seed แยก)
     ├─→ TP-W2-03 [Self-approve API test] ────────────────── Independent
     └─→ TP-W2-04 [HR submit → Manager queue] ─────────────── Independent
     │
     ▼
Wave 3 (Approve/Reject) ─────── depends on TP-W2-01 PASS ─── รัน Sequential
     │
     ├─→ TP-W3-01 [Manager Approves] ──────────────────────── บันทึก leaveRequestId(Approved)
     ├─→ TP-W3-02 [Balance verify] ────── after TP-W3-01 ────
     └─→ TP-W3-03 [Manager Rejects + Employee sees reason] ─── สร้าง Pending ใหม่
     │
     ▼
Wave 4 (Cancel) ──────────────── depends on Wave 3 ─────────── รัน Sequential
     │
     ├─→ TP-W4-01 [Cancel Pending] ────── ใช้ Pending ที่สร้างใน TP-W3-03
     ├─→ TP-W4-02 [Double-cancel] ─────── after TP-W4-01
     ├─→ TP-W4-03 [Cancel Approved] ───── ใช้ Approved จาก TP-W3-01
     ├─→ TP-W4-04 [Approve Cancel] ─────── after TP-W4-03
     └─→ TP-W4-05 [Reject Cancel] ─────── สร้าง CancelRequest ใหม่
     │
     ▼
Wave 5 (Report/HR) ──────────── depends on data from W1-W4 ─── HR view & filter
Wave 6 (Interface) ──────────── mostly independent ──────────── Excel/Email/File
     │
     ▼
Wave 7 (E2E) ──────────────────── depends on W1-W6 PASS ────── Full regression
```

---

## 7. Test Environment Requirements

| รายการ | Required Value | หมายเหตุ |
|--------|--------------|---------|
| Base URL | `http://localhost:5173` | verify HTTP 200 ก่อนรัน |
| Backend API | `http://localhost:5000` (หรือ proxy) | |
| Auth method | Browser login (email/password) | somchai@abc.com/1234 (EMP001), wipa@abc.com/1234 (EMP002), nanta@abc.com/1234 (EMP003) |
| Browser | Chrome Desktop (ตาม std-sit.md §7.2) | |
| Evidence folder | `30-coding/c2-sit/evidence/{SIT-xxx}/` | naming: `evidence-{scenario}-{step}-{pass|fail}.png` |
| Seed data file | `30-coding/c2-sit/sit-test-data-leave-request.yaml` | ต้องเพิ่ม Outsource + Probation + Multi-tier accounts |

---

*Test Plan สร้างโดย AI-assisted (Claude) — อ้างอิงจาก Screen SRS v1.1, Report SRS v1.0, Interface SRS v1.0, SIT Scenario v1.0, Coverage Matrix v1.0, Unit Test Issue Summary, std-sit.md, lesson-learned-sit-defects.md*
