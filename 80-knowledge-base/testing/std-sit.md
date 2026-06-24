# SIT Standard — Leave Request and Approval System

> มาตรฐานการทำ System Integration Testing สำหรับ Workshop นี้  
> ระบบ: Leave Request and Approval System (.NET 10 + React 18)  
> อัปเดตล่าสุด: 2026-06-16

---

## 1. ขอบเขตของ SIT

SIT ทดสอบการทำงานของระบบในระดับ **end-to-end business flow** ผ่าน UI หรือ API จริง โดยใช้ environment และ database ที่ใกล้เคียง production

| ระดับ | ทดสอบอะไร | เครื่องมือ |
|-------|----------|-----------|
| Unit Test | Application logic / Service layer (isolated) | xUnit + Moq |
| **SIT** | Business flow ตั้งแต่ UI → API → DB | Browser + Postman / Claude Browser Agent |
| UAT | Business acceptance โดย business user | Manual |

---

## 2. Entry Criteria (เงื่อนไขก่อนเริ่ม SIT)

- [ ] Unit Test ผ่านไม่ต่ำกว่า 70% (pass rate)
- [ ] ไม่มี P1 defect ค้างจาก unit test ที่ยังไม่ได้ fix
- [ ] Application build สำเร็จและ deploy บน SIT environment
- [ ] Seed data / test data พร้อม
- [ ] SIT Scenario และ Test Data ได้รับการ review แล้ว

---

## 3. Exit Criteria (เงื่อนไขผ่าน SIT)

- [ ] ทุก scenario priority High ผ่าน 100%
- [ ] ทุก scenario priority Medium ผ่าน ≥ 90%
- [ ] ไม่มี Critical defect ค้างอยู่
- [ ] High defect ค้างอยู่ได้ไม่เกิน 2 รายการ และมี workaround ชัดเจน
- [ ] Evidence (screenshot / log) ครบทุก scenario ที่ผ่าน

---

## 4. Defect Severity

| ระดับ | คำจำกัดความ | ตัวอย่าง | SLA แก้ไข |
|-------|------------|---------|-----------|
| **Critical** | ระบบ crash / ใช้งานไม่ได้ทั้งหมด | Login ไม่ได้เลย, DB connection down | ต้องแก้ก่อน test ต่อ |
| **High** | Business flow หลักทำงานผิด / ข้อมูลผิด | Submit แล้ว status ไม่เปลี่ยน, Balance ไม่ถูก deduct | แก้ภายใน 1 วัน |
| **Medium** | Feature บางส่วนผิด แต่มี workaround | Error message ไม่แสดง, UI ผิดรูปแบบ | แก้ก่อน UAT |
| **Low** | ความสวยงาม / ปัญหาเล็กน้อย | สี, ขนาด font, ช่องว่างไม่ตรง | แก้ใน next sprint |

---

## 5. Evidence Collection Guideline

### 5.1 สิ่งที่ต้อง capture ทุก scenario

| รายการ | วิธีเก็บ |
|--------|---------|
| Screenshot ก่อน action | Browser screenshot |
| Screenshot หลัง action (เห็น result) | Browser screenshot |
| API request/response (ถ้าทดสอบผ่าน Postman) | Export JSON หรือ screenshot |
| Database state (สำหรับ High/Critical scenario) | Query result screenshot |

### 5.2 การตั้งชื่อไฟล์ evidence

```
evidence-{scenario-id}-{step}-{pass|fail}.png
ตัวอย่าง: evidence-SIT-001-step3-pass.png
          evidence-SIT-002-step2-fail.png
```

### 5.3 โฟลเดอร์ evidence

```
30-coding/c2-sit/evidence/
├── SIT-001/
│   ├── evidence-SIT-001-step1-pass.png
│   └── evidence-SIT-001-step3-pass.png
├── SIT-002/
└── ...
```

---

## 6. Naming Convention

### 6.1 Scenario ID

```
SIT-{sequence:3 digits}
ตัวอย่าง: SIT-001, SIT-002, SIT-010
```

### 6.2 Scenario Name Format

```
{Actor} {verb} {object} {condition (optional)}
ตัวอย่าง: Employee submits leave request successfully
          Manager approves leave request
          System prevents submit when balance is insufficient
```

### 6.3 ไฟล์ Output

| ไฟล์ | ชื่อที่ใช้ |
|------|----------|
| SIT Scenario | `sit-scenario-{module}.md` |
| Coverage Matrix | `sit-coverage-matrix-{module}.md` |
| Test Data | `sit-test-data-{module}.md` |
| Execution Result | `sit-execution-result-{module}.md` |

---

## 7. Test Scope สำหรับ Leave Request System

### 7.1 Business Flows ที่ต้องทดสอบ

| Flow | SCR | Priority |
|------|-----|---------|
| Employee ยื่นคำขอลา | SCR-003 | High |
| Manager อนุมัติคำขอลา | SCR-004 | High |
| Manager ปฏิเสธคำขอลา | SCR-004 | High |
| Employee ยกเลิกคำขอ (Pending) | SCR-006 | High |
| Employee ยกเลิกคำขอ (Approved) → Manager approve cancel | SCR-006/007 | Medium |
| Employee ยกเลิกคำขอ (Approved) → Manager reject cancel | SCR-006/007 | Medium |
| HR ดูรายการคำขอทั้งหมด | SCR-010 | Medium |
| System แสดง error เมื่อข้อมูลไม่ครบ / balance ไม่พอ | SCR-003 | High |
| SLA escalate เมื่อ Manager ไม่อนุมัติภายในกำหนด | SCR-008 | Low |

### 7.2 สิ่งที่ไม่อยู่ใน SIT scope นี้

- Email notification จริง (mock ใน SIT)
- Performance / load test
- Security penetration test
- Mobile browser (ทดสอบเฉพาะ desktop Chrome)

---

## 8. Test Account สำหรับ SIT

| Role | EmployeeId | ชื่อ | Manager |
|------|-----------|------|---------|
| Employee | EMP001 | สมชาย ใจดี | EMP002 |
| Manager | EMP002 | วิชัย รักงาน | — |
| HR | EMP003 | นันทา พร้อมใจ | EMP002 |

> **หมายเหตุ:** ใช้ `X-Employee-Id` header แทน JWT (Mock Auth)
