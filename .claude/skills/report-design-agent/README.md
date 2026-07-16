# คู่มือการใช้งาน Skill: report-design-agent

สร้าง **Report Design Document** (1 report = 1 ไฟล์ .md) จาก Report SRS, Architecture Design และ Functional Design ตาม template มาตรฐานขององค์กร — ใช้ก่อนเริ่ม coding เพื่อให้ dev มีเอกสารระดับ implementation-ready

> Skill คู่กันสำหรับหน้าจอ: `screen-design-agent`

---

## 1. วิธีเรียกใช้

พิมพ์ใน Claude Code:

```
/report-design-agent [RP ID ที่ต้องการ]
```

### ตัวอย่าง Prompt

| สถานการณ์ | Prompt ตัวอย่าง |
|-----------|----------------|
| สร้าง 1 report | `/report-design-agent RP-001` |
| สร้างหลาย report พร้อมกัน | `/report-design-agent RP-001 RP-002` |
| สร้างทุก report ใน SRS | `/report-design-agent all` |
| สร้างใหม่ทับของเดิม (หลัง SRS เปลี่ยน) | `/report-design-agent RP-002 — Report SRS อัปเดตเป็น v1.1 แล้ว ช่วย regenerate และเพิ่ม change log` |
| ระบุ mockup ที่ต้องการใช้ | `/report-design-agent RP-001 — ใช้ mockup สไตล์ Odoo จาก report-mockup-index.md เป็น layout อ้างอิง` |
| รายงานส่งมอบลูกค้าแบบ formal | `/report-design-agent RP-001 — ขอเป็นรูปแบบ B (Formal Software Report Specification) ตาม KB` |

> ถ้าไม่ระบุ RP ID skill จะถามก่อนว่าต้องการสร้างของ report ไหน

### ตัวอย่าง Prompt แบบประโยคเต็ม (ไม่ใช้ slash command)

- "ใช้ skill report-design-agent สร้าง report design ของ RP-003 Notification Log Report ให้หน่อย"
- "ช่วย generate report design document สำหรับ Leave Balance Report (RP-002) ตาม knowledge base"

---

## 2. Input / Output

### Input ที่ skill อ่านอัตโนมัติ (ไม่ต้องแนบเอง)

| ลำดับ | ไฟล์ | ใช้ทำอะไร |
|:---:|------|-----------|
| 1 | `.claude/skills/report-design-agent/template.md` | **โครงสร้าง output ทางการ** — ทุกไฟล์ที่ generate ต้องตรงกับ template นี้ 100% |
| 2 | `80-knowledge-base/functional-design/03-report-functions/` (knowledge, samples) | หลักการออกแบบ report, sorting/summary pattern |
| 3 | `80-knowledge-base/functional-design/00-governance/` | naming convention, versioning guideline |
| 4 | `10-requirement-definition/b0-system-requriement/leave-request-and-approval-report-srs.md` | **แหล่ง requirement หลัก** — parameters, columns, messages, business rules ของ RP นั้น |
| 5 | Screen SRS + SRS อื่นที่อ้างถึง | SCR-xxx (หน้าจอ filter ของ report), NFR-xxx |
| 6 | `report-mockup-index.md` (root) + ไฟล์ mockup | layout อ้างอิงจริง (SAP / Dynamics / Odoo / Zoho style) — ใช้แทนการวาด ASCII เอง |
| 7 | `20-system-design/a0-architecture-design/` (application, data) | ชื่อตาราง/คอลัมน์จริง, export mechanism |
| 8 | `20-system-design/b0-functional-design/` (method-signature, sequence-diagram) | service method, filter DTO, pagination |

### Output

| รายการ | รายละเอียด |
|--------|-----------|
| ตำแหน่ง | `20-system-design/b0-functional-design/20-report-design/` |
| ชื่อไฟล์ | `rp-[nnn]-[short-name].md` เช่น `rp-001-leave-summary.md` (lowercase + hyphen ตาม naming convention) |
| จำนวน | 1 RP = 1 ไฟล์ เสมอ |
| โครงสร้าง | 14 section + Change Log ตาม template (Overview, Business Purpose, Parameters, Report Layout + Columns, Mockup, Commands, Sorting Logic, Summary Logic, Query/Data Source, Business Rules, Message List, Database Tables, Exception Handling, Notes/Assumptions) |
| Version แรก | 1.0, status Draft |

### Report ในโปรเจกต์นี้ (จาก Report SRS §1.2)

| Report ID | Report Name | Phase |
|-----------|-------------|-------|
| RP-001 | Leave Summary Report | Phase 2 |
| RP-002 | Leave Balance Report | Phase 2 |
| RP-003 | Notification Log Report | Phase 2 |

---

## 3. กระบวนการทำงานของ Skill (Process Flow)

```text
[ผู้ใช้ระบุ RP ID]
        │
        ▼
STEP 1  อ่าน Template + Knowledge Base
        ├─ template.md (โครงสร้าง output — บังคับ)
        ├─ knowledge.md (หลักการออกแบบ report function)
        └─ naming-convention.md / versioning-guideline.md
        │
        ▼
STEP 2  อ่าน Input เฉพาะ RP ที่ระบุ
        ├─ Report SRS → parameters, columns, layout, messages, rules
        ├─ report-mockup-index.md → mockup อ้างอิง (ถ้ามี)
        ├─ Data Architecture → ชื่อตาราง/คอลัมน์จริง (Table.Column)
        └─ Method Signature → service method + filter DTO + pagination
        │
        ▼
STEP 3  Generate เอกสาร
        ├─ copy โครงสร้างจาก template.md แล้วเติมข้อมูลจริง
        ├─ map ทุก parameter/column ลง DB column จริง (คอลัมน์ DB Mapping)
        ├─ เขียน pseudo query (JOIN, WHERE, GROUP BY, ORDER BY) จากตารางจริง
        ├─ ทุก rule/message อ้าง source (RFR/SFR/BR) — ห้ามแต่ง requirement เอง
        └─ สิ่งที่ SRS ไม่ระบุ → เติมได้แต่ต้องบันทึกเป็น Assumption (section 14)
        │
        ▼
STEP 3.1 ตรวจ Standardization Checklist
        ├─ section ครบตาม template ลำดับเดิม
        ├─ ไม่มี [placeholder] หลงเหลือ
        ├─ DB Mapping ใช้ชื่อจริงจาก Data Architecture
        ├─ Sorting + Summary Logic ระบุครบ
        └─ ชื่อไฟล์ตรง format rp-[nnn]-[short-name].md
        │
        ▼
STEP 4  บันทึกไฟล์ลง 20-report-design/
        │
        ▼
STEP 5  รายงานสรุปให้ผู้ใช้
        ├─ path ไฟล์ที่สร้าง
        ├─ Assumption ที่ตั้งไว้ → ผู้ใช้ต้อง confirm
        └─ Open question (requirement ที่อ้างถึงแต่หาไม่เจอ)
```

### หลักการสำคัญที่ skill ยึด

| หลักการ | ความหมาย |
|---------|----------|
| **Traceability** | ทุก parameter, column, business rule ต้องชี้กลับ source (RFR-xxx, SFR-xxx, BRD BR-xxx) ได้ — ไม่มีการแต่ง requirement ใหม่ |
| **ชื่อ DB จริงเท่านั้น** | DB Mapping และ Query Logic ใช้ชื่อจาก Data Architecture Design เท่านั้น หาไม่เจอ = เขียนเป็น Assumption |
| **Mockup จริงก่อนวาดเอง** | ถ้ามี mockup ใน report-mockup-index.md ให้อ้าง path จริง — วาด ASCII เองเฉพาะเมื่อไม่มี (และเป็น Assumption) |
| **Assumption โปร่งใส** | ทุกอย่างที่เติมเองนอกเหนือ SRS (sort ระดับรอง, format ตัวเลข, page size) ต้องอยู่ใน section 14 |
| **Message ID ตาม SRS** | ถ้า SRS กำหนดแล้ว (เช่น ERR-RPT-001) ใช้ตามเดิม — message ใหม่เท่านั้นที่ใช้ `ERR-RP[NNN]-nnn` |
| **1 report = 1 file** | ไม่รวมหลาย RP ในไฟล์เดียว เทียบ version และ review ได้ง่าย |

---

## 4. สิ่งที่ต้องทำหลัง skill ทำงานเสร็จ (ฝั่งผู้ใช้)

1. **Review Assumption** ใน section 14 ของทุกไฟล์ — confirm กับ Business/HR แล้วอัปเดตเอกสาร
2. **Review Query Logic** — ให้ dev/DBA ตรวจ JOIN และเงื่อนไขก่อน implement
3. เมื่อ SRS เปลี่ยน → เรียก skill ใหม่พร้อมบอก version SRS เพื่อ regenerate + เพิ่ม row ใน Change Log ของเอกสาร

## 5. ไฟล์ในโฟลเดอร์ skill นี้

| ไฟล์ | หน้าที่ |
|------|--------|
| `SKILL.md` | คำสั่งหลักของ skill (Claude อ่านตอนถูกเรียก) |
| `template.md` | โครงสร้าง output ทางการ (รูปแบบ A — Internal) — แก้ไฟล์นี้ไฟล์เดียวถ้าต้องการเปลี่ยนมาตรฐานเอกสาร |
| `README.md` | คู่มือฉบับนี้ (สำหรับคน ไม่ใช่สำหรับ Claude) |
