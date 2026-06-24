# SIT Coverage Matrix — Leave Request and Approval

**Module:** Leave Request and Approval  
**วันที่สร้าง:** 2026-06-16  
**Source:** `10-requirement-definition/b0-system-requriement/leave-request-and-approval-screen-srs.md` + `30-coding/c2-sit/sit-scenario-leave-request.md`  
**Version:** 1.0

### สัญลักษณ์

| สัญลักษณ์ | ความหมาย |
|----------|---------|
| ✅ | มี scenario ครอบคลุมแล้ว |
| ⚠️ | มี scenario แต่ครอบคลุมบางส่วน — ควรเพิ่ม step หรือ scenario เสริม |
| ❌ | ยังไม่มี scenario — ต้องเพิ่ม |
| N/A | ไม่อยู่ใน SIT scope Phase 1 (Phase 2 หรือ out-of-scope ตาม std-sit.md §7.2) |

---

## 1. Coverage Table — System Functional Requirements (SFR)

| FR ID | Requirement | Scenario ID | Scenario Name | Coverage | หมายเหตุ |
|---|---|---|---|---|---|
| SFR-001 | Login / Authentication ด้วย SSO องค์กร (SF-001) | — | — | N/A | ใช้ Mock Auth (`X-Employee-Id` header) แทน SSO จริงใน SIT — SSO ไม่อยู่ใน scope (std-sit.md §7.2) |
| SFR-002 | Employee ดูสรุปวันลาคงเหลือทุกประเภทได้ — Leave Balance Dashboard (SF-002) | SIT-017, SIT-018 | Balance accuracy immediately after approve / Balance restored after approve cancel | ⚠️ | ครอบคลุมเฉพาะ post-action state verification; ยังไม่มี scenario ทดสอบ dashboard standalone (initial load, ทุกประเภทลาแสดงครบ) |
| SFR-003 | Employee ยื่นคำขอลาได้ 7 ประเภท พร้อม validation ครบ (SF-003) | SIT-001, SIT-002, SIT-003, SIT-012, SIT-013, SIT-019 | Submit Annual / Sick / Business / Balance-insufficient blocked / Required-fields blocked / Half-day | ✅ | ครอบคลุม Happy Path 3 ประเภทหลัก + validation flows + half-day; 4 ประเภท Regular-only (Maternity ฯลฯ) ครอบคลุมผ่าน VR-001 (SIT-020) |
| SFR-004 | Manager เห็น Approval Inbox รายการคำขอของ subordinate ทั้งหมด (SF-004) | SIT-004, SIT-011 | Manager approves leave request / HR submits → appears in Manager queue | ✅ | SIT-011 ครอบคลุม DEF-002 (ManagerId = null → queue ว่าง) |
| SFR-005 | Manager Approve / Reject คำขอลาพร้อมเหตุผล (SF-005) | SIT-004, SIT-005, SIT-014, SIT-015 | Approve + balance deducted / Reject with reason (Employee sees it) / Reject blocked w/o reason / Self-approve forbidden | ✅ | ครอบคลุมทุก path รวม authorization guard (VR-010) และ DEF-006 (rejection reason visible) |
| SFR-006 | Employee ติดตาม Status คำขอลาของตัวเองได้ รวมเหตุผลปฏิเสธ (SF-006) | SIT-005, SIT-006 | Rejection reason visible to Employee / Cancel Pending → status Cancelled | ⚠️ | SIT-005 step 6–7 ตรวจ rejection reason ในหน้า Leave History; ยังไม่มี scenario ทดสอบ Status Tracking flow โดยตรงตั้งแต่ต้นจนจบ |
| SFR-007 | Employee ยกเลิกคำขอ Pending → Cancelled ทันที ไม่ต้องรอ Manager (SF-007) | SIT-006, SIT-016 | Employee cancels Pending leave immediately / System prevents double-cancel | ✅ | SIT-016 ครอบคลุม DEF-005 (cancel ซ้ำถูก block) |
| SFR-008 | Employee ยกเลิกคำขอ Approved → Status = CancelRequested + INSERT CancelRequest row (SF-008) | SIT-007 | Employee requests cancel on Approved leave (creates CancelRequest) | ✅ | SIT-007 ตรวจทั้ง Status change + DEF-004 (CancelRequest INSERT) + Manager cancel queue แสดงรายการ |
| SFR-009 | Manager Approve Cancel Request → balance คืน / Manager Reject Cancel → status กลับ Approved (SF-009) | SIT-008, SIT-009 | Manager approves cancel (balance restored) / Manager rejects cancel (status reverts to Approved) | ✅ | ครอบคลุม DEF-007 (balance ไม่คืน) + DEF-008 (status ไม่กลับ Approved) |
| SFR-010 | Background SLA: Reminder Email 4h ก่อนหมด + Escalate ไป HR เมื่อหมด SLA (SF-010) | — | — | N/A | Background scheduled job — ไม่อยู่ใน SIT scope (std-sit.md §7.2 "SLA escalate: Low") |
| SFR-011 | HR ดูรายการคำขอลาทั้งองค์กร กรองตามหลายเกณฑ์ได้ (SF-011) | SIT-010, SIT-011 | HR views all leave requests in monitoring dashboard / HR submits leave appears in Manager queue | ⚠️ | SIT-010 ครอบคลุม basic view + filter Status/Department; ยังไม่มี scenario ทดสอบ filter ครบทุก criteria (Employee Type / Leave Type / Date Range) |
| SFR-012 | HR Import ข้อมูล Outsource Employee จาก Excel template 8 fields (SF-012) | — | — | ❌ | ยังไม่มี scenario — ควรเพิ่ม **SIT-021**: HR uploads valid Excel → import success + record ใน DB; **SIT-022**: HR uploads Excel with invalid rows → partial import + error report per row |
| SFR-013 | Notification Log View — HR ตรวจ log Email ทั้งหมด (SF-015) | — | — | N/A | Phase 2 — รายละเอียดจะกำหนดในรอบถัดไป |
| SFR-014 | Leave History & Audit Trail — แสดงทุก action พร้อม timestamp (SF-013) | — | — | N/A | Phase 2 — รายละเอียดจะกำหนดในรอบถัดไป |
| SFR-015 | Leave Report Export — HR export รายงานเป็นไฟล์ (SF-014) | — | — | N/A | Phase 2 — Report template ยังไม่ยืนยัน (SRS §7 Open Issue) |

---

## 2. Coverage Table — Validation Rules (VR)

| FR ID | Requirement | Scenario ID | Scenario Name | Coverage | หมายเหตุ |
|---|---|---|---|---|---|
| VR-001 | Outsource ห้ามยื่นลา 4 ประเภท: คลอดบุตร / ทำหมัน / รับราชการทหาร / อุปสมบท | SIT-020 | Outsource employee cannot submit restricted leave type | ✅ | ทดสอบทั้ง UI (dropdown ซ่อน 4 ประเภท) + API bypass → HTTP 422 VR-001 |
| VR-002 | วันลาคงเหลือต้องเพียงพอก่อน Submit — ห้ามยื่นเกิน Remaining balance | SIT-012 | System prevents submit when Annual Leave balance is insufficient | ✅ | ตรวจ UI แสดง error ชัดเจน (ไม่ใช่ spinner ค้าง — DEF-003) + DB ไม่สร้าง record |
| VR-003 | ห้ามยื่นลาพักผ่อนในช่วงทดลองงาน (Probation Period) | — | — | ❌ | ยังไม่มี scenario — ควรเพิ่ม **SIT-023**: Probation employee submits Annual Leave → ระบบ block พร้อม error message |
| VR-004 | สิทธิ์วันลาประจำปีตามระดับอายุงาน (service tier: < 1yr / 1–3yr / 3yr+) | — | — | ❌ | ยังไม่มี scenario — ควรเพิ่ม **SIT-024**: Employee แต่ละ tier มี EntitledDays ถูกต้องตาม tier rule ใน Balance Dashboard |
| VR-005 | ลาพักผ่อนต้องยื่นล่วงหน้าตามจำนวนวันที่กำหนด (advance notice) | — | — | ❌ | ยังไม่มี scenario — ควรเพิ่ม **SIT-025**: Employee ยื่น Annual Leave โดยไม่ครบ advance notice → ระบบ block |
| VR-006 | ลากิจส่วนตัวต้องยื่นล่วงหน้า ≥ 3 วันทำการ | SIT-003 | Employee submits Business Leave request successfully (≥ 3 days advance) | ⚠️ | SIT-003 ครอบคลุม happy path ที่ยื่น ≥ 3 วัน; ยังไม่มี scenario ทดสอบกรณียื่น < 3 วันทำการ → ระบบ block |
| VR-007 | ลาป่วย > 2 วัน ต้องแนบใบรับรองแพทย์ — ระบบแสดง field `medical_certificate` | SIT-002 | Employee submits Sick Leave ≤ 2 days (no certificate required) | ⚠️ | SIT-002 ครอบคลุม ≤ 2 วัน (field ซ่อน); ยังไม่มี scenario ทดสอบ > 2 วัน (field แสดง + ต้องแนบไฟล์ก่อน Submit) |
| VR-008 | Leave Balance Dashboard แสดงยอดถูกต้องตาม calculation rules (EntitledDays cap 30, tier, carry-forward lakat 3 วัน) | SIT-017, SIT-018 | Balance accuracy immediately after approve / Balance restored after approve cancel | ⚠️ | ครอบคลุม post-action calculation; ยังไม่มี scenario ตรวจ initial balance display + cap 30 วัน + lakat rule |
| VR-009 | Employee ยกเลิกได้เฉพาะคำขอลาของตัวเองเท่านั้น | SIT-006, SIT-007 | Employee cancels Pending / Employee cancels Approved | ✅ | ทุก scenario ใช้ EMP001 cancel own request; authorization ถูกต้องตาม design |
| VR-010 | Manager ไม่สามารถ Approve คำขอลาของตัวเองได้ (self-approve forbidden) | SIT-015 | Manager cannot approve own leave request (Unauthorized) | ✅ | ทดสอบผ่าน API direct call (X-Employee-Id: EMP002) + ตรวจ UI ซ่อนปุ่ม Approve |
| VR-011 | ลาครึ่งวัน: IsHalfDay = true, DurationDays = 0.5, ต้องระบุ AM/PM | SIT-019 | Half-day leave submit and balance deducted as 0.5 days | ✅ | ตรวจทั้ง UI (field half_day_period แสดง/ซ่อน) + DB (DurationDays = 0.5) + Balance PendingDays +0.5 |
| VR-012 | SLA หมด → ปุ่ม Approve/Reject Cancel disabled + แสดงสถานะ "Escalated ไปยัง HR" | — | — | N/A | SLA background behavior — ไม่อยู่ใน SIT scope (std-sit.md §7.2) |
| VR-013 | Import Excel: 7 required fields ต้องครบถ้วน — validate ก่อน import, partial import valid rows | — | — | ❌ | ตาม SFR-012 — ยังไม่มี scenario; ควรเพิ่มใน SIT-022 (import with error rows) |

---

## 3. Coverage Summary

### 3.1 สรุปตัวเลข

| รายการ | SFR | VR | รวม |
|--------|-----|-----|-----|
| FR ทั้งหมด | 15 | 13 | **28** |
| N/A (Phase 2 + out-of-scope) | 5 | 1 | **6** |
| **Testable FR** | **10** | **12** | **22** |
| ✅ ครอบคลุมแล้ว | 6 (60%) | 5 (42%) | **11 (50%)** |
| ⚠️ ครอบคลุมบางส่วน | 3 (30%) | 3 (25%) | **6 (27%)** |
| ❌ ยังไม่มี scenario | 1 (10%) | 4 (33%) | **5 (23%)** |

### 3.2 ประเมิน Exit Criteria

| เกณฑ์ (std-sit.md §3) | สถานะ | รายละเอียด |
|----------------------|-------|-----------|
| High priority scenarios (SIT-001–007, SIT-011–014, SIT-017) ครอบคลุม FR High ✅ | **ผ่าน** | SFR-003–009 ทั้งหมดมี High scenario ครอบคลุม |
| Medium scenarios ≥ 90% pass rate | ขึ้นอยู่กับ execution | SIT-008, 009, 010, 015–016, 018–020 |
| ไม่มี Critical defect ค้าง | ยังไม่ทราบ | ต้องรอผล execution |

### 3.3 FR ที่ยังไม่มี scenario (❌) — ต้องเพิ่มก่อน SIT

| FR ID | Scenario ที่ควรเพิ่ม | Priority |
|-------|---------------------|---------|
| SFR-012 | SIT-021: HR uploads valid Excel → import X records success<br>SIT-022: HR uploads Excel with invalid rows → partial import + error report | Medium |
| VR-003 | SIT-023: Probation employee submits Annual Leave → blocked (error message แสดง) | High |
| VR-004 | SIT-024: Employee tier < 1yr มี EntitledDays ถูกต้อง ≠ tier 3yr+ | Medium |
| VR-005 | SIT-025: Annual Leave ยื่นโดยไม่ครบ advance notice → ระบบ block | High |
| VR-013 | รวมใน SIT-022 (import validation per row) | Medium |

### 3.4 FR ที่ครอบคลุมบางส่วน (⚠️) — ควรเสริม step

| FR ID | สิ่งที่ขาด |
|-------|-----------|
| SFR-002 | Standalone dashboard test: initial load แสดงทุกประเภทลา, ยอดถูกต้องก่อน action ใดๆ |
| SFR-006 | Dedicated status tracking flow: Employee เปิดหน้า Leave History → เห็นทุกสถานะครบ |
| SFR-011 | Filter ครบ: Employee Type / Leave Type / Date Range filter ทำงานถูกต้อง |
| VR-006 | Error case: Business Leave ยื่น < 3 วันทำการ → ระบบ block พร้อม error message |
| VR-007 | Sick Leave > 2 วัน: field `medical_certificate` แสดง + Submit ไม่ได้ถ้าไม่แนบ |
| VR-008 | Balance dashboard: cap 30 วัน, lakat 3 วัน, tier entitlement แสดงถูกต้อง |

---

**สรุป:** ⚠️ **ยังไม่พร้อมสำหรับ SIT เต็มรูปแบบ** — FR ที่ testable ครอบคลุมแล้ว 50% (11/22); มี 5 FR ที่ยังไม่มี scenario เลย (SFR-012, VR-003, VR-004, VR-005, VR-013) ซึ่งรวมถึง Probation guard (VR-003) และ Annual advance notice (VR-005) ที่เป็น High business risk

แนะนำ: เพิ่ม scenario SIT-021 ถึง SIT-025 ก่อนเริ่ม SIT execution เพื่อให้ coverage ✅ ≥ 70%

---

## 4. หมายเหตุ

> **Interface / Integration Requirements (SIR-*, IF-*) ไม่อยู่ใน matrix นี้**  
> Coverage matrix นี้ครอบคลุมเฉพาะ SFR และ VR (business + validation)  
> Requirements ประเภท System Integration (`SIR-003`, `SIR-004`) และ Interface (`IF-003`, `IF-005`) ที่ปรากฏใน SRS จะถูกครอบคลุมใน **Interface SRS document** แยกต่างหาก  
> ซึ่งจะสร้างโดยใช้ `.claude/agents/interface-srs-document-agent.md`  
> Output: `10-requirement-definition/b0-system-requirement/leave-request-and-approval-interface-srs.md`
