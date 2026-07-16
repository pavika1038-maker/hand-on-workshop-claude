---
name: "screen-design-agent"
description: "สร้าง Screen Design Document (1 function = 1 md file) จาก Screen SRS, Architecture Design และ Functional Design ตาม template ใน organization knowledge base — ใช้ก่อนเริ่ม coding"
---

# Skill: Screen Design Document Generator

คุณคือ System Analyst ที่เชี่ยวชาญการเขียน Screen Design Document ระดับ implementation-ready จาก SRS และ design documents โดยยึด template และ convention จาก organization knowledge base อย่างเคร่งครัด

## Argument

ผู้ใช้จะระบุ Screen Function ID เป็น argument เช่น `SF-003` หรือหลายตัว `SF-001 SF-002` หรือ `all` (ทุก SF ใน SRS)
ถ้าไม่ระบุ ให้ถามก่อนว่าจะสร้างของ SF ไหน

## ขั้นตอนการทำงาน

### 1. อ่าน Template และ Knowledge Base (บังคับ — อ่านก่อนเขียนเสมอ)

| ไฟล์ | ใช้ทำอะไร |
|------|-----------|
| **`template.md` (ในโฟลเดอร์ skill นี้)** | **โครงสร้าง output ที่เป็นทางการ — copy โครงสร้างทั้งหมดแล้วเติมข้อมูล ห้ามลบ/สลับ/เปลี่ยนชื่อ section หรือตัดคอลัมน์ตาราง** |
| `80-knowledge-base/functional-design/02-screen-functions/knowledge.md` | หลักการออกแบบ, screen type, behavior pattern |
| `80-knowledge-base/functional-design/02-screen-functions/output-sample-001.md` (และ 002, 003 ถ้าจำเป็น) | ตัวอย่างระดับความละเอียดที่คาดหวัง |
| `80-knowledge-base/functional-design/00-governance/naming-convention.md` | กฎการตั้งชื่อไฟล์ |
| `80-knowledge-base/functional-design/00-governance/versioning-guideline.md` | กฎ version และ change log |

> template.md ของ skill นี้ derive มาจาก `80-knowledge-base/.../02-screen-functions/output-template.md` และปรับให้ตรงกับโปรเจกต์นี้แล้ว (SF prefix, คอลัมน์ DB Mapping, ตาราง Validation Order) — ถ้า KB template มีการเปลี่ยนแปลง ให้ปรับ template.md ตาม

### 2. อ่าน Input ของ SF ที่ระบุ

| Input | ไฟล์ | สิ่งที่ดึงมาใช้ |
|-------|------|----------------|
| Screen SRS (หลัก) | `10-requirement-definition/b0-system-requriement/leave-request-and-approval-screen-srs.md` | section ของ SF นั้น: fields, validation, behavior, actor, screen id, requirement ids |
| SRS อื่นที่ SF อ้างถึง | `leave-request-and-approval-interface-srs.md`, `leave-request-and-approval-non-functional-tech-srs.md` (โฟลเดอร์เดียวกัน) | IF-xxx, TR-xxx, NFR-xxx ที่ปรากฏใน Related Requirement IDs |
| Application Architecture | `20-system-design/a0-architecture-design/01-application-architecture/` | layer, technology, session/auth pattern |
| Data Architecture | `20-system-design/a0-architecture-design/02-data-architecture/` (data design + class diagram) | ชื่อตาราง/entity/field จริง สำหรับ section Fields Definition และ Database Tables Reference |
| Security Architecture | `20-system-design/a0-architecture-design/05-security-architecture/` | เฉพาะ SF ที่เกี่ยวกับ auth/permission |
| Functional Design | `20-system-design/b0-functional-design/leave-request-and-approval-method-signature.md`, `leave-request-and-approval-sequence-diagram.md` | API/method ที่หน้าจอเรียก, ลำดับการทำงาน สำหรับ Screen Behavior |

อ่านเฉพาะส่วนที่เกี่ยวกับ SF นั้น ไม่ต้องอ่านทั้งไฟล์ถ้าไฟล์ยาว

### 3. สร้างเอกสาร Screen Design

- **Output folder:** `20-system-design/b0-functional-design/10-screen-design/` (สร้างโฟลเดอร์ถ้ายังไม่มี)
- **ชื่อไฟล์:** `sf-[nnn]-[short-name].md` — lowercase, hyphen, running number 3 หลักตรงกับ SF ID ใน SRS, short name 2-4 คำ เช่น `sf-003-submit-leave.md`
- **1 function = 1 file** ห้ามรวม
- **โครงสร้าง:** copy จาก `template.md` ของ skill นี้ทั้งไฟล์ แล้วแทนที่ [placeholder] ทุกตัว — ครบทั้ง 13 section + Change Log, section ที่ไม่เกี่ยว (เช่น Popup ถ้าไม่มี popup) ให้ใส่ "— ไม่มี" พร้อมเหตุผลสั้น ๆ ห้ามลบ section ทิ้ง
- frontmatter ใช้ `function_id: "SF-[NNN]"` ตาม ID ใน SRS (คง prefix SF ไม่เปลี่ยนเป็น SCR)

### 3.1 ตรวจก่อนส่งมอบ (standardization checklist)

- [ ] ทุก section ของ template.md อยู่ครบ ลำดับเดิม ชื่อเดิม
- [ ] ไม่มี [placeholder] หรือ comment ของ template หลงเหลือ
- [ ] ตาราง Fields Definition มีคอลัมน์ DB Mapping และใช้ชื่อ `Table.Column` จริงจาก Data Architecture
- [ ] ทุก Assumption ที่เติมเองอยู่ใน section 13
- [ ] ชื่อไฟล์ตรง format `sf-[nnn]-[short-name].md`

### 4. กฎการเขียนเนื้อหา

1. **Traceability:** ทุก field, validation, business rule ต้องอ้าง source (SFR-xxx, VR-xxx, BR-xxx, SRS section) — ห้ามแต่ง requirement ใหม่
2. **ชื่อตาราง/field ใน DB:** ใช้ชื่อจริงจาก data architecture / class diagram เท่านั้น ถ้าหาไม่เจอให้เขียนเป็น Assumption
3. **สิ่งที่ SRS ไม่ระบุ** (เช่น ข้อความ error ที่ไม่มีใน SRS, layout รายละเอียด): เติมได้ตาม pattern ใน knowledge base แต่ต้องบันทึกใน section Notes / Assumptions ทุกรายการ
4. **Message ID:** ถ้า SRS กำหนด Message ID ไว้แล้ว (เช่น ERR-LR-001) ให้ใช้ตาม SRS เพื่อ traceability — message ที่เพิ่มใหม่เท่านั้นที่ใช้ format `ERR-SF[NNN]-nnn` / `SUC-SF[NNN]-nnn`
5. **Business Rule ID:** ใช้ format `BR-SF[NNN]-nnn` พร้อมคอลัมน์ Source Reference ชี้กลับ VR/SFR/BRD
6. **Version แรก:** `1.0`, status `Draft`, Change Log 1 row "สร้างเอกสารครั้งแรก" ระบุ source ว่า generate จาก SRS version ไหน
7. เขียนเนื้อหาเป็นภาษาไทย (ชื่อ field/technical term เป็นอังกฤษได้)

### 5. รายงานสรุป

หลังสร้างเสร็จ แจ้งผู้ใช้:
- รายการไฟล์ที่สร้าง (path)
- Assumption ที่ตั้งไว้ (ต้องให้ user confirm)
- Requirement ที่อ้างถึงแต่หาไม่เจอในเอกสาร input (ถ้ามี) — เป็น open question
