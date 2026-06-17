# Screen Functions — องค์ความรู้หลัก

## 1. คำจำกัดความ

Screen Functions คือ function ที่มีหน้าจอ (UI) ให้ผู้ใช้โต้ตอบ เช่น ฟอร์มกรอกข้อมูล, หน้าจออนุมัติ, หน้าจอค้นหา, หน้าจอแสดงรายละเอียด, หน้าจอจัดการ Master Data

## 2. ประเภทของ Screen Function

| ประเภท | คำอธิบาย | ตัวอย่าง |
|--------|---------|---------|
| Create / Entry Form | ฟอร์มสร้างข้อมูลใหม่ | สร้างคำขอลา, สร้างพนักงานใหม่ |
| Edit Form | ฟอร์มแก้ไขข้อมูลที่มีอยู่ | แก้ไขคำขอลา, แก้ไขข้อมูลส่วนตัว |
| CRUD Master Data | หน้าจอจัดการข้อมูลหลัก (Search + Create/Edit/Delete popup) | Reason Master, Category Master |
| Search & Action Form | หน้าจอค้นหาพร้อม action ต่อเนื่อง (สร้าง/แก้ไข/ดูรายละเอียด) | Customer Consent Search, Order Search |
| Approval Worklist | หน้าจอรายการรออนุมัติ | Manager Approval, HR Approval |
| Search / List | หน้าจอค้นหาและแสดงรายการ | ค้นหาคำขอลา, รายการพนักงาน |
| Detail / History | หน้าจอแสดงรายละเอียดและประวัติ | รายละเอียดคำขอ, ประวัติการอนุมัติ |
| Dashboard | หน้าจอแสดงภาพรวม (interactive) | Leave Dashboard, KPI Dashboard |

## 3. โครงสร้างเอกสาร Screen Function

เอกสาร Screen Function ประกอบด้วย section หลักดังนี้:

| Section | คำอธิบาย | บังคับ |
|---------|---------|--------|
| 1. Overview | ข้อมูลทั่วไปของ function (ID, Name, Type, Actor, References) | Y |
| 2. Business Purpose | อธิบายเหตุผลทางธุรกิจที่หน้าจอนี้มีอยู่ | Y |
| 3. Screen Overview | Navigation path, inbound/outbound, preconditions, postconditions | Y |
| 3.x Screen Paths | รายการ path ทั้งหมดของหน้าจอ (กรณี multi-path เช่น CRUD) | เมื่อมีหลาย path |
| 3.x Screen Flow | แผนภาพ navigation flow (text-based หรือ mermaid) | Y |
| 4. Mockup / UI Layout | ASCII mockup หรือ reference ไปยัง mockup file | Y |
| 5. Fields Definition | รายละเอียด field ทั้งหมด แบ่งตาม section/path | Y |
| 6. Commands / Actions | ปุ่มและ action ทั้งหมดพร้อม trigger condition | Y |
| 7. Screen Behavior | Process description ทุก event (onLoad, onClick, etc.) | Y |
| 8. Business Rules | กฎเกณฑ์ทางธุรกิจที่กระทบหน้าจอ | Y |
| 9. Message List | Error messages และ Success messages | Y |
| 10. Popup / Sub-screen | รายละเอียด popup หรือ sub-screen (ถ้ามี) | เมื่อมี popup |
| 11. Database Tables Reference | ตาราง database ที่เกี่ยวข้องพร้อม alias | เมื่อมี query |
| 12. Notes / Assumptions | สมมติฐานและข้อสังเกต | Y |
| Change Log | ประวัติการเปลี่ยนแปลงเอกสาร + สรุปการเปลี่ยนแปลงสำคัญ | Y |

## 4. หลักการออกแบบ

| หลักการ | คำอธิบาย |
|---------|---------|
| อ้างอิง Menu Navigation | ทุก screen ต้องระบุว่าเข้าถึงจากเมนูไหน |
| Navigation Flow ชัดเจน | ระบุ inbound (มาจากไหน) และ outbound (ไปไหนต่อ) พร้อม flow diagram |
| Screen Paths | หน้าจอที่มีหลาย path (เช่น CRUD) ต้องระบุ path ทั้งหมดและ item description แยกตาม path |
| Field-level detail | ทุก field ต้องระบุ label, data type, required, default, validation, data source |
| Command/Action ครบ | ทุกปุ่มต้องระบุ trigger condition, default state, และ system response |
| Business Rule ที่หน้าจอ | ระบุ rule ที่กระทบ field, command, behavior ของหน้าจอ พร้อม decision tree ถ้าซับซ้อน |
| Error Handling | ทุก error case ต้องมี user message, error source (Lookup Master), และ recovery action |
| Mockup / Wireframe | ทุก screen ต้องมี ASCII mockup inline หรือ reference ไปยัง mockup file |
| Responsive | ออกแบบให้ใช้งานได้ทุกอุปกรณ์ (PC, Laptop, Tablet, Mobile) |

## 5. Screen Behavior Pattern

| Event | คำอธิบาย | ตัวอย่าง |
|-------|---------|---------|
| onLoad | เมื่อเปิดหน้าจอ | ดึงข้อมูลมาแสดง, ตั้งค่า default, โหลด dropdown |
| onChange | เมื่อเปลี่ยนค่า field | คำนวณจำนวนวันลา, แสดง/ซ่อน field |
| onClick | เมื่อกดปุ่ม | Submit, Approve, Reject, Save Draft, Search, Clear |
| onSubmit | เมื่อ submit ฟอร์ม | Validate → Call API → แสดงผล |
| onError | เมื่อเกิด error | แสดง error message, highlight field |
| onPopupOpen | เมื่อเปิด popup | โหลดข้อมูลจากแถวที่เลือก, ตั้งค่า read-only/editable |
| onPopupClose | เมื่อปิด popup | refresh ตารางหน้าจอหลัก (ถ้ามีการเปลี่ยนแปลง) |

## 6. Validation Pattern

| ระดับ | เมื่อใดตรวจสอบ | ตัวอย่าง |
|-------|--------------|---------|
| Field-level | เมื่อ user ออกจาก field (onBlur) | Required check, format check, max length |
| Form-level | เมื่อ user กด Submit/Search | Cross-field validation (date range), at-least-one-criteria check |
| Server-level | เมื่อ API ได้รับ request | Re-validate ทุกอย่าง + business rule ที่ซับซ้อน |

### Validation Pattern ที่พบบ่อย

| Pattern | คำอธิบาย | ตัวอย่าง |
|---------|---------|---------|
| At-least-one | ต้องกรอกอย่างน้อย 1 เงื่อนไข | Search criteria validation |
| Date range | วันที่สิ้นสุด >= วันที่เริ่ม | ช่วงวันที่ค้นหา |
| Required check | ต้องกรอกข้อมูลก่อน save | Reason Description ใน Create popup |
| Duplicate check | ตรวจสอบข้อมูลซ้ำ | Reason Code ซ้ำ |

## 7. CRUD Pattern (Master Data Screen)

หน้าจอ Master Data มักมีโครงสร้างดังนี้:

| Component | คำอธิบาย |
|-----------|---------|
| Search Screen (PATH1) | หน้าจอหลักสำหรับค้นหาและแสดงรายการ พร้อมปุ่ม Create |
| Create Popup (PATH2) | Popup สำหรับสร้างข้อมูลใหม่ พร้อม validation |
| Edit Popup (PATH3) | Popup สำหรับแก้ไขข้อมูล (บาง field read-only) |
| Delete Popup (PATH4) | Popup สำหรับยืนยันการลบ (ทุก field read-only) |

### CRUD Operation Pattern

| Operation | วิธีการ | หมายเหตุ |
|-----------|--------|---------|
| Create (Insert) | Insert record ใหม่ พร้อม auto-generate code | ระบบสร้าง code อัตโนมัติ (เช่น CLASS_ID + running sequence) |
| Read (Search) | Query ด้วย optional filters + pagination | รองรับ records per page + page navigator |
| Update (Edit) | Update เฉพาะ field ที่แก้ไขได้ + audit fields | LASTUPD_DATE, LASTUPD_BY อัปเดตอัตโนมัติ |
| Delete (Soft Delete) | อัปเดต DELETE_FLAG = 1 แทนการลบจริง | ไม่ลบข้อมูลจริง เพื่อรักษาประวัติ |

## 8. Search & Action Pattern

หน้าจอ Search & Action มักมีโครงสร้างดังนี้:

| Component | คำอธิบาย |
|-----------|---------|
| Header Section | Session data (Organization, Branch, User) — read-only |
| Search Criteria | เงื่อนไขค้นหาหลายช่อง พร้อม dropdown จาก Lookup Master |
| Action Buttons | Search, Clear, Export (Enable/Disable ตามสถานะ) |
| Result Table | ตารางแสดงผลพร้อม action link ต่อแถว |
| Popup / Sub-screen | รายละเอียดเพิ่มเติมหรือ action ต่อเนื่อง |

### Action Link Logic

เมื่อ result table มี action link ที่ซับซ้อน ให้ใช้ decision tree (text-based) อธิบาย logic:

```text
ตรวจสอบเงื่อนไข
│
├── กรณี A → แสดง "Action A" (Link)
├── กรณี B → แสดง "Action B" (ข้อความเท่านั้น ไม่มี link)
└── กรณี C → แสดง "Action C" (Link ไปหน้าอื่น)
```

## 9. Popup Pattern

| ประเภท Popup | คำอธิบาย | ตัวอย่าง |
|-------------|---------|---------|
| Create Popup | ฟอร์มสร้างข้อมูลใหม่ (dropdown + textbox + Save/Clear) | Create New Reason |
| Edit Popup | ฟอร์มแก้ไข (บาง field read-only + textbox + Save/Clear) | Edit Reason |
| Delete Popup | ยืนยันการลบ (ทุก field read-only + Delete/Cancel) | Confirm Delete |
| Detail Popup | แสดงรายละเอียด (ทุก field read-only + Close) | Consent Details |

### Popup Behavior

- เปิด popup → โหลดข้อมูลจากแถวที่เลือก
- ปิด popup → refresh ตารางหน้าจอหลัก (ถ้ามีการเปลี่ยนแปลง)
- ทุก popup ต้องมีปุ่ม Close [X]

## 10. Error Message Pattern

Error messages ดึงจาก Lookup Master (centralized):

| Component | คำอธิบาย |
|-----------|---------|
| Error Source | Lookup Master (KEY='09', VALUE2='ERR_CODE') |
| Display | VALUE3 เป็นข้อความ error แสดงบนหน้าจอ |
| Pattern | ERR + running number (เช่น ERR0005, ERR0021, ERR0037) |

### Error Message ที่พบบ่อย

| ประเภท | ตัวอย่าง |
|--------|---------|
| Required field | "กรุณากรอกข้อมูลที่จำเป็น" |
| At-least-one criteria | "กรุณาใส่เงื่อนไขในการค้นหา อย่างน้อย 1 เงื่อนไข" |
| Date range invalid | "วันที่สิ้นสุด ต้องมากกว่า หรือ เท่ากับวันที่เริ่ม" |
| No data found | "ไม่พบข้อมูลตามเงื่อนไขที่ระบุ" |

## 11. Pagination Pattern

หน้าจอที่แสดงข้อมูลจำนวนมากควรมี pagination:

| Component | คำอธิบาย |
|-----------|---------|
| Records per page | Dropdown เลือกจำนวน record ต่อหน้า (10, 50, 100, 200) |
| Showing entries | แสดงข้อความ "Showing {first} to {last} of {total} entries" |
| Page Navigator | Previous / Next / Page Number |

## 12. Database Query Pattern

### Query Structure

```text
Table: [Main Table]
  JOIN [Related Table] (Alias) ON [Join Condition]
  ...

WHERE [Base Conditions]
  AND [Optional Search Filters]

ORDER BY [Sort Fields]
```

### Audit Fields

ทุกตารางที่มีการ Create/Update ควรมี audit fields:

| Field | คำอธิบาย |
|-------|---------|
| CREATED_DATE | วันที่สร้าง (ระบบสร้างอัตโนมัติ) |
| CREATED_BY | ผู้สร้าง (Session Login User ID) |
| LASTUPD_DATE | วันที่แก้ไขล่าสุด (ระบบอัปเดตอัตโนมัติ) |
| LASTUPD_BY | ผู้แก้ไขล่าสุด (Session Login User ID) |

## 13. Navigation Flow Diagram

ทุกหน้าจอควรมี navigation flow diagram อย่างน้อย 1 รูปแบบ:

| รูปแบบ | เมื่อใช้ | ตัวอย่าง |
|--------|---------|---------|
| Text-based tree | ทุกหน้าจอ (บังคับ) | Main Menu → Screen → Action |
| Mermaid flowchart | หน้าจอที่มี flow ซับซ้อน (แนะนำ) | flowchart LR with styling |

### Mermaid Flowchart Convention

- ใช้ `flowchart LR` (left-to-right) เป็นค่าเริ่มต้น
- หน้าจอปัจจุบัน: `fill:#EAD7A4,stroke:#333`
- หน้าจอภายนอก: `stroke-dasharray: 5 5`
- ระบุ label บน arrow เป็น action ที่ trigger navigation

## 14. Change Log Pattern

### Revision History

| Column | คำอธิบาย |
|--------|---------|
| Version | เลข version (0.1, 0.2, ..., 1.0, 1.1, ...) |
| Date | วันที่เปลี่ยนแปลง |
| Author | ผู้เปลี่ยนแปลง |
| Change Type | Created / Updated / CR_NUMBER |
| Description | อธิบายสิ่งที่เปลี่ยนแปลง |
| Remark | หมายเหตุเพิ่มเติม (optional) |

### สรุปการเปลี่ยนแปลงสำคัญ

เพิ่มตาราง "สรุปการเปลี่ยนแปลงสำคัญ" ท้าย Change Log เพื่อให้เห็นภาพรวม:

| Column | คำอธิบาย |
|--------|---------|
| ช่วง Version | ช่วง version ที่เปลี่ยนแปลง (เช่น 0.1 → 0.3) |
| การเปลี่ยนแปลง | สรุปสิ่งที่เปลี่ยน |
| ผลกระทบ | ระบุ component ที่ได้รับผลกระทบ |

## 15. Best Practice

- ทุก screen ต้องมี mockup หรือ wireframe อ้างอิง (ASCII mockup inline, Figma, หรือ HTML)
- Validation ต้องทำทั้ง client-side (UX) และ server-side (security)
- Error message ต้องดึงจาก Lookup Master (centralized) เพื่อให้จัดการได้ง่าย
- Loading state ต้องแสดงให้ user รู้ว่าระบบกำลังทำงาน
- Confirmation dialog ต้องมีก่อน action ที่ไม่สามารถ undo ได้ (เช่น Delete, Submit, Approve)
- Soft Delete แทน Hard Delete เพื่อรักษาประวัติข้อมูล
- Audit fields (Created/Updated Date/By) ต้องมีในทุกตารางที่มีการ CRUD
- Popup ต้อง refresh ตารางหน้าจอหลักหลังจาก save/delete สำเร็จ
- Decision tree (text-based) ใช้อธิบาย business logic ที่ซับซ้อน
- Navigation flow diagram ควรมีทั้ง text-based tree และ mermaid flowchart
