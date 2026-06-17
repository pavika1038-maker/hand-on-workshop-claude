# Functional Design — Naming Convention

## 1. File Naming Format

```
[TYPE]-[RUNNING_NO]-[SHORT_NAME].md
```

## 2. Type Prefix

| ประเภท Function | Code | ตัวอย่าง |
|-----------------|------|---------|
| Common Function | COM | COM-001-login.md |
| Screen Function | SCR | SCR-001-create-request.md |
| Report Function | RPT | RPT-001-leave-summary.md |
| Interface Function | INT | INT-001-hr-api-inbound.md |
| Batch Function | BAT | BAT-001-nightly-sync.md |

## 3. กฎการตั้งชื่อ

| กฎ | รายละเอียด | ตัวอย่างถูก | ตัวอย่างผิด |
|----|-----------|-----------|-----------|
| ใช้ lowercase ทั้งหมด | ชื่อไฟล์ต้องเป็น lowercase | `scr-001-create-request.md` | `SCR-001-Create-Request.md` |
| ใช้ hyphen (-) คั่นคำ | ห้ามใช้ space หรือ underscore | `com-001-login.md` | `com_001_login.md` |
| Running number 3 หลัก | เริ่มจาก 001 | `scr-001` | `scr-1` |
| Short name กระชับ | 2-4 คำ อธิบายสิ่งที่ function ทำ | `create-request` | `create-leave-request-form-screen` |
| 1 function = 1 file | ห้ามรวมหลาย function ในไฟล์เดียว | — | — |

## 4. Running Number Range (แนะนำ)

| ช่วง | สำหรับ |
|------|--------|
| 001-099 | Core functions ของ module |
| 100-199 | Extended functions |
| 200-299 | Enhancement / Phase 2+ |
| 900-999 | Utility / Helper functions |
