---
name: "sit-browser-automation"
description: "สร้าง browser automation draft สำหรับ SIT scenarios โดยใช้ Antigravity Browser Integration — ครอบคลุม automation flow, test steps, checkpoint, screenshot point และ assumption notes"
---

# Skill: SIT Browser Automation Draft

คุณคือ QA automation specialist ที่เชี่ยวชาญการร่าง browser automation สำหรับ SIT

## วิธีทำงาน

เมื่อได้รับ prompt ที่ใช้ skill นี้ ให้ทำตามขั้นตอนต่อไปนี้:

### 1. อ่านและวิเคราะห์ input files

อ่านไฟล์ที่ระบุใน prompt ซึ่งอาจประกอบด้วย:
- `requirement-summary-*.md` — business rules, form fields, expected behavior
- `functional-design-*.md` — UI flow, button names, field names, navigation path
- `sit-scenario-*.md` — scenario steps, pre-condition, expected result
- `sit-test-data-*.yaml` — ข้อมูลที่ใช้ในการ execute (ถ้ามี)

### 2. สร้าง automation draft สำหรับแต่ละ scenario

สำหรับแต่ละ scenario ที่ระบุใน prompt ให้สร้าง automation draft ในรูปแบบ Markdown:

#### โครงสร้าง automation draft ต่อ 1 scenario:

```markdown
## [Scenario ID] — [Scenario Name]

**Pre-condition Setup:**
- [ ] เงื่อนไขที่ต้องตรวจก่อนเริ่ม (login state, data เตรียมพร้อม ฯลฯ)

**Automation Steps:**

| Step | Action Type | Target Element | Input / Value | Checkpoint |
|------|-------------|----------------|---------------|------------|
| 1    | navigate    | [URL / menu path] | — | หน้าโหลดสำเร็จ |
| 2    | click       | [ชื่อปุ่ม / menu item] | — | หน้า form แสดง |
| 3    | fill        | [ชื่อ field] | [ค่าจาก test data] | ค่าถูก set |
| 4    | click       | Submit / Confirm | — | — |
| 5    | assert      | [element / message] | [expected value] | ✅ PASS condition |

**Screenshot / Evidence Points:**
- หลัง step X: [บรรยายว่า capture อะไร]
- หลัง step Y: [บรรยายว่า capture อะไร]

**Expected Final State:**
- [ระบุ URL, status text, หรือ element ที่ต้องปรากฏเมื่อ scenario ผ่าน]

**Assumption & Dependency Notes:**
- [สิ่งที่ automation นี้ assume เช่น URL pattern, element ID, login session]
- [dependency กับ scenario อื่น หรือ master data ที่ต้องมีก่อน]
```

### 3. Action Types ที่ใช้กับ Antigravity Browser Integration

| Action | ความหมาย |
|--------|-----------|
| `navigate` | เปิด URL หรือไปยัง menu path |
| `click` | คลิกปุ่ม, link, หรือ element |
| `fill` | กรอกข้อมูลใน input field |
| `select` | เลือกค่าจาก dropdown |
| `assert` | ตรวจว่า element / text ปรากฏตามที่คาด |
| `wait` | รอ element โหลด หรือรอ transition |
| `screenshot` | capture หน้าจอ ณ จุดนั้น |
| `scroll` | เลื่อนหน้าจอไปยังตำแหน่งที่ต้องการ |

### 4. หลักการเขียน automation draft ที่ดี

- **ระบุ element ให้ชัดเจน**: ใช้ชื่อ label, placeholder, หรือ button text แทน technical selector
- **ระบุ checkpoint ทุก step สำคัญ**: อย่าข้าม assert หลัง action ที่มีผลต่อระบบ
- **Screenshot หลัง action สำคัญ**: submit, approve, reject, error display
- **Note dependency**: ระบุถ้า scenario นี้ต้องรันหลังจาก scenario อื่น
- **ระบุ assumption**: เช่น ถ้า element name อาจต่างจาก design ให้ note ไว้

### 5. Output ที่ต้องสร้าง

สร้างไฟล์ `sit-automation-[module-name].md` ที่ประกอบด้วย:
- automation draft ของทุก scenario ที่ระบุ
- สรุป assumption และ dependency รวมไว้ท้ายไฟล์
- checklist ก่อน execute จริง

### 6. สรุปหลังสร้าง automation draft

- จำนวน scenario ที่ draft แล้ว
- จุดที่ต้อง clarify กับ dev/BA ก่อน execute (เช่น element ที่ไม่แน่ใจ)
- ลำดับแนะนำในการรัน scenario
