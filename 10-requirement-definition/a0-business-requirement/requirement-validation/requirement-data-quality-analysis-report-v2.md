# Requirement Data Quality Analysis Report — v2

> **โปรเจกต์:** ระบบบริหารการลาและการอนุมัติ — ABC Company
> **วิเคราะห์โดย:** Requirement Data Quality Analysis Agent
> **เวอร์ชัน:** v2 (อัปเดตหลังได้รับคำตอบจาก Business User)
> **อ้างอิง v1:** `requirement-data-quality-analysis-report.md`
> **แหล่งคำตอบ:** `requirement-data-quality-analysis-qa-list.yaml` (12/12 Closed)

---

## 1. Executive Summary

- **จำนวนไฟล์ที่วิเคราะห์:** 6 ไฟล์ (raw-extracted 5 ไฟล์ + QA answers 1 ไฟล์)
- **ภาพรวม:** หลังได้รับคำตอบจาก Business User ครบทั้ง 12 รายการ ประเด็นขัดแย้งหลัก 3 ใน 4 รายการได้รับการ resolve แล้ว และประเด็นไม่ชัดเจนได้รับการตอบบางส่วน ทำให้ข้อมูลมีความพร้อมสูงขึ้นอย่างมีนัยสำคัญสำหรับ core flow แต่ยังคงมีประเด็นที่ต้องขอข้อมูลเพิ่มเติม 5 รายการก่อนออกแบบ data model ขั้นสมบูรณ์
- **ประเด็นที่ Resolve แล้ว (จาก v1):** 6 รายการ (Fully Resolved)
- **ประเด็นที่ Partially Resolved:** 6 รายการ (มีคำตอบแต่ยังต้องการข้อมูลเพิ่ม)
- **ประเด็นคงเหลือที่ต้องดำเนินการต่อ:** 5 รายการ
- **ระดับความพร้อม:** `Partially Ready` (ปรับปรุงจาก v1)
- **สรุปความเห็น:** ข้อมูลพร้อมเพียงพอสำหรับการออกแบบ System Requirement ในส่วน core flow (access, leave request, approval, notification, HR monitoring) แต่ยังต้องการข้อมูลเพิ่มเติมในส่วน leave quota ของลากิจและ Outsource, cancel/edit business rule, และตารางสะสมวันลาก่อนออกแบบ data model ขั้นสมบูรณ์

---

## 2. Source Files Reviewed

| ลำดับ | ชื่อไฟล์ | ประเภท | บทบาทใน v2 |
|-------|---------|--------|------------|
| 1 | `ABC-Company-Leave-Form.md` | `.md` | ข้อมูลต้นทาง — แบบฟอร์มการยื่นใบลา |
| 2 | `ABC-Compay-HR-Regularion.md` | `.md` | ข้อมูลต้นทาง — ระเบียบ HR 17 หมวด |
| 3 | `ABC-Leave-Workflow-Thai-QA.md` | `.md` | ข้อมูลต้นทาง — บทสนทนา HR Q&A |
| 4 | `Leave-Management-Request-and-Approval-Business-User.yaml` | `.yaml` | ข้อมูลต้นทาง — Business Requirements หลัก |
| 5 | `ระบบบริหารการลาและการอนุมัติ.md` | `.md` | ข้อมูลต้นทาง — Presentation สรุป |
| 6 | `requirement-data-quality-analysis-qa-list.yaml` | `.yaml` | **ใหม่ใน v2** — คำตอบจาก Business User (12/12 Closed) |

---

## 3. สถานะการ Resolve ประเด็นจาก v1

### 3.1 Conflicting Information — สถานะหลังได้รับคำตอบ

| ID | ประเด็น | สถานะ v2 | สรุปคำตอบที่ได้รับ |
|----|---------|----------|-------------------|
| C1 | รายการประเภทการลา | ✅ **Resolved** | ยืนยัน 7 ประเภท: ลาป่วย, ลากิจส่วนตัว, ลาพักผ่อนประจำปี, ลาคลอดบุตร, ลาเพื่อทำหมัน, ลารับราชการทหาร, ลาอุปสมบท (เปิดรับเพิ่มได้ในอนาคต) |
| C2 | ระยะเวลาแจ้งล่วงหน้าลาพักร้อน | ✅ **Resolved** | ยืนยัน: ต้องแจ้งล่วงหน้าอย่างน้อย **1 วัน** (ค่าจาก PPTX ถูกต้อง) |
| C3 | สถานะการเข้าถึง HRIS ปัจจุบัน | ✅ **Resolved** | HRIS เปิดเฉพาะ HR เท่านั้น — ระบบใหม่เป็น Web App แยก integrate กับ HRIS เดิม (ไม่ replace) |
| C4 | ข้อมูลลากิจ 45 วัน (ข้าราชการ) | ⚠️ **Partially Resolved** | ยืนยันว่าเป็นข้อมูลผิดพลาดจากกฎหมายราชการ — แต่ยังไม่ได้รับสิทธิ์ลากิจที่ถูกต้องของ ABC |

### 3.2 Ambiguous or Incomplete — สถานะหลังได้รับคำตอบ

| ID | ประเด็น | สถานะ v2 | สรุปคำตอบที่ได้รับ |
|----|---------|----------|-------------------|
| A1 | จำนวนวันลาสูงสุดต่อปี | ⚠️ **Partially Resolved** | ลาพักผ่อน 10 วัน/ปี ✓, ลาป่วยตามจริง ✓, **ลากิจยังไม่มีข้อมูล** ✗ |
| A2 | สิทธิ์วันลา Outsource | ⚠️ **Partially Resolved** | ยืนยันต้องรองรับ Outsource ในระบบเดียว ✓, **สิทธิ์วันลาและ onboard process ยังไม่ชัด** ✗ |
| A3 | Approval hierarchy | ✅ **Resolved** | 1 ระดับ — Line Manager เท่านั้น; Multi-level นอก Phase 1 |
| A4 | Cancel/Edit คำขอลา | ⚠️ **Partially Resolved** | มี proposed rules (Cancel: Pending ได้, Approved ต้อง re-approve, ไม่อนุญาต Edit หลัง Approve) — **ยังต้องยืนยันจาก Business Owner** ✗ |
| A5 | เหตุผลการ Reject | ✅ **Resolved** | Optional field + แสดงใน Email แจ้งผล |
| A6 | ผู้รับ Email notification | ⚠️ **Partially Resolved** | Event mapping ชัดเจน (ยื่น→Manager, Approve→Employee+HR, Reject→Employee) — **scope HR ยังไม่ 100%** ✗ |
| A7 | ช่องทาง notification Manager | ✅ **Resolved** | Email (หลัก) + In-system notification (เสริม) |
| A8 | ตารางสะสมวันลาพักผ่อน | ⚠️ **Partially Resolved** | มี proposed table แต่ **ยังต้องการ HR ยืนยันตารางจริง** ✗ |

---

## 4. ประเด็นที่ยังเปิดอยู่ (Open Items หลัง v2)

> ประเด็นต่อไปนี้ได้รับคำตอบบางส่วนแต่ยังต้องการข้อมูลเพิ่มเติมก่อนออกแบบ System Requirement ขั้นสมบูรณ์

| ID | ประเด็น | แหล่งข้อมูลที่ยังขาด | ผลกระทบต่อระบบ | ลำดับความสำคัญ |
|----|---------|----------------------|-----------------|----------------|
| R1 | **สิทธิ์วันลากิจ** ของพนักงาน ABC Company (จำนวนวัน/ปี) | HR ต้องยืนยันตัวเลขที่ถูกต้อง | สูง — กระทบ leave quota data model และ validation rule | **High** |
| R2 | **สิทธิ์วันลาของ Outsource** เทียบกับพนักงานประจำ | HR ต้องยืนยันนโยบายแยกหรือเท่ากัน | สูง — กระทบ leave balance calculation แยกตาม employee type | **High** |
| R3 | **Process onboard Outsource** เข้าระบบใหม่ | HR/IT ต้องกำหนด flow การนำเข้าข้อมูล Outsource | กลาง — กระทบ initial data migration และ user management | **Medium** |
| R4 | **Business rule Cancel (กรณี Approved)** — ยืนยัน re-approve flow | Business Owner ต้องยืนยัน proposed rules | กลาง — กระทบ status transition และ workflow design | **Medium** |
| R5 | **HR Email notification scope** — ทุก event หรือเฉพาะ Approved | Business Owner / HR ต้องกำหนด | กลาง — กระทบ notification design และ email template | **Medium** |
| R6 | **ตารางสะสมวันลาพักผ่อนตามอายุงาน** (เกณฑ์จริง) | HR ต้องให้ตารางที่ได้รับอนุมัติ | กลาง — กระทบ leave balance calculation logic | **Medium** |

---

## 5. Duplicated Information (คงเดิมจาก v1)

> ประเด็นซ้ำซ้อนเหล่านี้ไม่ได้รับผลกระทบจากคำตอบของ Business User เนื่องจากเป็นเรื่องของการซ้ำกันระหว่างเอกสาร ไม่ใช่ข้อขัดแย้ง

| ID | ประเด็น | ไฟล์ที่เกี่ยวข้อง | หมายเหตุ v2 |
|----|---------|------------------|------------|
| D1 | ประเภทการลา | Leave-Form.md, HR-Regulation.md, QA.md, YAML, PPTX.md | Resolved ด้านรายการ (C1) — ความซ้ำซ้อนยังมีอยู่ตามธรรมชาติของเอกสาร |
| D2 | ใบรับรองแพทย์กรณีลาป่วย > 3 วัน | QA.md, PPTX.md | ยืนยันสอดคล้องกัน — ไม่มีความขัดแย้ง |
| D3 | Workflow การยื่นลา | QA.md, YAML, PPTX.md | C3 Resolved แล้ว — Workflow ใน YAML (to-be) เป็น version หลักที่ใช้อ้างอิง |

---

## 6. QA List (เฉพาะประเด็นคงเหลือใน v2)

> QA รายการต่อไปนี้เป็นประเด็นที่ยังต้องดำเนินการต่อหลังจาก v1 ทุกรายการถูก Closed แล้ว

### High Priority

1. **[R1] สิทธิ์วันลากิจ (จำนวนวัน/ปี) สำหรับพนักงาน ABC Company**
   ข้อมูล PPTX ที่ระบุ 45 วันสำหรับข้าราชการได้รับการยืนยันว่าผิดพลาด แต่ยังไม่ได้รับตัวเลขที่ถูกต้องสำหรับพนักงาน ABC กรุณาระบุจำนวนวันลากิจสูงสุดต่อปี แยกตามประเภทพนักงาน (ประจำ / Outsource) ถ้ามีความแตกต่าง

2. **[R2] นโยบายสิทธิ์วันลาของพนักงาน Outsource**
   ยืนยันว่าพนักงาน Outsource มีสิทธิ์วันลาแต่ละประเภทเท่ากับพนักงานประจำหรือแตกต่าง หากแตกต่างขอตารางสิทธิ์แยกต่างหาก

### Medium Priority

3. **[R3] Process onboard ข้อมูล Outsource เข้าระบบใหม่**
   กำหนด flow ว่า HR จะนำเข้าข้อมูลพนักงาน Outsource อย่างไร (manual input, import file, หรือ API จากต้นสังกัด) และข้อมูลใดบ้างที่ต้องมี

4. **[R4] ยืนยัน Business Rule กรณียกเลิกคำขอที่ Approved แล้ว**
   Proposed rule: เมื่อ Approved แล้วพนักงานต้องให้หัวหน้าอนุมัติการยกเลิก (re-approve) — ขอให้ Business Owner ยืนยันหรือปรับ flow นี้

5. **[R5] กำหนด scope HR สำหรับ Email notification**
   ยืนยันว่า HR ต้องได้รับ Email notification ในทุก event (ยื่น, อนุมัติ, ปฏิเสธ) หรือเฉพาะบาง event เพื่อกำหนด email template ให้ครบ

6. **[R6] ตารางสะสมวันลาพักผ่อนประจำปีตามอายุงาน**
   ขอตารางที่ HR อนุมัติแล้ว ระบุจำนวนวันลาพักผ่อนตามเกณฑ์อายุงาน (เช่น 0–1 ปี, 1–3 ปี, 3+ ปี) เพื่อใช้ในการคำนวณ leave balance

---

## 7. Data Quality Assessment

### 7.1 Completeness

- **ระดับ:** ปานกลาง-สูง (72%) — เพิ่มขึ้นจาก v1 (60%)
- **เหตุผล:** ได้รับข้อมูลประเภทการลาครบ 7 ประเภท, approval model ชัดเจน, notification channel และ reject rule กำหนดแล้ว ยังขาดข้อมูลสิทธิ์วันลากิจ, สิทธิ์ Outsource, ตารางสะสมวันลา และ cancel flow ที่ได้รับการยืนยัน

### 7.2 Consistency

- **ระดับ:** สูง (85%) — เพิ่มขึ้นอย่างมากจาก v1 (40%)
- **เหตุผล:** ความขัดแย้ง 3 ใน 4 รายการ (C1, C2, C3) ได้รับการ resolve อย่างชัดเจน C4 ยืนยันว่าเป็นข้อมูลผิดพลาดแล้ว แต่ยังขาดข้อมูลที่ถูกต้องมาแทนที่

### 7.3 Clarity

- **ระดับ:** สูง (82%) — เพิ่มขึ้นจาก v1 (65%)
- **เหตุผล:** ขอบเขตระบบ, approval model, notification flow และประเภทการลาชัดเจนมากขึ้น ยังมีความไม่ชัดในเรื่อง quota ของลากิจและ Outsource policy

### 7.4 Traceability

- **ระดับ:** สูง (88%) — เพิ่มขึ้นเล็กน้อยจาก v1 (85%)
- **เหตุผล:** คำตอบ QA ทุกรายการสามารถ trace กลับไปยัง QA ID ใน qa-list.yaml และ reference ID (C1, A3, ฯลฯ) ได้ชัดเจน

### 7.5 Requirement Readiness

- **ระดับ:** สูง (78%) — เพิ่มขึ้นจาก v1 (55%)
- **เหตุผล:** Core flow (Login → ดูสิทธิ์ → ยื่นลา → อนุมัติ/ปฏิเสธ → Email แจ้งผล → HR ติดตาม) พร้อมสำหรับการออกแบบ System Requirement แล้ว ส่วนที่ยังขาดคือ business rule เชิง data (quota, accumulation) ซึ่งส่งผลต่อ data model แต่ไม่ blocking core feature

---

## 8. Final Verdict

- **สถานะ:** `Partially Ready` (คืบหน้าจาก v1 อย่างมีนัยสำคัญ)
- **เหตุผลหลัก:**
  - Core process และ business rule หลัก (ประเภทการลา, approval, notification, HRIS integration) พร้อมสำหรับการทำ System Requirement แล้ว
  - ยังขาด leave quota data ของลากิจและ Outsource ซึ่งจำเป็นสำหรับการออกแบบ leave balance module
- **ความเสี่ยงที่เหลือ:**
  - หากเริ่มออกแบบ leave balance module โดยไม่มีข้อมูล R1 และ R2 อาจต้องแก้ไข data model ในภายหลัง
  - Cancel flow (R4) ที่ยังไม่ได้รับการยืนยันอาจกระทบ status machine design
- **สิ่งที่ควรทำต่อ:**
  1. นำ Open Items R1 และ R2 (High Priority) ไปถาม HR ทันที
  2. เริ่มออกแบบ System Requirement สำหรับ core flow ได้เลยโดยไม่ต้องรอ
  3. สำหรับ leave balance module ให้ออกแบบ data structure รองรับ configurable quota เพื่อรอรับค่าจาก HR ทีหลัง
  4. นำ R4–R6 (Medium Priority) ยืนยันในรอบ BA workshop ถัดไปก่อน sign-off requirement

---

## 9. สรุปการเปลี่ยนแปลงจาก v1 สู่ v2

| มิติ | v1 | v2 | การเปลี่ยนแปลง |
|------|----|----|----------------|
| Completeness | 60% | 72% | +12% |
| Consistency | 40% | 85% | +45% |
| Clarity | 65% | 82% | +17% |
| Traceability | 85% | 88% | +3% |
| Requirement Readiness | 55% | 78% | +23% |
| ประเด็นขัดแย้ง | 4 รายการ | 0 Fully Open (1 Partial) | ลดลง 3 รายการ |
| ประเด็นไม่ชัดเจน | 8 รายการ | 6 Open Items คงเหลือ | ลดลง 2 รายการ (resolve fully 3) |
| **Final Verdict** | `Partially Ready` | `Partially Ready` (ใกล้ Ready) | Core flow พร้อม, ยังขาด quota data |

---

*รายงาน v2 นี้จัดทำโดย Requirement Data Quality Analysis Agent อ้างอิงจากคำตอบใน `requirement-data-quality-analysis-qa-list.yaml` และไฟล์ต้นทางใน `raw-extracted/`*
