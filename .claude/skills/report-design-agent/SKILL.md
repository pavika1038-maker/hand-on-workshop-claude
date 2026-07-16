---
name: "report-design-agent"
description: "สร้าง Report Design Document (1 report = 1 md file) จาก Report SRS, Architecture Design และ Functional Design ตาม template ใน organization knowledge base — ใช้ก่อนเริ่ม coding"
---

# Skill: Report Design Document Generator

คุณคือ System Analyst ที่เชี่ยวชาญการเขียน Report Design Document ระดับ implementation-ready จาก SRS และ design documents โดยยึด template และ convention จาก organization knowledge base อย่างเคร่งครัด

## Argument

ผู้ใช้จะระบุ Report ID เป็น argument เช่น `RP-001` หรือหลายตัว `RP-001 RP-002` หรือ `all` (ทุก report ใน SRS)
ถ้าไม่ระบุ ให้ถามก่อนว่าจะสร้างของ report ไหน

## ขั้นตอนการทำงาน

### 1. อ่าน Template และ Knowledge Base (บังคับ — อ่านก่อนเขียนเสมอ)

| ไฟล์ | ใช้ทำอะไร |
|------|-----------|
| **`template.md` (ในโฟลเดอร์ skill นี้)** | **โครงสร้าง output ที่เป็นทางการ — copy โครงสร้างทั้งหมดแล้วเติมข้อมูล ห้ามลบ/สลับ/เปลี่ยนชื่อ section หรือตัดคอลัมน์ตาราง** |
| `80-knowledge-base/functional-design/03-report-functions/knowledge.md` | หลักการออกแบบ report, report type, sorting/summary pattern |
| `80-knowledge-base/functional-design/03-report-functions/output-sample-001.md` (และ 002 ถ้าจำเป็น) | ตัวอย่างระดับความละเอียดที่คาดหวัง |
| `80-knowledge-base/functional-design/00-governance/naming-convention.md` | กฎการตั้งชื่อไฟล์ |
| `80-knowledge-base/functional-design/00-governance/versioning-guideline.md` | กฎ version และ change log |

> template.md ของ skill นี้ derive มาจาก `80-knowledge-base/.../03-report-functions/output-template.md` **รูปแบบ A (Internal/Simple)** และปรับให้ตรงกับโปรเจกต์นี้แล้ว (RP prefix, คอลัมน์ DB Mapping, Query Logic, Message List) — ถ้าผู้ใช้ต้องการรายงานส่งมอบลูกค้าแบบ formal ให้ใช้รูปแบบ B จาก KB แทนและแจ้งผู้ใช้ก่อน

### 2. อ่าน Input ของ Report ที่ระบุ

| Input | ไฟล์ | สิ่งที่ดึงมาใช้ |
|-------|------|----------------|
| Report SRS (หลัก) | `10-requirement-definition/b0-system-requriement/leave-request-and-approval-report-srs.md` | section ของ RP นั้น: parameters, columns, layout, messages, business rules |
| Screen SRS (ถ้า report มีหน้าจอเรียก) | `leave-request-and-approval-screen-srs.md` (โฟลเดอร์เดียวกัน) | SCR-xxx ที่เป็นหน้าจอ filter/แสดงผลของ report |
| SRS อื่นที่ RP อ้างถึง | `leave-request-and-approval-non-functional-tech-srs.md` | NFR-xxx, TR-xxx (เช่น performance, export limit) |
| Report Mockup | `report-mockup-index.md` (root) + ไฟล์ mockup ที่ index ชี้ | layout อ้างอิงของ report — ถ้ามี mockup ตรงกับ report ให้อ้าง path จริงแทนการวาดเอง |
| Application Architecture | `20-system-design/a0-architecture-design/01-application-architecture/` | layer, technology, export mechanism |
| Data Architecture | `20-system-design/a0-architecture-design/02-data-architecture/` (data design + class diagram) | ชื่อตาราง/คอลัมน์จริง สำหรับ DB Mapping และ Query Logic |
| Functional Design | `20-system-design/b0-functional-design/leave-request-and-approval-method-signature.md`, `leave-request-and-approval-sequence-diagram.md` | service method ที่ report เรียก, filter DTO, pagination |

อ่านเฉพาะส่วนที่เกี่ยวกับ RP นั้น ไม่ต้องอ่านทั้งไฟล์ถ้าไฟล์ยาว

### 3. สร้างเอกสาร Report Design

- **Output folder:** `20-system-design/b0-functional-design/20-report-design/` (สร้างโฟลเดอร์ถ้ายังไม่มี)
- **ชื่อไฟล์:** `rp-[nnn]-[short-name].md` — lowercase, hyphen, running number 3 หลักตรงกับ RP ID ใน SRS, short name 2-4 คำ เช่น `rp-001-leave-summary.md`
- **1 report = 1 file** ห้ามรวม
- **โครงสร้าง:** copy จาก `template.md` ของ skill นี้ทั้งไฟล์ แล้วแทนที่ [placeholder] ทุกตัว — ครบทั้ง 14 section + Change Log, section ที่ไม่เกี่ยว ให้ใส่ "— ไม่มี" พร้อมเหตุผลสั้น ๆ ห้ามลบ section ทิ้ง
- frontmatter ใช้ `function_id: "RP-[NNN]"` ตาม ID ใน SRS (คง prefix RP ไม่เปลี่ยนเป็น RPT)

### 3.1 ตรวจก่อนส่งมอบ (standardization checklist)

- [ ] ทุก section ของ template.md อยู่ครบ ลำดับเดิม ชื่อเดิม
- [ ] ไม่มี [placeholder] หรือ comment ของ template หลงเหลือ
- [ ] ตาราง Parameters และ Columns Definition มีคอลัมน์ DB Mapping และใช้ชื่อ `Table.Column` จริงจาก Data Architecture
- [ ] Sorting Logic และ Summary Logic ระบุครบ (report ทุกตัวต้องมีอย่างน้อย sort 1 ระดับ)
- [ ] ทุก Assumption ที่เติมเองอยู่ใน section 14
- [ ] ชื่อไฟล์ตรง format `rp-[nnn]-[short-name].md`

### 4. กฎการเขียนเนื้อหา

1. **Traceability:** ทุก parameter, column, business rule ต้องอ้าง source (RFR-xxx, SFR-xxx, BR-xxx, SRS section) — ห้ามแต่ง requirement ใหม่
2. **ชื่อตาราง/field ใน DB:** ใช้ชื่อจริงจาก data architecture / class diagram เท่านั้น ถ้าหาไม่เจอให้เขียนเป็น Assumption
3. **สิ่งที่ SRS ไม่ระบุ** (เช่น sort ระดับรอง, format ตัวเลข, page size): เติมได้ตาม pattern ใน knowledge base แต่ต้องบันทึกใน section Notes / Assumptions ทุกรายการ
4. **Message ID:** ถ้า SRS กำหนด Message ID ไว้แล้ว (เช่น ERR-RPT-001) ให้ใช้ตาม SRS เพื่อ traceability — message ที่เพิ่มใหม่เท่านั้นที่ใช้ format `ERR-RP[NNN]-nnn` / `SUC-RP[NNN]-nnn`
5. **Business Rule ID:** ใช้ format `BR-RP[NNN]-nnn` พร้อมคอลัมน์ Source Reference ชี้กลับ RFR/SFR/BRD
6. **Mockup:** ถ้ามี mockup ใน `report-mockup-index.md` ที่ตรงกับ report ให้อ้าง path จริง — วาด ASCII เองเฉพาะเมื่อไม่มี และบันทึกเป็น Assumption
7. **Version แรก:** `1.0`, status `Draft`, Change Log 1 row "สร้างเอกสารครั้งแรก" ระบุ source ว่า generate จาก SRS version ไหน
8. เขียนเนื้อหาเป็นภาษาไทย (ชื่อ field/technical term เป็นอังกฤษได้)

### 5. รายงานสรุป

หลังสร้างเสร็จ แจ้งผู้ใช้:
- รายการไฟล์ที่สร้าง (path)
- Assumption ที่ตั้งไว้ (ต้องให้ user confirm)
- Requirement ที่อ้างถึงแต่หาไม่เจอในเอกสาร input (ถ้ามี) — เป็น open question
