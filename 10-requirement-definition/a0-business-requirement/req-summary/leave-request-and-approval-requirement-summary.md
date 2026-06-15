# Requirement Summary Document
## หัวข้อ: ระบบบริหารการลาและการอนุมัติ (Leave Request and Approval)
### บริษัท ABC Company

> **เวอร์ชัน:** 1.0  
> **จัดทำโดย:** Requirement Summary Document Agent  
> **สถานะ:** Confirmed Baseline — พร้อมใช้เป็น input สำหรับ System Requirement  
> **อ้างอิงรอบ QA:** QA v1 (12/12 Closed) + QA v2 (6/6 Closed) + QA v3 (3/3 Closed)

---

## 1. วัตถุประสงค์ของเอกสาร

เอกสารนี้สรุป **business requirement ที่ยืนยันแล้ว** สำหรับระบบบริหารการลาและการอนุมัติ (Leave Request and Approval) ของ ABC Company โดยรวบรวมข้อมูลจากไฟล์ต้นทาง 5 ไฟล์ และคำตอบจาก QA 3 รอบรวม 21 รายการ (ทั้งหมด Closed)

**วัตถุประสงค์หลัก:** ใช้เป็น confirmed baseline สำหรับทีม BA/SA เพื่อเริ่มจัดทำ System Requirement Document ในขั้นถัดไป

---

## 2. หลักการสรุปและลำดับความสำคัญของแหล่งข้อมูล

| ลำดับ | แหล่งข้อมูล | บทบาท |
|-------|------------|-------|
| 1 (สูงสุด) | QA v3 Answers (M1–M3, ตอบโดย HR Manager) | Latest authority — ใช้แทนข้อมูลทุกชั้นที่ขัดแย้ง |
| 2 | QA v2 Answers (R1–R6, ตอบโดย HR Manager) | Authority หลักสำหรับ leave rules และ Outsource policy |
| 3 | QA v1 Answers (QA-H1–QA-L2, ตอบโดย Business User) | Authority สำหรับ scope, actors, workflow และ leave types |
| 4 | `Leave-Management-Request-and-Approval-Business-User.yaml` | Business requirements ต้นทางหลัก |
| 5 | `ABC-Compay-HR-Regularion.md`, `ABC-Company-Leave-Form.md`, `ABC-Leave-Workflow-Thai-QA.md`, `ระบบบริหารการลาและการอนุมัติ.md` | ข้อมูลอ้างอิงเสริม (ใช้เมื่อ QA ไม่ได้ cover) |

**กฎสำคัญ:** หาก source ขัดกัน ให้ยึด QA answer รอบล่าสุดที่ Closed เป็น authority หลักเสมอ

---

## 3. ภาพรวมที่ยืนยันแล้ว

### 3.1 วัตถุประสงค์ทางธุรกิจ
ลดความล่าช้าในการขอลาและอนุมัติ พร้อมให้พนักงานทุกประเภท (ประจำและ Outsource) ใช้งานระบบเดียวกันผ่าน Web เพื่อแทนที่กระบวนการด้วยเอกสาร / Excel นอกระบบ

> **แหล่งอ้างอิง:** `Leave-Management-Request-and-Approval-Business-User.yaml` → `overview.document_summary`

### 3.2 ขอบเขตที่ยืนยันแล้ว (In Scope)

| หัวข้อ | รายละเอียด | แหล่งอ้างอิง |
|--------|-----------|-------------|
| ตรวจสอบสิทธิ์วันลา | พนักงานดูวันลาคงเหลือแยกตามประเภทได้ด้วยตนเอง | BR-002, QA-H3 |
| ยื่นคำขอลา | พนักงานกรอกคำขอในระบบ ระบุประเภท วันที่ เหตุผล แนบเอกสาร | BR-003, Leave-Form.md |
| อนุมัติ / ปฏิเสธ | หัวหน้างานอนุมัติหรือ Reject ผ่านระบบ 1 ระดับ | BR-004, QA-H5 |
| ยกเลิกคำขอลา | Cancel flow 2 กรณี (Pending / Approved) พร้อม re-approve flow | R4 (QA v2), M1/M3 (QA v3) |
| แจ้งผลอัตโนมัติ | Email notification ทุก event | BR-005, R5 (QA v2) |
| ติดตามสถานะ | พนักงานตรวจสอบสถานะคำขอได้เอง | BR-006 |
| HR Monitoring | HR เห็นรายการคำขอทั้งหมดในระบบเดียว | BR-007 |
| รองรับ Outsource | พนักงาน Outsource ใช้ระบบเดียวกัน มี employee_type แยก | BR-008, R2, R3 (QA v2) |
| คืนวันลาอัตโนมัติ | เมื่อยกเลิก Approved สำเร็จ วันลาคืนอัตโนมัติ | NR1 (จาก R4) |
| Audit Trail (Phase 2) | เก็บประวัติการสร้าง แก้ไข อนุมัติคำขอลา | BR-009 |
| Report Export (Phase 2) | HR export รายงานตามช่วงเวลา/หน่วยงาน/ประเภทพนักงาน | BR-010 |

### 3.3 ขอบเขตที่ยืนยันแล้วว่าอยู่นอก scope (Out of Scope)

- OT / payroll integration เชิงลึก
- Mobile native application (Web responsive เพียงพอ)
- Multi-level approval เกิน 1 ระดับ (อยู่นอก Phase 1)

> **แหล่งอ้างอิง:** `Leave-Management-Request-and-Approval-Business-User.yaml` → `business_rules.out_of_scope`, QA-H5

### 3.4 สถาปัตยกรรมระดับ Business (ยืนยันแล้ว)

- ระบบใหม่ = **Web Application แยกต่างหาก** ที่พนักงานทุกคนเข้าถึงได้
- **Integrate กับ HRIS เดิม** เพื่อดึง master data — ไม่ replace HRIS ทั้งหมด
- HR ยังคงใช้ HRIS เดิมควบคู่ไป

> **แหล่งอ้างอิง:** QA-H6 (Business User)

---

## 4. ผู้เกี่ยวข้องหลัก (Actors)

| Actor | บทบาท | หมายเหตุ |
|-------|--------|---------|
| **พนักงานประจำ (Employee)** | ยื่นลา ตรวจสอบสิทธิ์ ติดตามสถานะ ยกเลิกคำขอ | มีสิทธิ์ลาครบ 7 ประเภท |
| **พนักงาน Outsource** | เหมือนพนักงานประจำ แต่สิทธิ์บางประเภทแตกต่าง | ลาคลอด/ทำหมัน/รับราชการ/อุปสมบท = ไม่มีสิทธิ์ |
| **หัวหน้างาน (Line Manager)** | Approve / Reject คำขอ, Re-approve การยกเลิก | 1 ระดับ — ไม่มี multi-level |
| **HR** | Monitor คำขอทั้งหมด, จัดการข้อมูล Outsource, รับ notification ทุก event | ใช้ระบบแทน Excel เดิม |
| **ระบบ (System)** | ส่ง Email notification, อัปเดตสถานะ, คืนวันลาอัตโนมัติ, SLA reminder | — |

> **แหล่งอ้างอิง:** `Leave-Management-Request-and-Approval-Business-User.yaml` → `overview.target_users`, R2, R5 (QA v2)

---

## 5. To-Be Workflow ที่ยืนยันแล้ว

### 5.1 Main Flow — ยื่นและอนุมัติคำขอลา

```
[พนักงาน] Login ผ่าน Web
    ↓
[พนักงาน] ตรวจสอบวันลาคงเหลือ (leave balance แยกตามประเภท)
    ↓
[พนักงาน] ยื่นคำขอลา (ระบุ: ประเภทการลา, วันที่, เหตุผล)
           └─ กรณีลาป่วย ≥ 3 วันทำการต่อเนื่อง: แนบใบรับรองแพทย์
    ↓
[ระบบ] บันทึกคำขอ (Status = Pending) + ส่ง Email แจ้ง หัวหน้างาน + HR
    ↓
[หัวหน้างาน] พิจารณา Approve หรือ Reject
              └─ กรณี Reject: ระบุเหตุผล (optional)
    ↓
[ระบบ] อัปเดตสถานะ (Approved / Rejected)
       + ส่ง Email แจ้ง พนักงาน + HR
    ↓
[HR] ติดตามสถานะ ตรวจสอบข้อมูล export report
```

### 5.2 Cancel Flow — ยกเลิกคำขอลา

**กรณี 1: คำขอสถานะ Pending**
```
[พนักงาน] ยกเลิกคำขอเอง (ไม่ต้องแจ้งหัวหน้า)
    ↓
[ระบบ] อัปเดตสถานะ → Cancelled ทันที
```

**กรณี 2: คำขอสถานะ Approved**
```
[พนักงาน] ส่งคำขอยกเลิก
    ↓
[ระบบ] อัปเดตสถานะ → Cancel Requested
       + ส่ง Email แจ้ง หัวหน้างาน (SLA: 1 วันทำการ)
    ↓
    ├─ [ระบบ] ส่ง Reminder Email 4 ชม. ก่อนหมดเวลา SLA
    │
    ├─ [หัวหน้างาน] Approve การยกเลิก (ภายใน 1 วันทำการ)
    │       ↓
    │   [ระบบ] อัปเดตสถานะ → Cancelled
    │          + คืนวันลาคงเหลือให้พนักงานอัตโนมัติ
    │          + ส่ง Email แจ้ง พนักงาน + หัวหน้างาน + HR
    │
    └─ [กรณี SLA หมด] ระบบ Escalate ไปยัง HR
```

> **กฎสำคัญ:** ห้ามแก้ไขคำขอที่ Approved แล้ว ต้องยกเลิกและยื่นใหม่เท่านั้น  
> กรณี Rejected: พนักงานยื่นใหม่ได้ทันที ไม่ต้องยกเลิก

> **แหล่งอ้างอิง:** `Leave-Management-Request-and-Approval-Business-User.yaml` → `workflow`, QA-M1, R4 (QA v2), M1/M3 (QA v3)

---

## 6. Happy Path ที่ใช้เป็น Baseline ได้แล้ว

**สถานการณ์:** พนักงานประจำ ยื่นลาพักผ่อน 3 วัน และได้รับการอนุมัติ

1. พนักงาน Login เข้าระบบ Web
2. ตรวจสอบวันลาพักผ่อนคงเหลือ (ต้องมีสิทธิ์เพียงพอ และผ่านทดลองงาน 3 เดือนแล้ว)
3. กรอกคำขอลาพักผ่อน: เลือกประเภท "ลาพักผ่อนประจำปี", ระบุวันที่ล่วงหน้าอย่างน้อย 1 วัน, กรอกเหตุผล
4. ระบบบันทึกคำขอ → Status = **Pending**; ส่ง Email แจ้งหัวหน้างาน + HR
5. หัวหน้างานเปิด Email → คลิก Approve
6. ระบบอัปเดต Status = **Approved**; ส่ง Email แจ้งพนักงาน + HR
7. พนักงานรับ Email ยืนยันการอนุมัติ
8. HR รับ Email monitoring (ทุก event)

> **แหล่งอ้างอิง:** `Leave-Management-Request-and-Approval-Business-User.yaml` → `workflow`, QA-H2, R5 (QA v2), M2 (QA v3)

---

## 7. Business Rules Baseline ที่ยืนยันแล้ว

### 7.1 ประเภทการลา (7 ประเภท — ยืนยันโดย Business User)

| ลำดับ | ประเภทการลา | พนักงานประจำ | Outsource | หมายเหตุ |
|-------|------------|:-----------:|:---------:|---------|
| 1 | ลาป่วย | ✅ | ✅ | เกิน 3 วันทำการต่อเนื่อง: แนบใบรับรองแพทย์ |
| 2 | ลากิจส่วนตัว | ✅ | ✅ | แจ้งล่วงหน้าอย่างน้อย 3 วันทำการ |
| 3 | ลาพักผ่อนประจำปี | ✅ | ✅ | แจ้งล่วงหน้าอย่างน้อย 1 วัน |
| 4 | ลาคลอดบุตร | ✅ | ❌ | Outsource ใช้สิทธิ์จากบริษัทต้นสังกัด |
| 5 | ลาเพื่อทำหมัน | ✅ | ❌ | Outsource ใช้สิทธิ์จากบริษัทต้นสังกัด |
| 6 | ลารับราชการทหาร | ✅ | ❌ | Outsource ใช้สิทธิ์จากบริษัทต้นสังกัด |
| 7 | ลาอุปสมบท | ✅ | ❌ | Outsource ใช้สิทธิ์จากบริษัทต้นสังกัด |

> บริษัทอาจประกาศเพิ่มประเภทการลาอื่นๆ ได้ในภายหลัง  
> **แหล่งอ้างอิง:** QA-H1 (Business User), R2 (HR Manager)

### 7.2 สิทธิ์วันลา (ยืนยันโดย HR Manager และ Business User)

| ประเภทการลา | สิทธิ์ | หมายเหตุ |
|------------|--------|---------|
| ลาป่วย | ตามจริง | เกิน 3 วันทำการต่อเนื่อง ต้องมีใบรับรองแพทย์ |
| ลากิจส่วนตัว | **3 วัน/ปี** (ทั้งประจำและ Outsource) | ข้อมูล 45 วันใน PPTX เป็นข้อมูลข้าราชการ — ยืนยันว่าผิดพลาด |
| ลาพักผ่อนประจำปี | ตามตารางอายุงาน (ดูด้านล่าง) | สะสมข้ามปีได้ cap = 30 วัน |
| ลาคลอดบุตร ฯลฯ | ตามกฎหมายแรงงาน (ไม่ระบุจำนวนในเอกสาร) | อยู่นอกขอบเขตยืนยันของ QA |

**ตารางสิทธิ์วันลาพักผ่อนตามอายุงาน:**

| ช่วงอายุงาน | วันลาพักผ่อน/ปี |
|------------|:--------------:|
| < 3 เดือน (ช่วงทดลองงาน) | ไม่มีสิทธิ์ |
| 3 เดือน – < 1 ปี | ไม่มีสิทธิ์ |
| 1 – 3 ปี | **10 วัน** |
| 3 – 5 ปี | **12 วัน** |
| 5 – 10 ปี | **15 วัน** |
| 10+ ปี | **18 วัน** |

- วันลาที่ไม่ได้ใช้สะสมข้ามปีได้ สูงสุดไม่เกิน **30 วัน**
- อายุงานพนักงานประจำ: นับจากวันบรรจุ (ไม่รวมช่วงทดลองงาน)
- อายุงาน Outsource: นับจากวันเริ่มงานใน ABC Company

> **แหล่งอ้างอิง:** QA-H3 (Business User), R1, R2, R6 (HR Manager), M2 (QA v3)

### 7.3 Approval Rules (ยืนยันแล้ว)

| กฎ | รายละเอียด |
|----|-----------|
| ผู้อนุมัติ | หัวหน้างานโดยตรง (Line Manager) เท่านั้น — 1 ระดับ |
| เหตุผลการ Reject | Optional — หัวหน้าระบุหรือไม่ก็ได้ |
| เหตุผลแสดงใน Email | ใช่ — เหตุผล Reject ปรากฏในอีเมลแจ้งผลพนักงาน |

> **แหล่งอ้างอิง:** QA-H5, QA-M2 (Business User)

### 7.4 Cancel / Edit Rules (ยืนยันแล้ว)

| สถานะเดิม | กฎ |
|----------|----|
| **Pending** | พนักงานยกเลิกได้เองทันที — ไม่ต้องแจ้งหัวหน้า → Status: **Cancelled** |
| **Approved** | พนักงานส่ง Cancel Request → หัวหน้า re-approve ภายใน **1 วันทำการ** → Status ระหว่างรอ: **Cancel Requested** → เมื่อ Approved: Status: **Cancelled** พร้อมคืนวันลาอัตโนมัติ |
| **Rejected** | พนักงานยื่นใหม่ได้ทันที ไม่ต้องยกเลิก |
| **Approved (Edit)** | ห้ามแก้ไข ต้องยกเลิกแล้วยื่นใหม่เท่านั้น |

> **แหล่งอ้างอิง:** QA-M1 (Business User), R4 (HR Manager), M1 (QA v3)

### 7.5 SLA — Re-approve การยกเลิก (ยืนยันแล้ว)

| รายการ | รายละเอียด |
|--------|-----------|
| กำหนดเวลา | 1 วันทำการ (นับจากวันที่พนักงานส่ง Cancel Request) |
| Reminder | ส่ง Email แจ้งหัวหน้างาน **4 ชั่วโมงก่อนหมดเวลา** |
| กรณี SLA หมด | **Escalate ไปยัง HR** เพื่อดำเนินการแทน |

> **แหล่งอ้างอิง:** R4 (HR Manager), M3 (QA v3 — ตอบโดย HR Manager)

### 7.6 Notification Events (ยืนยันแล้ว)

| Event | ผู้รับ Email |
|-------|------------|
| พนักงานยื่นลา | หัวหน้างาน + HR |
| Approved | พนักงาน + HR |
| Rejected | พนักงาน + HR |
| ยกเลิก Approved (Cancellation Approved) | พนักงาน + หัวหน้างาน + HR |
| SLA Reminder (4 ชม. ก่อนหมด) | หัวหน้างาน |
| SLA Escalate (หมดเวลา) | HR |

> ช่องทาง: Email เป็นหลัก + In-system notification บนหน้าจอเมื่อ Login (QA-L1)  
> **แหล่งอ้างอิง:** QA-M3, QA-L1 (Business User), R5 (HR Manager), M3 (QA v3)

### 7.7 Outsource Onboarding Process (ยืนยันแล้ว)

- **วิธี:** Import จาก Excel template (HR ส่ง template ให้บริษัทต้นสังกัดกรอก แล้ว HR import)
- **ความถี่:** ทุกต้นไตรมาส หรือเมื่อมีการเปลี่ยนแปลง

**ข้อมูลบังคับ 7 fields:**

| # | Field |
|---|-------|
| 1 | ชื่อ-นามสกุล (TH/EN) |
| 2 | รหัสพนักงาน (กำหนดโดย HR) |
| 3 | แผนก / ตำแหน่ง |
| 4 | บริษัทต้นสังกัด |
| 5 | Email (ใช้ Login) |
| 6 | วันเริ่มงานใน ABC |
| 7 | หัวหน้างานโดยตรง (Line Manager) |

> **แหล่งอ้างอิง:** R3 (HR Manager)

### 7.8 Status State Machine (ยืนยันแล้ว)

```
             ยื่นลา
               ↓
           [Pending]
          /         \
    ยกเลิกเอง      หัวหน้า action
         ↓          /        \
    [Cancelled]  Approve    Reject
                   ↓           ↓
              [Approved]   [Rejected]
              /       \         ↓
        Edit ❌     Cancel    ยื่นใหม่
                     Request
                       ↓
               [Cancel Requested]
               /               \
         Approve           SLA หมด
         การยกเลิก         → Escalate HR
              ↓
         [Cancelled]
         + คืนวันลา
```

> **แหล่งอ้างอิง:** R4 (HR Manager), M1 (QA v3)

---

## 8. สิ่งที่ Intentionally ยังไม่ใส่ใน Requirement Summary นี้

ประเด็นต่อไปนี้ถูกกันออกจาก confirmed baseline เพราะยังไม่มีข้อมูลยืนยันที่ครบถ้วน:

| หัวข้อ | เหตุผลที่ยังไม่รวม | สิ่งที่ต้องทำต่อ |
|--------|-------------------|----------------|
| **จำนวนวันลาคลอดบุตร / ทำหมัน / รับราชการ / อุปสมบท** | ไม่มีข้อมูลจำนวนวันที่ยืนยันในเอกสารต้นทางหรือ QA ใดๆ | ขอข้อมูลจาก HR พร้อม reference กฎหมายแรงงาน |
| **Formula การสะสมวันลา (carry-forward logic)** | รู้ว่า cap = 30 วัน แต่ไม่ชัดว่า วันที่ไม่ใช้ในปีหนึ่งถูกนำไปสะสมอย่างไร (เต็มจำนวน? หรือ formula?) | ยืนยัน carry-forward calculation กับ HR |
| **HR Email: Individual vs Distribution List** | R5 mention แนวคิด HR email group แต่ยังไม่ได้รับการ confirm เป็น requirement | ตัดสินใจ design ในขั้น System Requirement |
| **Leave Eligibility — Outsource อายุงานน้อยกว่า 3 เดือน** | M2 ยืนยันว่า < 3 เดือน ไม่มีสิทธิ์ลาพักผ่อน แต่ยังไม่ได้ยืนยัน approach ดึง probation date (จาก HRIS หรือ config) | ตัดสินใจ technical approach ในขั้น System Requirement |
| **Intermediate cancel status name** | M1 ยืนยัน Final state = "Cancelled" แต่ไม่ได้ระบุชื่อ Intermediate state อย่างชัดเจน (ใช้ "Cancel Requested" ตาม recommendation) | ยืนยันหรือตัดสินใจโดย BA ใน design session |
| **SLA Escalate ไปหาใครใน HR** | M3 ระบุ "Escalate" แต่ไม่ระบุ assignee ใน HR | ตัดสินใจ workflow design ในขั้น System Requirement |
| **Reporting format และ fields สำหรับ export** | BR-010 ระบุเพียง "export ตามช่วงเวลา/หน่วยงาน/ประเภทพนักงาน" แต่ไม่มี report template | Phase 2 requirement — ยืนยันกับ HR ก่อน design |

---

## 9. Source Reference

### ไฟล์ต้นทาง

| ไฟล์ | บทบาทในเอกสารนี้ |
|------|----------------|
| `raw-extracted/Leave-Management-Request-and-Approval-Business-User.yaml` | Business requirements หลัก (BR-001–BR-010, Workflow, User Stories) |
| `raw-extracted/ABC-Company-Leave-Form.md` | ข้อมูล Leave Form fields (ประเภทการลา, ข้อมูลพนักงาน) |
| `raw-extracted/ABC-Compay-HR-Regularion.md` | ระเบียบการลา ขอบเขตพนักงาน |
| `raw-extracted/ABC-Leave-Workflow-Thai-QA.md` | ขั้นตอนการลา ระยะเวลาแจ้งล่วงหน้า |
| `raw-extracted/ระบบบริหารการลาและการอนุมัติ.md` | Overview workflow 4 ขั้นตอน |

### ไฟล์ QA และ Report

| ไฟล์ | บทบาท | สถานะ |
|------|-------|-------|
| `requirement-validation/requirement-data-quality-analysis-report-v3.md` | รายงาน Data Quality รอบ v3 (Final) | ✅ |
| `requirement-validation/requirement-data-quality-analysis-qa-list.yaml` | QA v1 (12 items, ตอบโดย Business User) | 12/12 Closed |
| `requirement-validation/requirement-data-quality-analysis-qa-list-v2.yaml` | QA v2 (6 items, ตอบโดย HR Manager) | 6/6 Closed |
| `requirement-validation/requirement-data-quality-analysis-qa-list-v3.yaml` | QA v3 (3 items, ตอบโดย HR Manager) | 3/3 Closed |

### QA Item Reference Index

| QA ID | ประเด็น | ผู้ตอบ | ใช้ใน Section |
|-------|---------|--------|--------------|
| QA-H1 | ประเภทการลา 7 ประเภท | Business User | 7.1 |
| QA-H2 | ลาพักผ่อนแจ้งล่วงหน้า 1 วัน | Business User | 7.1, Happy Path |
| QA-H3 | สิทธิ์วันลา (ลาป่วย/พักผ่อน) | Business User | 7.2 |
| QA-H4 | Outsource policy (ต้องรองรับในระบบ) | Business User | 3.2, 4 |
| QA-H5 | Approval 1 ระดับ (Line Manager) | Business User | 4, 7.3 |
| QA-H6 | HRIS integration model | Business User | 3.4 |
| QA-M1 | Cancel / Edit rules (เบื้องต้น) | Business User | 7.4 |
| QA-M2 | Reject reason optional + แสดงใน Email | Business User | 7.3 |
| QA-M3 | Email recipients ต่อ event | Business User | 7.6 |
| QA-M4 | สะสมวันลาตามอายุงาน (เบื้องต้น) | Business User | 7.2 |
| QA-L1 | Notification channel (Email + In-system) | Business User | 7.6 |
| QA-L2 | ลากิจ 45 วัน (ข้าราชการ) = ผิดพลาด | Business User | 7.2 |
| R1 | สิทธิ์ลากิจ = 3 วัน/ปี | HR Manager | 7.2 |
| R2 | Outsource leave type matrix | HR Manager | 7.1 |
| R3 | Onboard Outsource via Excel template | HR Manager | 7.7 |
| R4 | Cancel/Edit rule (confirmed + SLA 1 วัน) | HR Manager | 7.4, 7.5 |
| R5 | HR รับ notification ทุก event + event ยกเลิก | HR Manager | 7.6 |
| R6 | ตารางสะสมวันลาพักผ่อน + cap 30 วัน | HR Manager | 7.2 |
| M1 | Final cancel status = "Cancelled" | HR Manager | 7.4, 7.8 |
| M2 | Probation 3 เดือน = ไม่มีสิทธิ์ลาพักผ่อน | HR Manager | 7.2 |
| M3 | SLA Reminder 4 ชม. + Escalate HR | HR Manager | 7.5 |

---

*เอกสารนี้จัดทำโดย Requirement Summary Document Agent  
อ้างอิงจาก QA v1 (Business User), QA v2 (HR Manager) และ QA v3 (HR Manager) ซึ่งทั้งหมด Closed แล้ว (21/21)*
