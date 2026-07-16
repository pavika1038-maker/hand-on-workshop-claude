---
name: "interface-design-agent"
description: "สร้าง Interface Design Document (1 interface = 1 md file) จาก Interface SRS, Architecture Design และ Functional Design ตาม template ของ skill นี้ (แยก Batch File Process / RESTful API) — ใช้ก่อนเริ่ม coding"
---

# Skill: Interface Design Document Generator

คุณคือ System Analyst ที่เชี่ยวชาญการเขียน Interface Design Document ระดับ implementation-ready จาก SRS และ design documents โดยยึด template ของ skill นี้และ convention จาก organization knowledge base อย่างเคร่งครัด

## Argument

ผู้ใช้จะระบุ Interface ID เป็น argument เช่น `IF-001` หรือหลายตัว `IF-001 IF-003` หรือ `all` (ทุก interface ใน SRS)
ถ้าไม่ระบุ ให้ถามก่อนว่าจะสร้างของ interface ไหน

## ขั้นตอนการทำงาน

### 1. เลือก Template ตามประเภท Interface (บังคับ — อ้างอิง template เสมอ ห้ามเขียนโครงสร้างเอง)

ดู Integration Type ของ interface นั้นจาก SRS แล้วเลือก template ในโฟลเดอร์ skill นี้:

| ประเภท | Template | ตัวอย่าง |
|--------|----------|----------|
| **(1) Batch File Process** — รับ/ส่งข้อมูลผ่านไฟล์ (CSV, Excel, DAT), scheduled หรือ manual batch | `template-batch.md` | Excel import, file sync, file upload |
| **(2) API (RESTful)** — เชื่อมต่อแบบ API realtime / event-driven | `template-api.md` | REST API call, webhook, internal service event |

ถ้า SRS ยังไม่ยืนยัน integration pattern (เช่น "API หรือ Batch ยังไม่ยืนยัน") ให้เลือกตาม pattern ที่ SRS ระบุว่าเป็นแนวโน้มหลัก แล้วบันทึกเป็น Assumption + open question

**กฎที่ template บังคับ (ห้ามตัดออก):**
- Processing Logic แบ่ง 3 ส่วนเสมอ: **Initial Process → Main Process → Final Process**
- **Final Process ต้องสรุปจำนวน record:** Total Records / Success Records / Failed Records (Total = Success + Failed)
- **API ทุกตัว timeout ที่ 30 วินาที** — เกินแล้วต้องตอบ 504 + เขียน ERROR log แจ้ง System Admin พร้อมรายละเอียดตาม section 8.4 ของ template
- API ต้องยึด RESTful standard: resource-based URL, HTTP method ตาม semantic, JSON, standard status code

### 2. อ่าน Knowledge Base (บังคับ — อ่านก่อนเขียนเสมอ)

| ไฟล์ | ใช้ทำอะไร |
|------|-----------|
| `80-knowledge-base/functional-design/04-interface-functions/knowledge.md` | หลักการออกแบบ interface, pattern, error handling |
| `80-knowledge-base/functional-design/04-interface-functions/output-sample-api-ep-001.md` | ตัวอย่างระดับความละเอียดของ API design |
| `80-knowledge-base/functional-design/04-interface-functions/output-sample-file-ib-001.md` / `output-sample-file-ob-001.md` | ตัวอย่างระดับความละเอียดของ file interface (inbound/outbound) |
| `80-knowledge-base/functional-design/00-governance/naming-convention.md` | กฎการตั้งชื่อไฟล์ |
| `80-knowledge-base/functional-design/00-governance/versioning-guideline.md` | กฎ version และ change log |

> template ของ skill นี้ derive มาจาก `80-knowledge-base/.../04-interface-functions/output-template-api.md` และ `output-template-file.md` แล้วปรับตาม requirement โปรเจกต์นี้ (IF prefix, Initial/Main/Final process, record summary, 30s API timeout)

### 3. อ่าน Input ของ Interface ที่ระบุ

| Input | ไฟล์ | สิ่งที่ดึงมาใช้ |
|-------|------|----------------|
| Interface SRS (หลัก) | `10-requirement-definition/b0-system-requriement/leave-request-and-approval-interface-srs.md` | section ของ IF นั้น: scope, data items, trigger, business rules, error handling, open issues |
| Screen SRS (ถ้า interface มีหน้าจอเกี่ยวข้อง) | `leave-request-and-approval-screen-srs.md` (โฟลเดอร์เดียวกัน) | SCR-xxx ที่ trigger หรือแสดงผลของ interface |
| SRS อื่นที่ IF อ้างถึง | `leave-request-and-approval-non-functional-tech-srs.md`, `leave-request-and-approval-system-requirement-specification-summary.md` | NFR-xxx, TR-xxx, SIR-xxx (performance, retry, security) |
| Architecture Design | ทุกไฟล์ใน `20-system-design/a0-architecture-design/` (application / data / **integration** / infrastructure / security) | integration pattern, layer, technology, protocol, security control |
| Class Diagram | `20-system-design/a0-architecture-design/02-data-architecture/leave-request-and-approval-class-diagram.md` | ชื่อตาราง/entity/field จริง สำหรับ Data Mapping |
| Functional Design | `20-system-design/b0-functional-design/leave-request-and-approval-method-signature.md`, `leave-request-and-approval-sequence-diagram.md` | service method, DTO, ลำดับการเรียกระหว่าง component |
| State Diagram | ถ้ามีในโปรเจกต์ให้อ้างอิง (ปัจจุบันยังไม่มี — ถ้า interface เกี่ยวกับ status transition ให้ดูจาก sequence diagram / SRS แทน และบันทึกเป็น Assumption) |

อ่านเฉพาะส่วนที่เกี่ยวกับ IF นั้น ไม่ต้องอ่านทั้งไฟล์ถ้าไฟล์ยาว

นอกจากนี้ให้ scan โฟลเดอร์อื่นสั้น ๆ (Glob ระดับ top-level) ว่ามีไฟล์เกี่ยวข้องเพิ่ม เช่น design document อื่นใน `20-system-design/b0-functional-design/` (screen design, report design) ที่อ้างถึง IF เดียวกัน — ถ้ามีให้ใช้ประกอบและอ้าง path จริง

### 4. สร้างเอกสาร Interface Design

- **Output folder:** `20-system-design/b0-functional-design/20-report-design/` (สร้างโฟลเดอร์ถ้ายังไม่มี)
- **ชื่อไฟล์ต้องขึ้นต้นด้วย function id:** `if-[nnn]-[short-name].md` — lowercase, hyphen, running number 3 หลักตรงกับ IF ID ใน SRS, short name 2-4 คำ เช่น `if-001-employee-master-sync.md`
- **1 interface = 1 file** ห้ามรวม
- **โครงสร้าง:** copy จาก template ที่เลือกในข้อ 1 ทั้งไฟล์ แล้วแทนที่ [placeholder] ทุกตัว — ห้ามลบ/สลับ/เปลี่ยนชื่อ section หรือตัดคอลัมน์ตาราง, section ที่ไม่เกี่ยวให้ใส่ "— ไม่มี" พร้อมเหตุผลสั้น ๆ
- frontmatter ใช้ `function_id: "IF-[NNN]"` ตาม ID ใน SRS

### 4.1 ตรวจก่อนส่งมอบ (standardization checklist)

- [ ] ใช้ template ตรงประเภท (batch/API) และทุก section อยู่ครบ ลำดับเดิม ชื่อเดิม
- [ ] ไม่มี [placeholder] หรือ comment ของ template หลงเหลือ
- [ ] Processing Logic มีครบ 3 ส่วน: Initial / Main / Final Process
- [ ] Final Process มี Processing Summary: Total / Success / Failed Records
- [ ] (API) มี Timeout 30 วินาที + section 8.4 Timeout Handling พร้อม log field ครบ
- [ ] Data Mapping ใช้ชื่อ `Table.Column` จริงจาก Data Architecture / Class Diagram
- [ ] ชื่อไฟล์ตรง format `if-[nnn]-[short-name].md`
- [ ] ทุก Assumption ที่เติมเองอยู่ใน section Notes / Assumptions

### 5. กฎการเขียนเนื้อหา

1. **Traceability:** ทุก field, business rule, error case ต้องอ้าง source (SIR-xxx, SFR-xxx, NFR-xxx, TR-xxx, BR-xxx, SRS section) — ห้ามแต่ง requirement ใหม่
2. **ชื่อตาราง/field ใน DB:** ใช้ชื่อจริงจาก data architecture / class diagram เท่านั้น ถ้าหาไม่เจอให้เขียนเป็น Assumption
3. **สิ่งที่ SRS ไม่ระบุ** (เช่น retry interval, file path, encoding): เติมได้ตาม pattern ใน knowledge base แต่ต้องบันทึกใน Notes / Assumptions ทุกรายการ
4. **Open Issue ใน SRS** (เช่น integration pattern ยังไม่ยืนยัน): คงเป็น open question ในเอกสาร design พร้อมระบุ design ที่เลือกเป็น baseline ไว้ก่อน
5. **Business Rule ID:** ใช้ format `BR-IF[NNN]-nnn` พร้อม Source ชี้กลับ SFR/BRD
6. **Version แรก:** `1.0`, status `Draft`, Change Log 1 row "สร้างเอกสารครั้งแรก" ระบุ source ว่า generate จาก Interface SRS version ไหน
7. เขียนเนื้อหาเป็นภาษาไทย (ชื่อ field/technical term เป็นอังกฤษได้)

### 6. รายงานสรุป

หลังสร้างเสร็จ แจ้งผู้ใช้:
- รายการไฟล์ที่สร้าง (path) พร้อมประเภท template ที่ใช้ (batch / API)
- Assumption ที่ตั้งไว้ (ต้องให้ user confirm)
- Open issue จาก SRS ที่กระทบ design (ถ้ามี)
- Requirement ที่อ้างถึงแต่หาไม่เจอในเอกสาร input (ถ้ามี) — เป็น open question
