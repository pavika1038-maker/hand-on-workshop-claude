# Governance — องค์ความรู้หลัก

## 1. วัตถุประสงค์

Governance คือชุดกฎและมาตรฐานที่ควบคุมกระบวนการจัดทำเอกสาร Functional Design ทั้งหมด
เพื่อให้ทุกเอกสารมีคุณภาพสม่ำเสมอ ติดตามได้ และใช้งานร่วมกันระหว่าง BA / SA / Dev / QA

## 2. หลักการสำคัญ

| หลักการ | คำอธิบาย |
|---------|---------|
| 1 Function = 1 Document | ห้ามรวมหลาย function ในไฟล์เดียว |
| ทุก Function ต้องมี ID | ใช้ prefix ตามประเภท (COM/SCR/RPT/INT/BAT) + running number 3 หลัก |
| Single Source of Truth | Function Index เป็นตัวคุม scope ทั้งหมด |
| Template-based | ทุกเอกสารต้องสร้างจาก template มาตรฐาน |
| Traceable | ทุก function ต้อง trace กลับไปยัง requirement (SRS/BRD) ได้ |
| Versioned | ทุกการเปลี่ยนแปลงต้องบันทึก version และ change log |

## 3. Workflow มาตรฐาน

```text
1. Define Menu / Navigation (COM-003)
       ↓
2. ลงทะเบียน function ใน Function Index
       ↓
3. สร้างเอกสารจาก output-template.md (1 function = 1 file)
       ↓
4. Review → Approve
       ↓
5. อัปเดต Function Index (status, cross-link)
       ↓
6. เมื่อแก้ไข → bump version + บันทึก change log
```

## 4. บทบาทและความรับผิดชอบ

| บทบาท | ความรับผิดชอบ |
|-------|-------------|
| BA (Business Analyst) | เขียน Common Functions, Screen Functions, Report Functions |
| SA (System Analyst) | เขียน Interface Functions, Batch Functions, ตรวจสอบ technical feasibility |
| Dev (Developer) | Review ความเป็นไปได้ทางเทคนิค, ให้ feedback |
| QA (Quality Assurance) | ใช้เอกสารเป็น basis สำหรับ test case design |
| PM (Project Manager) | ติดตาม status ผ่าน Function Index |

## 5. เอกสารที่เกี่ยวข้อง

| เอกสาร | ไฟล์ | หน้าที่ |
|--------|------|--------|
| Naming Convention | [naming-convention.md](naming-convention.md) | กฎการตั้งชื่อไฟล์และ ID |
| Document Template | [document-template.md](document-template.md) | template สำหรับสร้างเอกสาร |
| Versioning Guideline | [versioning-guideline.md](versioning-guideline.md) | กฎการจัดการ version |
| Function Index | [../function-index.md](../function-index.md) | ดัชนี function ทั้งหมด |
