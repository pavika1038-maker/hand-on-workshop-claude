---
name: "sit-execution"
description: "Execute SIT scenarios บนระบบจริงด้วย browser tools — บันทึก actual result, capture screenshot, log defect และสร้าง execution report พร้อม retest status"
---

# Skill: SIT Execution

คุณคือ QA execution specialist ที่รัน SIT scenario บนระบบจริงผ่าน browser tools ของ Claude Code

## วิธีทำงาน

### 1. อ่านและเตรียมก่อน execute

อ่านไฟล์ input ที่ระบุใน prompt:
- `requirement-summary-*.md` — expected behavior และ business rules
- `sit-scenario-*.md` — steps และ expected result
- `sit-test-data-*.yaml` — test data สำหรับแต่ละ scenario
- `sit-automation-*.md` — automation steps และ checkpoint (ถ้ามี)

ตรวจสอบก่อนเริ่ม:
- Base URL เข้าถึงได้
- Browser tools พร้อมใช้งาน
- Test data ครบตาม scenario ที่จะรัน

### 2. Execute แต่ละ scenario

สำหรับแต่ละ scenario ใน Run Scope:

1. **Setup** — login ด้วย user/role ที่ถูกต้องตาม test data
2. **Navigate** — ไปยัง menu หรือ URL ที่เกี่ยวข้อง
3. **Execute** — ทำตาม steps ทีละขั้น
4. **Capture** — screenshot ตาม checkpoint ที่กำหนด
5. **Assert** — ตรวจ actual result เทียบ expected result
6. **Log** — บันทึก PASS / FAIL พร้อมรายละเอียด

### 3. Screenshot / Evidence มาตรฐาน

Capture ที่จุดต่อไปนี้เสมอ:
- หลัง login สำเร็จ
- หน้า form ก่อนกรอกข้อมูล
- หลัง fill ข้อมูลครบก่อน submit
- หลัง submit / action สำคัญ (success หรือ error message)
- หน้าผลลัพธ์สุดท้าย (status หรือ approval result)

### 4. บันทึก Actual Result

| Scenario ID | Step | Expected | Actual | Status | Screenshot |
|-------------|------|----------|--------|--------|------------|
| SIT-001 | Submit | สถานะ Pending Approval | [actual] | PASS/FAIL | [path] |

### 5. Defect Log

เมื่อพบ defect ให้ log ดังนี้:

| Defect ID | Scenario | Step | Description | Severity | Status | Screenshot |
|-----------|----------|------|-------------|----------|--------|------------|
| DEF-001 | SIT-001 | 4 | [อธิบาย] | Critical/Major/Minor/Trivial | Open | [path] |

Severity definition:
- **Critical** — ระบบ crash หรือ data เสียหาย ไม่สามารถดำเนินการต่อได้
- **Major** — function หลักทำงานผิดพลาด มี workaround แต่ยาก
- **Minor** — function ทำงานได้แต่ output ไม่ถูกต้องบางส่วน
- **Trivial** — ปัญหา UI หรือ cosmetic ไม่กระทบ function

### 6. Output ที่ต้องสร้าง

อัปเดตหรือสร้างไฟล์ `sit-execution-report-[module].md` ประกอบด้วย:
- Result Summary ตาราง PASS/FAIL แต่ละ scenario
- Total: X PASS / Y FAIL / Z BLOCKED
- Evidence List พร้อม path
- Defect / Blocker List พร้อม severity
- Retest Recommendation