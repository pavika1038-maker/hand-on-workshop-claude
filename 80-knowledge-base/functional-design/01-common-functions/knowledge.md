# Common Functions — องค์ความรู้หลัก

## 1. คำจำกัดความ

Common Functions คือ function ที่ใช้ร่วมกันทั้งระบบ ไม่ได้เป็นของ business module ใด module หนึ่ง
ทุก user role สามารถเข้าถึงได้ (ตามสิทธิ์ที่กำหนด)

## 2. ขอบเขต

| ประเภท | ตัวอย่าง | Prefix |
|--------|---------|--------|
| Authentication & Authorization | Login, Logout, Session Management | COM-001 ~ COM-010 |
| User Management | User Profile, Change Password, Preferences | COM-011 ~ COM-020 |
| Navigation & Layout | Menu Structure, Breadcrumb, Home Dashboard | COM-021 ~ COM-030 |
| Notification | In-app Notification, Email Notification Center | COM-031 ~ COM-040 |
| Utility | File Upload, Export, Print, Help | COM-041 ~ COM-050 |

## 3. หลักการออกแบบ

| หลักการ | คำอธิบาย |
|---------|---------|
| Menu ต้อง define ก่อนเสมอ | Menu Navigation เป็นตัวกำหนดโครงสร้างของระบบทั้งหมด |
| Reusable | Common function ต้องออกแบบให้ reuse ได้ทุก module |
| Consistent UX | ทุก common function ต้องมี look & feel เดียวกัน |
| Security-first | Login, Session, Authorization ต้องออกแบบตาม security standard |
| Centralized | Logic ของ common function ต้องอยู่ที่เดียว ไม่กระจาย |

## 4. Menu / Navigation Design

**สำคัญที่สุด:** Menu Navigation ต้อง define ก่อน function อื่นทั้งหมด เพราะเป็นตัวกำหนดว่า:
- ระบบมี module อะไรบ้าง
- แต่ละ module มีหน้าจออะไร
- ผู้ใช้แต่ละ role เห็นเมนูอะไร

### ตัวอย่าง Menu Structure

```text
├── Home (Dashboard)
├── Leave
│   ├── Create Request
│   ├── My Requests
│   └── Approval Worklist
├── Reports
│   ├── Leave Summary
│   └── Usage Dashboard
├── Admin (HR only)
│   ├── Employee Management
│   ├── Leave Type Setup
│   └── Holiday Calendar
└── Settings
    ├── My Profile
    └── Preferences
```

### Menu Visibility by Role

| เมนู | Employee | Outsource | Manager | HR |
|------|----------|-----------|---------|-----|
| Home | ✅ | ✅ | ✅ | ✅ |
| Leave > Create Request | ✅ | ✅ | ✅ | ✅ |
| Leave > Approval Worklist | ❌ | ❌ | ✅ | ✅ |
| Reports | ❌ | ❌ | ❌ | ✅ |
| Admin | ❌ | ❌ | ❌ | ✅ |

## 5. Best Practice

- ออกแบบ Menu Navigation ก่อน แล้วค่อยแตก Screen/Report/Interface/Batch functions
- Login flow ต้องรองรับ SSO / AD Login ตาม security architecture ขององค์กร
- Dashboard ต้องแสดงข้อมูลตาม role (Employee เห็นของตัวเอง, Manager เห็นทีม, HR เห็นทั้งหมด)
- Notification ต้อง centralize ไม่กระจายไปแต่ละ module
