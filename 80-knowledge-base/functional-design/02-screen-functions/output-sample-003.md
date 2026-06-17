# Screen Functions — Output Sample (Consent Management)

> ตัวอย่างเอกสาร Functional Specification สำหรับหน้าจอจัดการความยินยอม (Consent Management)

---

# CMP1010 — Create Customer Consent

## 1. Overview

| รายการ | รายละเอียด |
| --- | --- |
| Function ID | CMP1010 |
| Function Name | Create Customer Consent |
| Category | Screen |
| Screen Type | Search & Action Form |
| Description | หน้าจอสำหรับค้นหาข้อมูลลูกค้าและสถานะความยินยอม (Consent) ข้ามทุกสาขา โดยดึงข้อมูลจากระบบหลัก |
| Actor / User Role | Branch User, Branch Admin |
| Related Screen IDs | CON-0020, CON-0030, CON-0100 |
| Source Reference | Consent Management Module |
| Version | 1.5 |
| Created By | BA Team (17-Apr-2026) |
| Updated By | BA Team (17-May-2026) |

## 2. Business Purpose

ให้ผู้ใช้งานสาขาสามารถค้นหาข้อมูลลูกค้าและตรวจสอบสถานะความยินยอม พร้อมทั้งสร้างหรือแก้ไขความยินยอมผ่านระบบได้

## 3. Screen Overview

| รายการ | รายละเอียด |
| --- | --- |
| Screen Name | Customer Consent Search |
| Menu Path | Main Menu > Consent Management |
| Navigation Inbound | Main Menu → เลือกเมนู Consent Management |
| Navigation Outbound | Click "Create Consent" → เลือกวิธีการสร้างความยินยอม / Click "Edit Consent" → ขอแก้ไขความยินยอม |
| Preconditions | ผู้ใช้ login สำเร็จ และมีสิทธิ์เข้าถึงหน้าจอ Consent Management |
| Postconditions | แสดงผลการค้นหาลูกค้าและสถานะความยินยอม |

### Related Screens

| Screen ID | Screen Name | Description |
| --- | --- | --- |
| CON-0020 | Collection via Paper | ค้นหาลูกค้าและบันทึกความยินยอมจากเอกสาร |
| CON-0030 | Collection via SMS/Email | ค้นหาลูกค้าและส่ง SMS หรือ Email เพื่อขอความยินยอม |
| CON-0100 | Edit Consent | ค้นหาลูกค้าและส่งคำขอแก้ไขความยินยอมไปยังเจ้าหน้าที่ |

### Screen Flow

- Path:

```text
Main Menu
  └── ตรวจสอบและสร้าง/แก้ไขความยินยอม (CMP1010)
        ├── Click "Create Consent" → เลือกวิธีการสร้างความยินยอม
        └── Click "Edit Consent"   → ขอแก้ไขความยินยอม
```

- Outline Diagram:

```mermaid
flowchart LR

    A[Menu]

    B[Create Customer Consent for Dealer]

    C[Select Collection Method]

    D[Request Edit Consents for Dealer]

    E[Another Screen]

    %% Main navigation
    A -->|Clicked link "Create Customer consent for dealer"| B
    B -->|Clicked link "Menu"| A

    %% Create flow
    B -->|Clicked link "Create Consent"| C

    %% Edit flow
    B -->|Clicked link "Edit consent"| D

    %% External navigation
    C --> E

    %% Styling (optional)
    style A stroke-dasharray: 5 5
    style D stroke-dasharray: 5 5
    style C stroke-dasharray: 5 5
    style E stroke-dasharray: 5 5
    style B fill:#EAD7A4,stroke:#333

```


## 4. Mockup / UI Layout

| รายการ | รายละเอียด |
| --- | --- |
| Mockup Reference | — |
| Layout Description | 4 ส่วน: Header (Session Data), Search Criteria, Action Buttons, Result Table |

Screen Image:

+------------------------------------------------------------------------------------------------------------------------------------------------------+
| [NISSAN] Consent Management System                                              Dealer: SIAM NISSAN SALES | Branch: LADPRAO                     |
|                                                                                  User ID: 37228  Name: Chatpong Wongkanya   [Logout]              |
+------------------------------------------------------------------------------------------------------------------------------------------------------+
| Menu >> ตรวจสอบและสร้าง/แก้ไขความยินยอม                                                                                                             |
+------------------------------------------------------------------------------------------------------------------------------------------------------+

+------------------------------------------------------------------------------------------------------------------------------------------------------+
| 🔍 เงื่อนไขในการค้นหา                                                                                                                                |
+------------------------------------------------------------------------------------------------------------------------------------------------------+
| รหัสผู้ติดต่อ : [___________]     ชื่อ : [_____________]     นามสกุล : [_____________]                                          |
| เบอร์โทรศัพท์ : [___________]     E-Mail : [_____________]                                                                       |
|                                                                                                                                  |
| ประเภทการให้ความยินยอม : [___________ ▼ ]                                                                                         |
|                                                                                                                                  |
| วันที่เริ่ม : [ 01/03/2020 ]     ถึงวันที่ : [ 30/04/2020 ]                                                                       |
|                                                                                                                                  |
| สถานะการให้ความยินยอม : [___________ ▼ ]                                                                                         |
|                                                                                                                                  |
| วันที่บันทึก : [___________]     ถึงวันที่ : [___________]                                                                       |
|                                                                                                                                  |
|                                      [ Search ]   [ Clear ]                                                                      |
+------------------------------------------------------------------------------------------------------------------------------------------------------+

+----+----------------+----------------+----------------------+------------------+-------------------+-------------------+-------------------+-------------------+
| No | รหัสลูกค้า     | ประเภทลูกค้า    | ชื่อ-นามสกุล         | Passport/ID      | เบอร์โทรศัพท์      | E-Mail            | สถานะ             | ...               |
+----+----------------+----------------+----------------------+------------------+-------------------+-------------------+-------------------+-------------------+
| 1  | INV-000001     | ลูกค้าทั่วไป    | สมชาย ใจดี            | 1234567890       | 081-234-5678      | test@gmail.com    | ยินยอม           | ...               |
| 2  | INV-000002     | ลูกค้าทั่วไป    | สมหญิง สวยงาม        | 2234567890       | 082-345-6789      | test2@gmail.com   | ไม่ยินยอม        | ...               |
| 3  | INV-000003     | ลูกค้าทั่วไป    | John Doe             | 3234567890       | 083-456-7890      | john@gmail.com    | ยินยอม           | ...               |
| .. | ...            | ...            | ...                  | ...              | ...               | ...               | ...               | ...               |
+----+----------------+----------------+----------------------+------------------+-------------------+-------------------+-------------------+-------------------+

(Scroll for more columns → Consent Date / Channel / Updated By / SMS Result / etc.)

+------------------------------------------------------------------------------------------------------------------------------------------------------+
| 17 เมษายน 2020 17:14:00                                                                                                                                |
+------------------------------------------------------------------------------------------------------------------------------------------------------+

## 5. Fields Definition

### 5.1 Header Section (Read-only Session Data)

| No | Field Name | Label | Type | Description |
| :---: | --- | --- | --- | --- |
| 1 | org_name | ชื่อองค์กร | Label | Read-only, แสดงชื่อองค์กรจาก session |
| 2 | branch_name | ชื่อสาขา | Label | Read-only, แสดงชื่อสาขาจาก session |
| 3 | navigator | เมนูนำทาง | Link | หน้าจอปัจจุบัน = Unlink, หน้าจออื่น = Link |
| 4 | user_id | รหัสผู้ใช้ | Label | Read-only, แสดงรหัสผู้ใช้จาก session |
| 5 | user_name | ชื่อผู้ใช้ | Label | Read-only, แสดงชื่อผู้ใช้จาก session |
| 6 | logout | ออกจากระบบ | Link | ออกจากระบบ |

### 5.2 Search Criteria Section

| No | Field Name | Label (TH/EN) | Type | Length | Required | Default | Description |
| :---: | --- | --- | --- | --- | --- | --- | --- |
| 7 | customer_id | รหัสลูกค้า / Customer ID | Textbox | 15 | N | ว่าง | กรอกโดยตรง |
| 8 | first_name | ชื่อ / First Name | Textbox | 60 | N | ว่าง | กรอกโดยตรง |
| 9 | last_name | นามสกุล / Last Name | Textbox | 60 | N | ว่าง | กรอกโดยตรง |
| 10 | id_card | เลขบัตรประจำตัว / ID Card | Textbox | 13 | N | ว่าง | กรอกโดยตรง |
| 11 | mobile | เบอร์มือถือ / Mobile | Textbox | 10 | N | ว่าง | กรอกโดยตรง |
| 12 | email | อีเมล / E-Mail | Textbox | 40 | N | ว่าง | กรอกโดยตรง |
| 13 | consent_status | สถานะความยินยอม / Consent Status | Combobox | — | N | ว่าง | ดึงจาก Lookup Master (KEY='07') เรียงตาม VALUE1 ASC |
| 14 | customer_date_from | วันที่เริ่ม — ประวัติลูกค้า | Date Picker | — | N | ว่าง | Calendar popup, Format: dd/mm/yyyy |
| 15 | customer_date_to | ถึงวันที่ — ประวัติลูกค้า | Date Picker | — | N | ว่าง | Calendar popup, Format: dd/mm/yyyy |
| 16 | consent_date_from | วันที่เริ่ม — ประวัติความยินยอม | Date Picker | — | N | ว่าง | Calendar popup, Format: dd/mm/yyyy |
| 17 | consent_date_to | ถึงวันที่ — ประวัติความยินยอม | Date Picker | — | N | ว่าง | Calendar popup, Format: dd/mm/yyyy |

### 5.3 Result Table

| No | Field Name | Label | Type | Data Source | Format / Description |
| :---: | --- | --- | --- | --- | --- |
| 21 | row_no | ลำดับ | Label | — | Running number |
| 22 | customer_id | รหัสลูกค้า | Label | Customer Master | PersonID |
| 23 | customer_type | ประเภทลูกค้า | Label | Customer Master | 1=ลูกค้ามุ่งหวัง, 2=ลูกค้า |
| 24 | full_name | ชื่อ-นามสกุล | Label | Customer Master | FirstName + "  " + LastName |
| 25 | id_card | เลขบัตรประจำตัว | Label | Customer Master | Format: X-XXXX-XXXXX-XX-X |
| 26 | mobile | เบอร์มือถือ | Label | Customer Master | Format: XXX-XXX-XXXX |
| 27 | email | อีเมล | Label | Customer Master | — |
| 28 | consent_status | สถานะความยินยอม | Label | Consent Record (A) | ไม่พบ = "ยังไม่ได้รับคำตอบ", พบ = "ยินยอม/ไม่ยินยอม" หรือ "ได้รับคำตอบ" |
| 29 | consent_action | สร้าง/แก้ไขความยินยอม | Link | Consent Record | ดู Logic ใน Process Description |
| 30 | registered_by | บันทึกประวัติลูกค้า โดย | Label | Customer Master + User Master (C) | Format: UserID + ":" + FirstName + "  " + LastName |
| 31 | register_date | วันที่บันทึกประวัติลูกค้า | Label | Customer Master | Format: dd/mm/yyyy HH:MM |
| 32 | updated_by | แก้ไขประวัติลูกค้าล่าสุดโดย | Label | Customer Master + User Master (D) | Format: UserID + ":" + FirstName + "  " + LastName |
| 33 | update_date | วันที่แก้ไขประวัติลูกค้าล่าสุด | Label | Customer Master | Format: dd/mm/yyyy HH:MM |
| 34 | consent_registered_by | บันทึกประวัติความยินยอม โดย | Label | Consent Record (A) + User Master (E) | Format: UserID + ":" + FirstName + "  " + LastName |
| 35 | consent_register_date | วันที่บันทึกประวัติความยินยอม | Label | Consent Record (A) | Format: dd/mm/yyyy HH:MM |
| 36 | consent_updated_by | แก้ไขประวัติความยินยอมล่าสุดโดย | Label | Consent Record (A) + User Master (F) | Format: UserID + ":" + FirstName + "  " + LastName |
| 37 | consent_update_date | วันที่แก้ไขประวัติความยินยอมล่าสุด | Label | Consent Record (A) | Format: dd/mm/yyyy HH:MM |
| 38 | consent_detail | รายละเอียด | Popup Link | Consent Record (A) | เปิด Consent Details Popup |
| 39 | current_datetime | วันเวลาปัจจุบัน | Label | — | Format: dd/mm/yyyy HH:MM |
| 40 | sms_result | ผลการส่ง SMS โดยสำนักงานใหญ่ | Label | Notification Log | ดู SMS Result Logic |

## 6. Commands / Actions

| No | Command | Type | Default State | Trigger Condition | System Response |
| :---: | --- | --- | --- | --- | --- |
| 18 | Search | Button | Enable | กดปุ่ม | ค้นหาข้อมูลตามเงื่อนไข |
| 19 | Clear | Button | Enable | กดปุ่ม | ล้างเงื่อนไขค้นหาทั้งหมด |
| 20 | Export | Button | **Disable** | พบข้อมูลจากการค้นหา | เปิดใช้งานเมื่อพบข้อมูล |

## 7. Screen Behavior

### 7.1 Initial Screen (onLoad)

- แสดงหน้าจอพร้อม Session data (Organization Name, Branch Name, User ID, User Name)
- Search, Clear: **Enable**
- Export: **Disable**

### 7.2 Click "Menu"

- Redirect ไปยัง Main Menu

### 7.3 Click "Logout"

- ล้าง login session และปิดโปรแกรม

### 7.4 Click Date Picker (Item 14–17)

- แสดง Calendar Popup

### 7.5 Click "Search" (Item 18)

#### 7.5.1 Validate — ต้องกรอกอย่างน้อย 1 เงื่อนไข

- ตรวจสอบ Item 7–17 ทั้งหมด
- **ถ้าว่างทั้งหมด:** แสดง Error "กรุณาใส่เงื่อนไขในการค้นหา อย่างน้อย 1 เงื่อนไข"
  - Error source: Lookup Master (KEY='09', VALUE2='ERR0005')
- **ถ้ามีข้อมูลอย่างน้อย 1 ช่อง:** ดำเนินการต่อ

#### 7.5.2 Validate — ช่วงวันที่ประวัติลูกค้า (Item 14 & 15)

- ตรวจสอบเมื่อกรอกทั้ง 2 ช่อง
- **ถ้า Item 15 < Item 14:** แสดง Error "วันที่สิ้นสุด ต้องมากกว่า หรือ เท่ากับวันที่เริ่ม"
  - Error source: Lookup Master (KEY='09', VALUE2='ERR0006')

#### 7.5.3 Validate — ช่วงวันที่ประวัติความยินยอม (Item 16 & 17)

- ตรวจสอบเมื่อกรอกทั้ง 2 ช่อง
- **ถ้า Item 17 < Item 16:** แสดง Error "วันที่สิ้นสุด ต้องมากกว่า หรือ เท่ากับวันที่เริ่ม"
  - Error source: Lookup Master (KEY='09', VALUE2='ERR0007')

**ถ้าผ่าน validation ทั้งหมด → ดำเนินการ Process 7.8 (ดึงข้อมูลแสดงบนหน้าจอ)**

### 7.6 Click "Clear" (Item 19)

- ล้าง Item 7–17 ทั้งหมด แล้ว refresh หน้าจอกลับสู่ Initial state

### 7.7 โหลดข้อมูล Consent Status Dropdown (Item 13)

- Query: Lookup Master WHERE KEY = '07'
- Sort: VALUE1 ASC

### 7.8 ดึงข้อมูลแสดงบนหน้าจอ (Result Table)

#### Base Query

```text
Table: Customer Master
  JOIN Consent Record (A) ON CustomerID = ConsentCustomerID AND ConsentStatus = 'Active'
  JOIN Consent Record (B) ON ConsentCentralID AND ConsentStatus = 'Waiting approval'
  JOIN User Master (C) ON RegisterByUser AND GroupCode = '002'
  JOIN User Master (D) ON UpdateByUser AND GroupCode = '002'
  JOIN User Master (E) ON Consent.RegisterUser AND GroupCode = '002'
  JOIN User Master (F) ON Consent.UpdateUser AND GroupCode = '002'

WHERE Customer Master.Source1 = 'PRIMARY'
  AND Customer Master.Source2 = Session.OrgCode
  AND Customer Master.Status = 'Active'
```

#### Optional Search Filters

| Item | Field | Operator | Value |
| --- | --- | --- | --- |
| 7 รหัสลูกค้า | Customer Master.PersonID | = | Input value |
| 8 ชื่อ | Customer Master.FirstName | LIKE | Input + '%' |
| 9 นามสกุล | Customer Master.LastName | LIKE | Input + '%' |
| 10 เลขบัตรประจำตัว | Customer Master.IDCard | = | Input value |
| 11 เบอร์มือถือ | Customer Master.PhoneNumber | = | Input value |
| 12 อีเมล | Customer Master.Email | = | Input value |
| 13 สถานะ = "ยังไม่ได้รับคำตอบ" | Consent Record (A).ConsentCentralID | IS NOT NULL | — |
| 13 สถานะ = "ยินยอม/ไม่ยินยอม" | Consent Record (A).ConsentCentralID | IS NULL | — |
| 14 วันที่เริ่ม (ลูกค้า) | Customer Master.UpdateDate | >= | Input date |
| 15 ถึงวันที่ (ลูกค้า) | Customer Master.UpdateDate | <= | Input date |
| 16 วันที่เริ่ม (ความยินยอม) | Consent Record (A).UpdateDate | >= | Input date |
| 17 ถึงวันที่ (ความยินยอม) | Consent Record (A).UpdateDate | <= | Input date |

#### Sort Order

1. Customer Master.UpdateDate — **DESC**
2. Consent Record (A).UpdateDate — **DESC**

#### 7.8.1 กรณีไม่พบข้อมูล

- แสดง Error: "ไม่พบข้อมูลประวัติลูกค้าและประวัติความยินยอม"
  - Error source: Lookup Master (KEY='09', VALUE2='ERR0008')
- Export button: **Disable**

#### 7.8.2 กรณีพบข้อมูล

- แสดงข้อมูลในตารางตาม Fields Definition
- Export button: **Enable**

## 8. Business Rules

### 8.1 Item 29 — สร้าง/แก้ไขความยินยอม Logic

```text
ตรวจสอบ Consent data ใน Consent Record (A)
│
├── ไม่พบ Consent data
│   └── แสดง "สร้างความยินยอม" → Link ไปหน้าเลือกวิธีการสร้าง
│
└── พบ Consent data
    │
    ├── Customer Type ไม่ตรงกัน (Customer Master vs Consent Record)
    │   └── แสดง "สร้างความยินยอม" → Link ไปหน้าเลือกวิธีการสร้าง
    │
    └── Customer Type ตรงกัน
        │
        ├── พบ Consent data ใน Consent Record (B) [Waiting approval]
        │   └── แสดง "ขอแก้ไขความยินยอม" (ข้อความเท่านั้น ไม่มี link)
        │
        └── ไม่พบ Consent data ใน Consent Record (B)
            │
            ├── RegisterUser ≠ 'By SMS' และ ≠ 'By Email'
            │   └── แสดง "แก้ไขความยินยอม" → Link ไปหน้า Request Edit
            │
            ├── RegisterUser = 'By SMS' หรือ 'By Email'
            │   └── แสดง "บันทึกโดยลูกค้า" (ข้อความเท่านั้น)
            │
            └── RegisterByPGM = 'CMP1040'
                └── แสดง "copy from Central" (ข้อความเท่านั้น)
```

### 8.2 Item 40 — ผลการส่ง SMS โดยสำนักงานใหญ่ (SMS Result Logic)

**Query:** Notification Log

```text
WHERE MethodType = '2' (SMS)
  AND ProcessType = '1' (Insert)
  AND RegisterByProgram = 'BATCH0002'
  AND CustomerID = Customer Master.CustomerID
  AND FirstName = Customer Master.FirstName
  AND LastName = Customer Master.LastName
```

**Display Logic:**

| กรณี | แสดงผล |
| --- | --- |
| พบ record + มี UpdateConsentDateTime | "ได้รับข้อมูลความยินยอมแล้ว" |
| พบ record + ไม่มี UpdateConsentDateTime | "ยังไม่ได้รับคำตอบ วันที่ส่ง SMS : " + SendDateTime |
| ไม่พบ record | "-" |

## 9. Message List

### Error Messages

| Message ID | Trigger | Message (TH) |
| --- | --- | --- |
| ERR0005 | ไม่กรอกเงื่อนไขค้นหาเลย | กรุณาใส่เงื่อนไขในการค้นหา อย่างน้อย 1 เงื่อนไข |
| ERR0006 | วันที่สิ้นสุด (ลูกค้า) < วันที่เริ่ม | วันที่สิ้นสุด ต้องมากกว่า หรือ เท่ากับวันที่เริ่ม |
| ERR0007 | วันที่สิ้นสุด (ความยินยอม) < วันที่เริ่ม | วันที่สิ้นสุด ต้องมากกว่า หรือ เท่ากับวันที่เริ่ม |
| ERR0008 | ไม่พบข้อมูลจากการค้นหา | ไม่พบข้อมูลประวัติลูกค้าและประวัติความยินยอม |

## 10. Consent Details Popup (Item 38)

### Popup Items

| No | Field Name | Label | Data Source | Description |
| :---: | --- | --- | --- | --- |
| 10-1 | title | หัวข้อ | Customer Master | รหัสลูกค้า : ชื่อ-นามสกุล |
| 10-2 | row_no | ลำดับ | — | Running number |
| 10-3 | customer_id | รหัสลูกค้า | Customer Master | PersonID |
| 10-4 | customer_type | ประเภทลูกค้า | Customer Master | ลูกค้ามุ่งหวัง / ลูกค้า |
| 10-5 | full_name | ชื่อ-นามสกุล | Customer Master | FirstName + LastName |
| 10-6 | id_card | เลขบัตรประจำตัว | Customer Master | IDCard |
| 10-7 | mobile | เบอร์มือถือ | Customer Master | PhoneNumber |
| 10-8 | email | อีเมล | Customer Master | Email |
| 10-9 | revision | Revision | Consent Record (A) | Consent revision number |
| 10-10 | effective_date | เริ่มใช้งาน / Effective Date | Consent Record (A) | StartDate |
| 10-11 | expiration_date | สิ้นสุดการใช้งาน / Expiration Date | Consent Record (A) | EndDate |
| 10-12 | consent_status | สถานะ Consent | Consent Record (A) | ConsentStatus |
| 10-13 | reject_remark | Remark (ของ Reject) | Consent Record (A) | RemarkOfReject |
| 10-14 | close | ปิด [X] | — | ปิด Popup กลับหน้าจอหลัก |

### Popup Query

```text
WHERE Customer Master.Source1 = 'PRIMARY'
  AND Customer Master.Status = 'Active'
  AND Customer Master.PersonID = [Selected Row Item 22]
  AND Customer Master.CustomerID = Consent Record (A).CustomerID
  AND Consent Record (A).ConsentStatus = 'Active'

ORDER BY Revision DESC
```

## 11. Database Tables Reference

| Table Name | Alias | Description |
| --- | --- | --- |
| Customer Master | — | ข้อมูลพื้นฐานลูกค้า |
| Consent Record | (A) | ข้อมูลความยินยอมที่ Active (ConsentStatus = 'Active') |
| Consent Record | (B) | ข้อมูลความยินยอมที่รอการอนุมัติ (ConsentStatus = 'Waiting approval') |
| User Master | (C) | User master สำหรับ Customer Register By |
| User Master | (D) | User master สำหรับ Customer Update By |
| User Master | (E) | User master สำหรับ Consent Register By |
| User Master | (F) | User master สำหรับ Consent Update By |
| Lookup Master | — | Master table สำหรับ dropdown, error messages |
| Notification Log | — | Log การส่ง Email/SMS |

## 12. Notes / Assumptions

| ประเภท | รายละเอียด | ผลกระทบ |
| --- | --- | --- |
| Assumption | ข้อมูลลูกค้าดึงจากระบบหลักเท่านั้น | จำกัดขอบเขตข้อมูลที่แสดง |
| Assumption | สถานะ Consent ดึงจาก Lookup Master | ต้อง maintain ข้อมูล Lookup ให้เป็นปัจจุบัน |

## Change Log

| Version | Date | Author | Change Type | Description | Remark |
| --- | --- | --- | --- | --- | --- |
| 0.1 | 17-Apr-2026 | BA Team | Created | สร้างเอกสารครั้งแรก | — |
| 0.2 | 29-Apr-2026 | BA Team | Updated | แก้ไขทุก sheet | — |
| 0.3 | 05-May-2026 | BA Team | Updated | แก้ไข wording บนหน้าจอตาม feedback | — |
| 0.4 | 09-Jun-2026 | BA Team | Updated | เปลี่ยน Remark data source เป็น "Remark of Reject" | — |
| 0.5 | 09-Jul-2026 | BA Team | Updated | เพิ่มเงื่อนไขแสดง message ของ สร้าง/แก้ไขความยินยอม | — |
| 0.6 | 04-Aug-2026 | BA Team | Updated | เพิ่มเงื่อนไขแสดง message ของ สร้าง/แก้ไขความยินยอม (เพิ่มเติม) | — |
| 1.0 | 12-Apr-2026 | BA Team | CR004 | เพิ่ม Customer Type field | — |
| 1.1 | 22-Apr-2026 | BA Team | CR004 | แก้ไข wording ของ Customer Type และ สถานะความยินยอม | — |
| 1.2 | 10-May-2026 | BA Team | CR004 | เพิ่ม field "ผลการส่ง SMS โดยสำนักงานใหญ่" | — |
| 1.3 | 12-May-2026 | BA Team | CR004 | แก้ไข wording field SMS | — |
| 1.4 | 13-May-2026 | BA Team | CR004 | แก้ไขเงื่อนไขการแสดงผล field SMS | — |
| 1.5 | 17-May-2026 | BA Team | CR004 | แก้ไข SMS query — ลบ CustomerID, เพิ่ม FirstName/LastName matching | — |

### สรุปการเปลี่ยนแปลงสำคัญ

| ช่วง Version | การเปลี่ยนแปลง | ผลกระทบ |
| --- | --- | --- |
| 0.1 → 0.3 | แก้ไข wording ตาม feedback, แก้ไข error message logic | UI wording + error handling |
| 0.4 | เปลี่ยน Remark field เป็น RemarkOfReject | Consent Details Popup |
| 0.5 → 0.6 | เพิ่มเงื่อนไข Item 29 (สร้าง/แก้ไขความยินยอม logic) | Business logic ของ link display |
| 1.0 → 1.1 (CR004) | เพิ่ม Customer Type field + แก้ไข wording | Item 23, 10-4, สถานะความยินยอม |
| 1.2 → 1.5 (CR004) | เพิ่ม SMS Result field (Item 40) + ปรับ query condition | เพิ่ม column ในตาราง + SMS log query |
