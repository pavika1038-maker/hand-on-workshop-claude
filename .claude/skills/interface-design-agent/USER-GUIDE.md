# คู่มือการใช้งาน Skill: interface-design-agent

> สร้าง Interface Design Document ระดับ implementation-ready (1 interface = 1 ไฟล์ .md) จาก Interface SRS, Architecture Design และ Functional Design — ใช้**ก่อนเริ่ม coding**

---

## 1. Skill นี้ทำอะไร

แปลง requirement ของ interface (IF-xxx) ใน SRS ให้เป็นเอกสาร design ที่ developer นำไป implement ได้ทันที โดย:

- เลือก template ตามประเภท interface อัตโนมัติ (Batch File / RESTful API)
- ดึงข้อมูลจาก SRS, architecture design, class diagram, method signature มา map เป็น field-level spec
- บังคับ convention ขององค์กร: Initial/Main/Final Process, record summary, API timeout 30 วินาที, process logging, error catalog
- ทุก field/rule มี traceability กลับไปยัง requirement ID เสมอ

## 2. ใช้ตอนไหนใน workflow

```
SRS (10-requirement-definition) ──┐
Architecture Design ──────────────┼──► [skill นี้] ──► Interface Design Doc ──► Coding
Functional Design ────────────────┘
```

ต้องมีเอกสาร input ครบก่อน (ดูข้อ 4) — ถ้า SRS ยังไม่ยืนยัน integration pattern skill จะเลือก pattern แนวโน้มหลักแล้วบันทึกเป็น Assumption ให้ user confirm

## 3. วิธีเรียกใช้

พิมพ์ในแชทได้เลย เช่น:

| คำสั่ง | ผลลัพธ์ |
| --- | --- |
| `ใช้ interface-design-agent สร้าง IF-001` | สร้าง design ของ IF-001 ตัวเดียว |
| `สร้าง interface design IF-001 IF-003` | สร้างหลายตัวพร้อมกัน |
| `สร้าง interface design ทั้งหมด` / `all` | สร้างทุก IF ที่อยู่ใน Interface SRS |
| (ไม่ระบุ ID) | skill จะถามก่อนว่าจะสร้างของ interface ไหน |

## 4. Input ที่ต้องมีก่อนใช้ (checklist)

| เอกสาร | Path | จำเป็น |
| --- | --- | :---: |
| Interface SRS | `10-requirement-definition/b0-system-requriement/leave-request-and-approval-interface-srs.md` | ✅ หลัก |
| Architecture Design | `20-system-design/a0-architecture-design/` (integration, data, security ฯลฯ) | ✅ |
| Class Diagram (ชื่อตาราง/field จริง) | `20-system-design/a0-architecture-design/02-data-architecture/leave-request-and-approval-class-diagram.md` | ✅ |
| Functional Design (method signature, sequence diagram) | `20-system-design/b0-functional-design/` | ✅ |
| Screen SRS / SRS อื่นที่ IF อ้างถึง | โฟลเดอร์เดียวกับ Interface SRS | ตามที่ IF อ้าง |
| Knowledge Base | `80-knowledge-base/functional-design/04-interface-functions/` + `00-governance/` | ✅ (skill อ่านเอง) |

ถ้าข้อมูลบางส่วนหาไม่เจอ skill จะไม่แต่งเอง แต่บันทึกเป็น **Assumption / Open Question** ในเอกสาร

## 5. Template ที่ skill ใช้ (เลือกอัตโนมัติตาม Integration Type ใน SRS)

| ประเภท | Template | ใช้กับ | Section เด่น |
| --- | --- | --- | --- |
| **(1) Batch File Process** | `template-batch.md` | Excel/CSV import, file sync, scheduled batch | File Format, File Naming, File Path, Trigger/Timing |
| **(2) API (RESTful)** | `template-api.md` | REST API, webhook, realtime/event-driven | Request/Response Field Mapping (Root + Length), HTTP Status Codes, Timeout Handling, Error Message Catalog |

## 6. กฎบังคับที่เอกสารทุกฉบับต้องมี (ห้ามตัด)

1. **Processing Logic 3 ส่วนเสมอ:** Initial Process → Main Process → Final Process
2. **Final Process ต้องสรุป record:** Total = Success + Failed
3. **(API) Timeout 30 วินาที:** เกินแล้วตอบ 504 + เขียน ERROR log แจ้ง System Admin (field ครบตาม section 8.4)
4. **(API) Case Branching:** ทุก DB step ต้องแตก Case exist / not exist / exception error → จบที่ sub-process มาตรฐาน (Success / Validation Fail / Exception Fail)
5. **(API) Process Logging:** เขียน log ตอน start / ทุก step / end ตามโครง log record ใน template
6. **(API) Fail response ต้องครบทุก field** — field ที่ไม่มีค่าใส่ `""` ห้ามตัดออก + set Error_Flag แยก validation/system error
7. **(API) Error Message Catalog:** ทุก error code ที่อ้างในเอกสารต้องอยู่ในตาราง section 8.5
8. **Traceability:** ทุก field/rule อ้าง SIR-xxx, SFR-xxx, NFR-xxx, BR-xxx — ห้ามแต่ง requirement ใหม่
9. **ชื่อตาราง/field:** ใช้ชื่อจริงจาก class diagram เท่านั้น

## 7. Output ที่ได้

- **โฟลเดอร์:** `20-system-design/b0-functional-design/20-report-design/`
- **ชื่อไฟล์:** `if-[nnn]-[short-name].md` (lowercase, hyphen) เช่น `if-001-employee-master-sync.md`
- **1 interface = 1 ไฟล์** — version แรก `1.0` status `Draft` พร้อม Change Log
- **รายงานสรุปท้ายงาน:** รายการไฟล์ที่สร้าง, Assumption ที่ต้องให้ confirm, open issue จาก SRS, requirement ที่หาไม่เจอ

## 8. สิ่งที่ user ต้องทำหลังได้เอกสาร

1. Review **Assumption** ทุกข้อใน section Notes/Assumptions — confirm หรือแก้
2. ตอบ **Open Question** (เช่น integration pattern ที่ยังไม่ยืนยัน, URL ของ environment ที่เป็น TBD)
3. เมื่อ review ผ่าน → เปลี่ยน status จาก `Draft` เป็น `Approved` + กรอกตาราง Approvals
4. ถ้าแก้เอกสารภายหลัง → เพิ่ม row ใน Change Log ของไฟล์ + central index `10-requirement-definition/b0-system-requriement/srs-change-log.md` และ mark field ที่แก้ตาม convention field-level change tracking

## 9. FAQ

| คำถาม | คำตอบ |
| --- | --- |
| SRS ยังไม่ระบุว่าเป็น API หรือ Batch? | skill เลือก pattern แนวโน้มหลักจาก SRS แล้วบันทึกเป็น Assumption + open question — user ต้อง confirm |
| อยากแก้ template? | แก้ `template-api.md` / `template-batch.md` ในโฟลเดอร์ skill นี้ — เอกสารที่สร้างใหม่จะใช้โครงล่าสุดทันที (เอกสารเก่าไม่ update ย้อนหลัง) |
| field ใน DB หาไม่เจอใน class diagram? | skill จะใส่เป็น Assumption — ห้ามเดาชื่อเอง user ต้องยืนยันชื่อจริง |
| สร้างซ้ำ IF เดิมได้ไหม? | ได้ แต่จะ overwrite ไฟล์เดิม — ถ้าต้องการแก้บางส่วนให้สั่งแก้เฉพาะจุด + update Change Log แทน |

## ไฟล์ในโฟลเดอร์ skill นี้

| ไฟล์ | หน้าที่ |
| --- | --- |
| `SKILL.md` | นิยาม skill + ขั้นตอนการทำงาน (สำหรับ agent) |
| `template-batch.md` | Template สำหรับ Batch File Process |
| `template-api.md` | Template สำหรับ RESTful API |
| `USER-GUIDE.md` | คู่มือนี้ (สำหรับคน) |
