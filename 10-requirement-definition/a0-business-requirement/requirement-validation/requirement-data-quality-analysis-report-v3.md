# Requirement Data Quality Analysis Report — v3

> **โปรเจกต์:** ระบบบริหารการลาและการอนุมัติ — ABC Company
> **วิเคราะห์โดย:** Requirement Data Quality Analysis Agent
> **เวอร์ชัน:** v3 (อัปเดตหลังได้รับคำตอบจาก HR Manager ครบทุกรายการ)
> **อ้างอิง v2:** `requirement-data-quality-analysis-report-v2.md`
> **แหล่งคำตอบ v2:** `requirement-data-quality-analysis-qa-list-v2.yaml` (6/6 Closed)

---

## 1. Executive Summary

- **จำนวนไฟล์ที่วิเคราะห์:** 7 ไฟล์ (raw-extracted 5 ไฟล์ + QA v1 answers + QA v2 answers)
- **ภาพรวม:** หลังได้รับคำตอบจาก HR Manager ครบทั้ง 6 รายการ (R1–R6) ข้อมูลทุกประเด็นสำคัญได้รับการยืนยันและมีความสมบูรณ์เพียงพอสำหรับการออกแบบ System Requirement ครบถ้วน ทั้ง core process, business rules, data model, leave quota, cancel/edit flow และ notification design ยังพบประเด็น minor design decisions 3 รายการที่สามารถตัดสินใจได้ระหว่างขั้นตอน design โดยไม่ blocking การเริ่มต้นเขียน requirement
- **ประเด็นขัดแย้งคงเหลือ:** 0 รายการ (ทุก conflict ได้รับการ resolve แล้ว)
- **ประเด็นไม่ชัดเจนคงเหลือ:** 3 รายการ (Minor — ระดับ design decision)
- **ประเด็นใหม่ที่พบจากคำตอบ v2:** 3 รายการ (เป็น requirement เพิ่มเติมที่ต้องรวมใน design)
- **ระดับความพร้อม:** `Ready`
- **สรุปความเห็น:** ข้อมูลพร้อมสำหรับการเริ่มจัดทำ System Requirement ได้ทันที ประเด็น minor ที่เหลือ 3 รายการสามารถตัดสินใจได้โดยทีม BA/SA ระหว่าง design session โดยไม่จำเป็นต้องรอคำตอบเพิ่มเติมจาก Business Owner หรือ HR Manager อีก

---

## 2. Source Files Reviewed

| ลำดับ | ชื่อไฟล์ | ประเภท | บทบาทใน v3 |
|-------|---------|--------|------------|
| 1 | `ABC-Company-Leave-Form.md` | `.md` | ข้อมูลต้นทาง — แบบฟอร์มการยื่นใบลา |
| 2 | `ABC-Compay-HR-Regularion.md` | `.md` | ข้อมูลต้นทาง — ระเบียบ HR 17 หมวด |
| 3 | `ABC-Leave-Workflow-Thai-QA.md` | `.md` | ข้อมูลต้นทาง — บทสนทนา HR Q&A |
| 4 | `Leave-Management-Request-and-Approval-Business-User.yaml` | `.yaml` | ข้อมูลต้นทาง — Business Requirements หลัก |
| 5 | `ระบบบริหารการลาและการอนุมัติ.md` | `.md` | ข้อมูลต้นทาง — Presentation สรุป |
| 6 | `requirement-data-quality-analysis-qa-list.yaml` | `.yaml` | คำตอบ v1 จาก Business User (12/12 Closed) |
| 7 | `requirement-data-quality-analysis-qa-list-v2.yaml` | `.yaml` | **ใหม่ใน v3** — คำตอบ v2 จาก HR Manager (6/6 Closed) |

---

## 3. สถานะการ Resolve ประเด็นทั้งหมด (v1 + v2)

### 3.1 สรุป Resolution Status รวม 3 รอบ

| รอบ | จำนวน QA | Fully Resolved | Partially Resolved | Closed ใน v3 |
|-----|----------|----------------|-------------------|--------------|
| v1 (Business User) | 12 | 6 | 6 | ✅ ทั้งหมด |
| v2 (HR Manager) | 6 | 6 | 0 | ✅ ทั้งหมด |
| **รวม** | **18** | **18** | **0** | **✅ 18/18** |

### 3.2 สรุปคำตอบสำคัญจาก HR Manager (v2 Closed)

| ID | ประเด็น | คำตอบสรุป |
|----|---------|-----------|
| R1 | สิทธิ์วันลากิจ | **3 วัน/ปี** — ทั้งพนักงานประจำและ Outsource เท่ากัน |
| R2 | สิทธิ์วันลา Outsource | ลาป่วย/กิจ/พักผ่อน: เหมือนพนักงานประจำ — ลาคลอด/ทำหมัน/รับราชการ/อุปสมบท: **ไม่มีสิทธิ์** (ใช้จากบริษัทต้นสังกัด) |
| R3 | Onboard Outsource | Import จาก **Excel template** — update ทุกต้นไตรมาส (7 fields บังคับ) |
| R4 | Cancel/Edit rule | Pending: ยกเลิกเองได้ทันที — Approved: หัวหน้า re-approve ภายใน **1 วันทำการ** — Rejected: ยื่นใหม่ได้ทันที — **คืนวันลา auto เมื่อยกเลิก Approved** |
| R5 | HR Email scope | HR รับ **ทุก event** (ยื่น, Approve, Reject) — เพิ่ม **event ใหม่**: ยกเลิก Approved → แจ้ง HR ด้วย |
| R6 | ตารางสะสมวันลา | < 1 ปี = ไม่มีสิทธิ์, 1–3 ปี = 10 วัน, 3–5 ปี = 12 วัน, 5–10 ปี = 15 วัน, 10+ ปี = 18 วัน — **cap 30 วัน** |

---

## 4. Duplicated Information

> ประเด็นซ้ำซ้อนในเอกสารต้นทางไม่ส่งผลต่อความพร้อมของ requirement เนื่องจากทุก conflict ได้รับการ resolve แล้ว

| ID | ประเด็น | ไฟล์ที่เกี่ยวข้อง | หมายเหตุ v3 |
|----|---------|------------------|------------|
| D1 | ประเภทการลา | Leave-Form.md, HR-Regulation.md, QA.md, YAML, PPTX.md | Resolved (C1) — ใช้รายการ 7 ประเภทจาก Business User เป็น canonical |
| D2 | ใบรับรองแพทย์ลาป่วย > 3 วัน | QA.md, PPTX.md | สอดคล้องกัน ยืนยันแล้ว |
| D3 | Workflow การยื่นลา | QA.md, YAML, PPTX.md | Resolved (C3) — ใช้ YAML workflow (to-be) เป็น reference หลัก |

---

## 5. Conflicting Information

> ไม่มีประเด็นขัดแย้งคงเหลือใน v3 — ทุก conflict ได้รับการ resolve ครบถ้วนแล้ว

| ID | ประเด็น | สถานะ v3 | หมายเหตุ |
|----|---------|----------|---------|
| C1 | รายการประเภทการลา | ✅ Resolved (v1) | 7 ประเภท ยืนยันโดย Business User |
| C2 | ระยะเวลาแจ้งล่วงหน้าลาพักร้อน | ✅ Resolved (v1) | 1 วัน ยืนยันโดย Business User |
| C3 | สถานะ HRIS ปัจจุบัน | ✅ Resolved (v1) | Web App แยก integrate กับ HRIS เดิม |
| C4 | ข้อมูลลากิจ 45 วัน (ข้าราชการ) | ✅ Resolved (v2) | ยืนยันว่าผิดพลาด — สิทธิ์จริง = 3 วัน/ปี (R1) |

---

## 6. Ambiguous or Incomplete Information (Minor — v3)

> ประเด็นต่อไปนี้เป็น design decisions ที่ทีม BA/SA สามารถตัดสินใจได้โดยไม่ต้องรอ Business Owner หรือ HR Manager

| ID | ประเด็น | ไฟล์ต้นทาง | สิ่งที่ยังต้องกำหนด | ข้อเสนอแนะเบื้องต้น | ผลกระทบ |
|----|---------|------------|---------------------|---------------------|---------|
| M1 | **สถานะ "Cancelled"** ในระบบ | QA-list-v2 (R4) | Cancel flow เพิ่ม status ใหม่ "Cancelled" หรือใช้ชื่ออื่น เช่น "Withdrawn" | ใช้ "Cancelled" เพื่อความชัดเจน แยกจาก Rejected | ต่ำ — กระทบ status enum ในระบบ |
| M2 | **ระยะเวลาช่วงทดลองงาน** สำหรับ leave eligibility | QA-list-v2 (R6) | R6 ระบุว่า < 1 ปี ไม่มีสิทธิ์ลาพักผ่อน แต่ไม่ระบุระยะทดลองงาน (โดยทั่วไป 3 เดือน) | ระบบควร check probation period จาก HRIS / hire_date | ต่ำ — กระทบ leave eligibility validation |
| M3 | **SLA Re-approve** เมื่อครบ 1 วันทำการแล้วไม่มีการ action | QA-list-v2 (R4) | R4 กำหนดว่าหัวหน้าต้อง re-approve ภายใน 1 วันทำการ แต่ไม่ระบุว่าถ้าหมดเวลาแล้วเกิดอะไร (auto-approve? escalate? แจ้งเตือนซ้ำ?) | แนะนำ: ส่ง reminder ก่อนครบกำหนด และ escalate ไปยัง HR หากหมดเวลา | ต่ำ–กลาง — กระทบ SLA workflow design |

---

## 7. Requirements ใหม่ที่ได้จากคำตอบ v2

> ประเด็นต่อไปนี้เป็น **requirement ใหม่** ที่ถูกเพิ่มขึ้นจากคำตอบของ HR Manager ซึ่งไม่ปรากฏในเอกสารต้นทางเดิม ต้องนำไปรวมใน Business Requirement ก่อน sign-off

| ID | Requirement ใหม่ | แหล่งที่มา | BR ที่ต้องเพิ่ม |
|----|-----------------|-----------|----------------|
| NR1 | **ระบบต้องคืนวันลาคงเหลือโดยอัตโนมัติ** เมื่อคำขอที่ Approved ถูกยกเลิกสำเร็จ | R4 (HR Manager) | ต้องเพิ่ม BR ใหม่: Leave Balance Auto-Restore |
| NR2 | **เพิ่ม Notification Event: ยกเลิก Approved** → ส่ง Email แจ้งพนักงาน + หัวหน้างาน + HR | R5 (HR Manager) | BR-005 ต้องอัปเดต: เพิ่ม event "Cancellation Approved" |
| NR3 | **Outsource ต้องมี employee_type** ในระบบเพื่อ control สิทธิ์ลาบางประเภท (ลาคลอด/ทำหมัน/รับราชการ/อุปสมบท = ไม่มีสิทธิ์) | R2 (HR Manager) | BR-008 ต้องอัปเดต: เพิ่ม leave type restriction ตาม employee_type |

---

## 8. QA List (v3 — Minor Design Decisions Only)

> ประเด็นต่อไปนี้ไม่ต้องถาม Business Owner หรือ HR Manager อีก — ทีม BA/SA ตัดสินใจและ document ไว้ใน System Requirement ได้เลย

### Low Priority (Design Decisions)

1. **[M1] กำหนด status name สำหรับการยกเลิกคำขอลา**
   ตัดสินใจว่าจะใช้ `Cancelled` หรือ `Withdrawn` เป็นชื่อ status เมื่อยกเลิกสำเร็จ และ `Cancellation Pending` หรือ `Cancel Requested` สำหรับสถานะระหว่างรอ re-approve

2. **[M2] กำหนดเกณฑ์ probation period สำหรับ leave eligibility**
   ระบุว่าระบบจะดึง probation end date จาก HRIS หรือคำนวณจาก hire_date + N เดือน เพื่อใช้ block สิทธิ์ลาพักผ่อนก่อนผ่านทดลองงาน

3. **[M3] กำหนด SLA action เมื่อหัวหน้างานไม่ re-approve ภายใน 1 วันทำการ**
   ตัดสินใจ: (a) ส่ง reminder Email ก่อนครบกำหนดกี่ชั่วโมง และ (b) เมื่อหมดเวลาแล้วให้ auto-approve, escalate ไป HR หรือ expire request

---

## 9. Data Quality Assessment

### 9.1 Completeness

- **ระดับ:** สูงมาก (93%)
- **เหตุผล:** ข้อมูลครอบคลุม process, actor, business rule, leave types (7 ประเภท), leave quota (ทุกประเภท + Outsource), notification events (4 events), cancel flow, accumulation table (5 tiers + cap), onboard process และ HRIS integration model ประเด็นที่ขาดเหลือ (M1–M3) เป็น design decisions ไม่ใช่ข้อมูลที่ต้องขอจาก Business

### 9.2 Consistency

- **ระดับ:** สูงมาก (95%)
- **เหตุผล:** ทุก conflict ใน v1 และ v2 ได้รับการ resolve ครบถ้วน ข้อมูลในทุกไฟล์สอดคล้องกัน เมื่อใช้ QA answers เป็น authoritative source ไม่มีความขัดแย้งที่ยังเปิดอยู่

### 9.3 Clarity

- **ระดับ:** สูงมาก (92%)
- **เหตุผล:** Business rules ทุกข้อชัดเจน โดยเฉพาะ leave quota, cancel flow, notification scope และ Outsource policy ที่เพิ่งได้รับการยืนยัน ประเด็น M1–M3 ยังต้องการ naming/design decision แต่ไม่กระทบ clarity ของ business logic

### 9.4 Traceability

- **ระดับ:** สูงมาก (92%)
- **เหตุผล:** ทุกข้อมูลสำคัญสามารถ trace กลับได้ถึง QA ID (C1–C4, A1–A8, R1–R6), ไฟล์ต้นทาง และ respondent ชัดเจน requirement ใหม่ NR1–NR3 ก็ trace กลับไปยัง R4, R5, R2 ได้โดยตรง

### 9.5 Requirement Readiness

- **ระดับ:** สูงมาก (90%)
- **เหตุผล:** ข้อมูลพร้อมเริ่ม System Requirement ได้ทันที core flow, business rules, data model, leave balance logic และ notification design มีข้อมูลครบ ประเด็น M1–M3 เป็นเพียง detail ที่กำหนดได้ระหว่าง design session

---

## 10. สรุปการเปลี่ยนแปลงทั้ง 3 รอบ

| มิติ | v1 (Baseline) | v2 (หลัง Business User) | v3 (หลัง HR Manager) |
|------|--------------|------------------------|----------------------|
| Completeness | 60% | 72% | **93%** |
| Consistency | 40% | 85% | **95%** |
| Clarity | 65% | 82% | **92%** |
| Traceability | 85% | 88% | **92%** |
| Requirement Readiness | 55% | 78% | **90%** |
| Conflicts คงเหลือ | 4 | 1 (partial) | **0** |
| Open QA Items | 12 | 6 | **3 (minor design only)** |
| **Final Verdict** | Partially Ready | Partially Ready | **Ready** |

---

## 11. Final Verdict

- **สถานะ:** `Ready`
- **เหตุผลหลัก:**
  - ทุก conflict และ ambiguity ที่เป็น business-level ได้รับการ resolve ครบถ้วนใน 2 รอบ (18/18 QA Closed)
  - ข้อมูลพร้อมสำหรับ System Requirement ทั้ง core flow, business rules, leave quota, cancel flow, notification design และ data model
  - ประเด็นที่เหลือ 3 รายการ (M1–M3) เป็น design-level decisions ที่ทีม BA/SA ตัดสินใจได้เอง
- **สิ่งที่ต้องระวัง:**
  - Requirement ใหม่ 3 รายการ (NR1–NR3) ที่ได้จากคำตอบ HR Manager ต้องเพิ่มใน Business Requirement ก่อน sign-off (BR-005 อัปเดต, BR-008 อัปเดต, BR ใหม่ Leave Balance Auto-Restore)
  - Outsource leave type restriction (R2) ต้องสะท้อนใน data model ผ่าน employee_type flag
- **สิ่งที่ควรทำต่อ:**
  1. ✅ **เริ่มจัดทำ System Requirement Document ได้เลย**
  2. อัปเดต Business Requirement (NR1–NR3) ก่อน sign-off เพื่อให้ครบถ้วน
  3. ตัดสินใจ design decisions M1–M3 ใน BA/SA workshop แล้ว document ลง System Requirement
  4. ออกแบบ leave_type permission matrix แยกตาม employee_type (ประจำ vs Outsource)
  5. กำหนด status machine diagram: Pending → Approved → Cancelled / Rejected → (ยื่นใหม่)

---

*รายงาน v3 นี้จัดทำโดย Requirement Data Quality Analysis Agent อ้างอิงจากคำตอบใน `requirement-data-quality-analysis-qa-list-v2.yaml` และข้อมูลสะสมจาก v1 ถึง v3*
