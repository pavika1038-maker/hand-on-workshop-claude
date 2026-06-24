# Knowledge Base — Testing (SIT)

> ไฟล์ index สำหรับ knowledge base ทั้งหมดใน Session 2 Day 3: SIT  
> ระบบ: Leave Request and Approval System  
> อัปเดตล่าสุด: 2026-06-16

---

## ไฟล์ทั้งหมดในโฟลเดอร์นี้

| ไฟล์ | วัตถุประสงค์ | ใช้เมื่อ |
|------|------------|---------|
| `std-sit.md` | มาตรฐาน SIT: scope, entry/exit criteria, defect severity, evidence guideline, naming | เริ่มต้นทุกครั้ง — กำหนดกรอบทำงาน |
| `template-sit-scenario.md` | Template และตัวอย่าง SIT scenario table | สร้าง `sit-scenario-*.md` ด้วย AI |
| `template-coverage-matrix.md` | Template สำหรับ coverage matrix + ตัวอย่างจริง | สร้าง `sit-coverage-matrix-*.md` |
| `template-sit-test-data.md` | Template สำหรับ test account, leave balance, test data แยกตาม scenario | สร้าง `sit-test-data-*.md` |
| `lesson-learned-sit-defects.md` | Defect history + edge case ที่มักพลาดใน SIT | สร้าง scenario เพิ่มเติมจากประสบการณ์จริง |

---

## วิธีใช้ร่วมกับ AI

### Prompt พื้นฐาน — สร้าง SIT Scenario

```
อ่านไฟล์เหล่านี้ก่อน:
1. 80-knowledge-base/testing/std-sit.md
2. 80-knowledge-base/testing/template-sit-scenario.md
3. 80-knowledge-base/testing/lesson-learned-sit-defects.md
4. {SRS หรือ functional-design ของ module ที่ต้องการ}

แล้วสร้าง SIT scenario สำหรับ: [ชื่อ module / flow]
บันทึกเป็นไฟล์: 30-coding/c2-sit/sit-scenario-{module}.md
```

### Prompt พื้นฐาน — สร้าง Coverage Matrix

```
อ่านไฟล์เหล่านี้ก่อน:
1. 80-knowledge-base/testing/template-coverage-matrix.md
2. 30-coding/c2-sit/sit-scenario-{module}.md
3. {SRS section ที่มี FR list}

แล้วสร้าง coverage matrix และบันทึกเป็น:
30-coding/c2-sit/sit-coverage-matrix-{module}.md
```

### Prompt พื้นฐาน — สร้าง Test Data

```
อ่านไฟล์เหล่านี้ก่อน:
1. 80-knowledge-base/testing/std-sit.md (§8 Test Accounts)
2. 80-knowledge-base/testing/template-sit-test-data.md
3. 30-coding/c2-sit/sit-scenario-{module}.md

แล้วสร้าง test data สำหรับ {module} และบันทึกเป็น:
30-coding/c2-sit/sit-test-data-{module}.md
```

---

## ลำดับการสร้าง SIT Asset

```
Step 1: สร้าง SIT Scenario  →  sit-scenario-leave-request.md
Step 2: สร้าง Coverage Matrix  →  sit-coverage-matrix-leave-request.md
Step 3: สร้าง Test Data  →  sit-test-data-leave-request.md
Step 4: สร้าง Test Plan  →  sit-test-plan-leave-request.md
```

---

## Checklist ก่อนเริ่มสร้าง SIT Asset ✅

- [x] มีมาตรฐาน SIT → `std-sit.md`
- [x] มี template SIT scenario → `template-sit-scenario.md`
- [x] มี template coverage matrix → `template-coverage-matrix.md`
- [x] มี template test data → `template-sit-test-data.md`
- [x] มี lesson learned / defect history → `lesson-learned-sit-defects.md`
- [ ] ไฟล์ทั้งหมดเข้าถึงได้จาก AI session ที่ใช้ (แนบเมื่อเริ่ม prompt)
