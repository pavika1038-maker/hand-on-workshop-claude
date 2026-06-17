# Report Functions — องค์ความรู้หลัก

## 1. คำจำกัดความ

Report Functions คือ function ที่แสดงข้อมูลเชิงวิเคราะห์หรือสรุปผล โดยเป็น read-only ไม่มีการแก้ไขข้อมูล รายงานอาจเป็นแบบ on-screen, PDF, หรือ Excel

## 2. ประเภทของ Report

| ประเภท | คำอธิบาย | ตัวอย่าง |
|--------|---------|---------|
| Transaction Detail | รายการธุรกรรมรายเอกสาร พร้อมยอดรวมตามมิติ | Sales Transaction for GL Posting |
| Monitoring / Detail List | รายการข้อมูลพร้อมสถานะ สำหรับติดตาม | Leave Monitoring Report |
| Summary | สรุปยอดตามมิติ (เวลา, แผนก, ประเภท) | Leave Summary Report |
| Dashboard | แสดงภาพรวมแบบ interactive (chart, KPI) | Usage Dashboard |
| Export | ส่งออกข้อมูลเป็นไฟล์ (Excel, PDF) | Leave Data Export |

## 3. โครงสร้างเอกสาร Report Function

เอกสาร Report Function มี 2 รูปแบบหลัก:

### 3.1 รูปแบบย่อ (Internal / Simple Report)

เหมาะสำหรับรายงานภายในที่ไม่ซับซ้อน เช่น Summary Report

| Section | คำอธิบาย | บังคับ |
|---------|---------|--------|
| 1. Overview | ข้อมูลทั่วไป (ID, Name, Type, Actor, Frequency) | Y |
| 2. Business Purpose | เหตุผลทางธุรกิจ | Y |
| 3. Report Parameters | Filter/Parameter ทั้งหมด | Y |
| 4. Report Layout | โครงสร้าง Header/Summary/Detail/Footer + Columns Definition | Y |
| 5. Mockup / UI Layout | ASCII mockup ของหน้าจอรายงาน | Y |
| 6. Commands / Actions | ปุ่ม Execute, Export, etc. | Y |
| 7. Business Rules | กฎเกณฑ์ทางธุรกิจ | Y |
| 8. Notes / Assumptions | สมมติฐาน | Y |
| Change Log | ประวัติการเปลี่ยนแปลง | Y |

### 3.2 รูปแบบเต็ม (Formal / Software Report Specification)

เหมาะสำหรับรายงานที่ต้องส่งมอบลูกค้าหรือรายงานที่ซับซ้อน เช่น Transaction Detail Report

| Section | คำอธิบาย | บังคับ |
|---------|---------|--------|
| Document Information | Metadata (Title, Purpose, System, Company, Version) | Y |
| Approval & Sign-off | Certification, Review By, Approve By | Y |
| Change History | ประวัติการเปลี่ยนแปลง (Date, Author, Version, Reason, Reference Section) | Y |
| 1. Introduction | Overview, Business Background, Purpose | Y |
| 2. Overview Scope | ระบบที่ทำงาน + หมายเหตุด้าน performance/scope | Y |
| 3. Summary Requirements | Report Invocation, Display Options, Filter Conditions, Sorting Logic, Output | Y |
| 4. Business Flow | ผลกระทบต่อ business flow (หรือระบุว่าไม่มี) | Y |
| 5. Menu & Function Overview | Menu path + function type | Y |
| 6. Functional Requirement Details | Filter screen spec + Filter fields specification | Y |
| 7. Report Requirement (PDF/Excel) | Header, Detail, Footer + ASCII mockup ของ output | Y |
| Notes / Assumptions | สมมติฐาน | เมื่อมี |

## 4. หลักการออกแบบ

| หลักการ | คำอธิบาย |
|---------|---------|
| Read-only | Report ไม่มีการเขียนข้อมูล |
| แยกจาก Transaction | Report query ต้องไม่กระทบ performance ของ transaction |
| Filter ชัดเจน | ทุก report ต้องมี parameter/filter ที่ชัดเจน พร้อม default value |
| Permission-based | Filter บางตัว (เช่น Branch) ต้องแสดงเฉพาะข้อมูลที่ผู้ใช้มีสิทธิ์ |
| Sorting & Paging | รองรับการเรียงลำดับและแบ่งหน้า |
| Export | รองรับ export เป็น Excel และ PDF เป็นอย่างน้อย |
| Performance Note | ระบุหมายเหตุด้าน performance เมื่อรายงานดึงข้อมูลจากหลาย table |

## 5. โครงสร้างมาตรฐานของ Report

### 5.1 Filter Screen (หน้าจอกำหนดเงื่อนไข)

```text
┌─────────────────────────────────────┐
│  Report Title                       │
├─────────────────────────────────────┤
│  Filter Field 1 : [ Dropdown ▼ ]   │
│  Filter Field 2 : [ Dropdown ▼ ]   │
│  Filter Field 3 : [ Dropdown ▼ ]   │
│  Sort By        : [ Dropdown ▼ ]   │
│  From Date      : [ Date 📅 ]      │
│  To Date        : [ Date 📅 ]      │
│  [ ] Include Cancelled              │
│                                     │
│  [ Preview ] [ Excel ] [ Editor ]   │
└─────────────────────────────────────┘
```

### 5.2 Report Output (PDF/Excel)

```text
┌─────────────────────────────────────┐
│  Report Header                      │
│  (Company, Report Name, Print Info) │
├─────────────────────────────────────┤
│  Filter Criteria Summary            │
│  (เงื่อนไขที่ใช้กรอง)               │
├─────────────────────────────────────┤
│  Detail Table                       │
│  (ตารางข้อมูลหลัก รายเอกสาร)       │
├─────────────────────────────────────┤
│  Summary Per Group                  │
│  (สรุปตามสาขา/แผนก/ประเภท)         │
├─────────────────────────────────────┤
│  Grand Total                        │
│  (สรุปรวมทั้งหมด)                   │
├─────────────────────────────────────┤
│  Footer                            │
│  (Remark, Page No.)                │
└─────────────────────────────────────┘
```

## 6. Filter Pattern

### 6.1 Filter Field Types

| Type | คำอธิบาย | ตัวอย่าง |
|------|---------|---------|
| Dropdown (Single) | เลือกได้ 1 ค่า | Sort By, Document Type |
| Dropdown (Multi-select) | เลือกได้หลายค่า | Branch Code, Module |
| Date Range | ช่วงวันที่ (From – To) | Document Date Range |
| Toggle / Checkbox | เปิด/ปิด option | Include Cancelled Documents |
| Textbox | กรอกค่าอิสระ | Customer Code, Document No. |

### 6.2 Filter Behavior

| Behavior | คำอธิบาย |
|----------|---------|
| Permission-based | Dropdown แสดงเฉพาะค่าที่ผู้ใช้มีสิทธิ์ (เช่น Branch ที่มีสิทธิ์เข้าถึง) |
| Default Value | ทุก filter ต้องมี default value ที่เหมาะสม (เช่น Current Date, All) |
| Mandatory | ระบุ filter ที่บังคับกรอก vs ไม่บังคับ |
| Conditional | บาง filter แสดงเฉพาะเมื่อเลือกค่าบางอย่าง (เช่น Department แสดงเฉพาะ Other AR) |

## 7. Report Output Pattern

### 7.1 Header Section

| Component | คำอธิบาย |
|-----------|---------|
| Company Name | ชื่อบริษัท |
| Report Name | ชื่อรายงาน |
| Print Date/Time | วันเวลาที่พิมพ์ |
| Printed By | ผู้พิมพ์ |
| Filter Criteria | เงื่อนไขที่ใช้กรอง (Branch, Module, Date Range, etc.) |
| Page No. | เลขหน้า |

### 7.2 Detail Section

| Component | คำอธิบาย |
|-----------|---------|
| Row Number | ลำดับ running number |
| Document Info | เลขที่เอกสาร, วันที่, เลขที่อ้างอิง |
| Entity Info | รหัส/ชื่อลูกค้า, สาขา, แผนก |
| Amount Breakdown | แยกยอดตามหมวด (Parts, Service, VAT, etc.) |
| Status Flags | สถานะเอกสาร (Cancel Flag, Module) |

### 7.3 Summary Section

รายงานที่มีข้อมูลหลายกลุ่มควรมี summary หลายระดับ:

| ระดับ | คำอธิบาย | ตัวอย่าง |
|-------|---------|---------|
| Per Group | สรุปตามกลุ่ม (สาขา, แผนก, ประเภทเอกสาร) | สรุปตามสาขา: จำนวน, ยอดขาย, ส่วนลด, ภาษี, รวม |
| Grand Total | สรุปรวมทั้งหมด | รวมทุกสาขา: จำนวน, ยอดขาย, ส่วนลด, ภาษี, รวม |

### 7.4 Footer Section

| Component | คำอธิบาย |
|-----------|---------|
| Remark | หมายเหตุ (เช่น ตัวเลขสีแดง = รายการยกเลิก) |
| Page Number | เลขหน้า / จำนวนหน้าทั้งหมด |

## 8. Sorting Logic Pattern

รายงานที่มีข้อมูลหลายมิติควรมี sorting logic หลายระดับ:

| ระดับ | คำอธิบาย | ตัวอย่าง |
|-------|---------|---------|
| Level 1 | กลุ่มหลัก | Branch Code |
| Level 2 | กลุ่มย่อย | ประเภทเอกสาร (Invoice → Debit → Credit) |
| Level 3 | ภายในกลุ่ม | วันที่เอกสาร |
| Level 4 | ภายในวันที่ | เลขที่เอกสาร |

ผู้ใช้อาจเลือก Sort By ได้จาก filter (เช่น เรียงตามเลขที่เอกสาร หรือ วันที่เอกสาร)

## 9. Export Pattern

| Format | คำอธิบาย | ปุ่ม |
|--------|---------|------|
| PDF Preview | แสดงตัวอย่างรายงานบนหน้าจอ | Preview / ตัวอย่างพิมพ์ |
| Excel | ส่งออกเป็นไฟล์ Excel | Export Excel |
| Editor | เปิดในโปรแกรมแก้ไข (ถ้ามี) | Editor |

## 10. Document Information & Approval Pattern

สำหรับรายงานแบบ Formal (Software Report Specification):

### 10.1 Document Information

| Field | คำอธิบาย |
|-------|---------|
| Document Title | ชื่อเอกสาร spec |
| Purpose | วัตถุประสงค์ของเอกสาร |
| System | ระบบที่รายงานทำงาน |
| Company | บริษัทเจ้าของระบบ |
| Version | เลข version ของเอกสาร |

### 10.2 Approval & Sign-off

| Component | คำอธิบาย |
|-----------|---------|
| Certification | ข้อความรับรองว่าสามารถพัฒนาได้ตามเอกสาร |
| Review By | รายชื่อผู้ review พร้อมระบุ role (Technical, Accounting, etc.) |
| Approve By | รายชื่อผู้อนุมัติพร้อมระบุ role |

### 10.3 Change History

| Column | คำอธิบาย |
|--------|---------|
| Date | วันที่เปลี่ยนแปลง |
| Author | ผู้เปลี่ยนแปลง |
| Version | เลข version |
| Reason for Change | เหตุผลที่เปลี่ยนแปลง (เช่น Updated based on review feedback) |
| Reference Section | section ที่ได้รับผลกระทบ |

## 11. Performance Consideration

| หัวข้อ | คำอธิบาย |
|--------|---------|
| Multi-table Query | รายงานที่ดึงข้อมูลจากหลาย table ต้องระบุหมายเหตุด้าน performance |
| Scope Boundary | ระบุชัดเจนว่า performance optimization อยู่ใน scope หรือไม่ |
| Data Volume | ประเมินปริมาณข้อมูลที่รายงานต้องประมวลผล |

## 12. Best Practice

- ทุก report ต้องมี mockup แสดง layout ทั้ง filter screen และ report output (PDF/Excel)
- Default filter ต้องกำหนดให้ report แสดงข้อมูลที่เป็นประโยชน์ทันที (ไม่ใช่ว่างเปล่า)
- Report ที่มีข้อมูลมากต้องรองรับ pagination
- ใช้ dedicated view หรือ reporting API แยกจาก transaction API
- Filter ที่เกี่ยวกับ organizational hierarchy (Branch, Department) ต้องแสดงตามสิทธิ์ผู้ใช้
- รายงานที่มีข้อมูลหลายกลุ่มต้องมี summary หลายระดับ (Per Group + Grand Total)
- ระบุ sorting logic ชัดเจนสำหรับ PDF output
- รายงานแบบ formal ต้องมี Document Information, Approval & Sign-off, และ Change History
- ระบุ Business Flow impact แม้ว่าจะไม่มีการเปลี่ยนแปลง (ระบุว่า "ไม่มีการเปลี่ยนแปลง")
- ระบุ performance note เมื่อรายงานดึงข้อมูลจากหลาย table
- แยก Filter Fields Specification เป็นตารางชัดเจน (No, Field, Type, Mandatory, Default)
