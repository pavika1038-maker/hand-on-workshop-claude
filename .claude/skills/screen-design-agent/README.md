# คู่มือการใช้งาน Skill: screen-design-agent

สร้าง **Screen Design Document** (1 function = 1 ไฟล์ .md) จาก Screen SRS, Architecture Design และ Functional Design ตาม template มาตรฐานขององค์กร — ใช้ก่อนเริ่ม coding เพื่อให้ dev มีเอกสารระดับ implementation-ready

---

## 1. วิธีเรียกใช้

พิมพ์ใน Claude Code:

```
/screen-design-agent [SF ID ที่ต้องการ]
```

### ตัวอย่าง Prompt

| สถานการณ์ | Prompt ตัวอย่าง |
|-----------|----------------|
| สร้าง 1 function | `/screen-design-agent SF-001` |
| สร้างหลาย function พร้อมกัน | `/screen-design-agent SF-001 SF-002 SF-004` |
| สร้างทุก function ใน SRS | `/screen-design-agent all` |
| สร้างเฉพาะ Phase 1 | `/screen-design-agent SF-001 ถึง SF-012` |
| สร้างใหม่ทับของเดิม (หลัง SRS เปลี่ยน) | `/screen-design-agent SF-003 — SRS อัปเดตเป็น v1.2 แล้ว ช่วย regenerate และเพิ่ม change log` |
| ระบุเงื่อนไขเพิ่ม | `/screen-design-agent SF-011 — หน้าจอนี้เป็น dashboard ขอให้เน้น section Fields Definition ของ chart/widget เป็นพิเศษ` |

> ถ้าไม่ระบุ SF ID skill จะถามก่อนว่าต้องการสร้างของ function ไหน

### ตัวอย่าง Prompt แบบประโยคเต็ม (ไม่ใช้ slash command)

- "ใช้ skill screen-design-agent สร้าง screen design ของ SF-007 Cancel Leave ให้หน่อย"
- "ช่วย generate screen design document สำหรับหน้า Manager Approval Inbox (SF-004) ตาม knowledge base"

---

## 2. Input / Output

### Input ที่ skill อ่านอัตโนมัติ (ไม่ต้องแนบเอง)

| ลำดับ | ไฟล์ | ใช้ทำอะไร |
|:---:|------|-----------|
| 1 | `.claude/skills/screen-design-agent/template.md` | **โครงสร้าง output ทางการ** — ทุกไฟล์ที่ generate ต้องตรงกับ template นี้ 100% |
| 2 | `80-knowledge-base/functional-design/02-screen-functions/` (knowledge, samples) | หลักการออกแบบ, screen type, ระดับความละเอียดที่คาดหวัง |
| 3 | `80-knowledge-base/functional-design/00-governance/` | naming convention, versioning guideline |
| 4 | `10-requirement-definition/b0-system-requriement/leave-request-and-approval-screen-srs.md` | **แหล่ง requirement หลัก** — fields, validation, behavior, messages ของ SF นั้น |
| 5 | SRS อื่นที่ SF อ้างถึง (interface-srs, non-functional-tech-srs) | IF-xxx, TR-xxx, NFR-xxx |
| 6 | `20-system-design/a0-architecture-design/` (application, data, security) | ชื่อตาราง/คอลัมน์จริง, layer, auth pattern |
| 7 | `20-system-design/b0-functional-design/` (method-signature, sequence-diagram) | service method ที่หน้าจอเรียก, ลำดับ validation, transaction |

### Output

| รายการ | รายละเอียด |
|--------|-----------|
| ตำแหน่ง | `20-system-design/b0-functional-design/10-screen-design/` |
| ชื่อไฟล์ | `sf-[nnn]-[short-name].md` เช่น `sf-003-submit-leave.md` (lowercase + hyphen ตาม naming convention) |
| จำนวน | 1 SF = 1 ไฟล์ เสมอ |
| โครงสร้าง | 13 section + Change Log ตาม template (Overview, Business Purpose, Screen Overview + Flow, Mockup, Fields Definition, Commands, Screen Behavior, Business Rules, Message List, Popup, Database Tables, Exception Handling, Notes/Assumptions) |
| Version แรก | 1.0, status Draft |

---

## 3. กระบวนการทำงานของ Skill (Process Flow)

```text
[ผู้ใช้ระบุ SF ID]
        │
        ▼
STEP 1  อ่าน Template + Knowledge Base
        ├─ template.md (โครงสร้าง output — บังคับ)
        ├─ knowledge.md (หลักการออกแบบ screen function)
        └─ naming-convention.md / versioning-guideline.md
        │
        ▼
STEP 2  อ่าน Input เฉพาะ SF ที่ระบุ
        ├─ Screen SRS → fields, validation (VR-xxx), behavior, messages
        ├─ Data Architecture → ชื่อตาราง/คอลัมน์จริง (Table.Column)
        ├─ Method Signature → service method + ลำดับ validation + transaction
        └─ Sequence Diagram / SRS อื่น → flow และ requirement ที่อ้างถึง
        │
        ▼
STEP 3  Generate เอกสาร
        ├─ copy โครงสร้างจาก template.md แล้วเติมข้อมูลจริง
        ├─ map ทุก field ลง DB column จริง (คอลัมน์ DB Mapping)
        ├─ ทุก rule/message อ้าง source (SFR/VR/BR) — ห้ามแต่ง requirement เอง
        └─ สิ่งที่ SRS ไม่ระบุ → เติมได้แต่ต้องบันทึกเป็น Assumption (section 13)
        │
        ▼
STEP 3.1 ตรวจ Standardization Checklist
        ├─ section ครบตาม template ลำดับเดิม
        ├─ ไม่มี [placeholder] หลงเหลือ
        ├─ DB Mapping ใช้ชื่อจริงจาก Data Architecture
        └─ ชื่อไฟล์ตรง format sf-[nnn]-[short-name].md
        │
        ▼
STEP 4  บันทึกไฟล์ลง 10-screen-design/
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
| **Traceability** | ทุก field, validation, business rule ต้องชี้กลับ source (SFR-xxx, VR-xxx, BRD BR-xxx) ได้ — ไม่มีการแต่ง requirement ใหม่ |
| **ชื่อ DB จริงเท่านั้น** | คอลัมน์ DB Mapping ใช้ชื่อจาก Data Architecture Design เท่านั้น หาไม่เจอ = เขียนเป็น Assumption |
| **Assumption โปร่งใส** | ทุกอย่างที่เติมเองนอกเหนือ SRS (mockup, error message ใหม่, format เลข ref) ต้องอยู่ใน section 13 เพื่อให้ Business/UX confirm |
| **Message ID ตาม SRS** | ถ้า SRS กำหนดแล้ว (เช่น ERR-LR-001) ใช้ตามเดิม — message ใหม่เท่านั้นที่ใช้ `ERR-SF[NNN]-nnn` |
| **1 function = 1 file** | ไม่รวมหลาย SF ในไฟล์เดียว เทียบ version และ review ได้ง่าย |

---

## 4. สิ่งที่ต้องทำหลัง skill ทำงานเสร็จ (ฝั่งผู้ใช้)

1. **Review Assumption** ใน section 13 ของทุกไฟล์ — confirm กับ Business/UX แล้วอัปเดตเอกสาร
2. **Review ASCII mockup** — ถ้ามี mockup ทางการภายหลัง ให้แทนที่และเพิ่ม change log
3. เมื่อ SRS เปลี่ยน → เรียก skill ใหม่พร้อมบอก version SRS เพื่อ regenerate + เพิ่ม row ใน Change Log ของเอกสาร

## 5. ตัวอย่างผลลัพธ์จริง

ดู [sf-003-submit-leave.md](../../../20-system-design/b0-functional-design/10-screen-design/sf-003-submit-leave.md) — generate จาก Screen SRS §2.3 (SF-003) เป็นตัวอย่าง baseline ของระดับความละเอียดที่คาดหวัง

## 6. ไฟล์ในโฟลเดอร์ skill นี้

| ไฟล์ | หน้าที่ |
|------|--------|
| `SKILL.md` | คำสั่งหลักของ skill (Claude อ่านตอนถูกเรียก) |
| `template.md` | โครงสร้าง output ทางการ — แก้ไฟล์นี้ไฟล์เดียวถ้าต้องการเปลี่ยนมาตรฐานเอกสาร |
| `README.md` | คู่มือฉบับนี้ (สำหรับคน ไม่ใช่สำหรับ Claude) |
