---
title: "System Requirement Specification Summary"
document_type: "SRS Summary Template"
version: "1.0"
language: "th"
---
# System Requirement Specification Summary: [ชื่อระบบ / ชื่อกระบวนการ]

## 1. วัตถุประสงค์ของเอกสาร

- [อธิบายวัตถุประสงค์ของ SRS Summary ฉบับนี้]
- [อธิบายว่าระบบนี้รองรับ business objective อะไรจาก BRD]
- [อธิบายว่าขอบเขตของเอกสารนี้ครอบคลุม requirement ระดับ summary ใด]

## 2. ขอบเขตและแหล่งอ้างอิง

### 2.1 ขอบเขตของเอกสาร

- [ระบุขอบเขตของระบบหรือขอบเขตการแปลงจาก BRD]
- [ระบุว่าเอกสารนี้เป็น summary baseline ก่อนแตกเป็น SRS รายละเอียดของแต่ละรายการ]

### 2.2 เอกสารอ้างอิงหลัก

| ลำดับ | เอกสารอ้างอิง   | บทบาทของเอกสาร                      |
| ---------- | ---------------------------- | ------------------------------------------------- |
| 1          | [BRD file path]              | [baseline หลักสำหรับการแปลง SRS Summary] |
| 2          | [summary / QA / policy file] | [เอกสารช่วยยืนยัน requirement]    |

## 3. หลักการแปลงและ Traceability

### 3.1 หลักการแปลง BRD เป็น SRS Summary

- แปลง requirement เชิงธุรกิจให้เป็น requirement เชิงระบบ
- แยก requirement ออกเป็น `Functional`, `Non-Functional`, และ `Technical`
- หากข้อมูลจาก BRD ไม่พอให้สรุป ให้ระบุว่า `ข้อมูลไม่เพียงพอ`
- คงระดับเอกสารไว้ในเชิง summary ไม่ลงลึกถึง implementation หรือ detailed design

### 3.2 หลักการ Traceability

- ทุก requirement ต้องมี `Requirement ID`
- ทุก requirement สำคัญต้องมี `Source BRD Reference`
- หาก requirement แตกมาจากหลายจุดใน BRD ให้ระบุ source ได้มากกว่า 1 จุด
- รายการใน `Screen List`, `Interface List`, และ `Validation Rule Matrix` ต้องอ้าง `Related Requirement IDs` และ `Source BRD Reference` ให้ตรวจสอบย้อนกลับได้

## 4. Functional Requirement

### 4.1 Screen Function Requirement

> หมายเหตุ:
> หากมี requirement เพิ่มเติมจาก user input ที่เป็นลักษณะ Functional และมีหลักฐานอ้างอิงชัดเจน
> ให้รวมไว้ใน section นี้หรือ section 4.2 / 4.3 ตามลักษณะของ requirement โดยไม่ต้องแยกหัวข้อพิเศษ

| Req ID  | Module / Screen        | System Requirement Description    | Input / Output Summary      | Source BRD Reference         | หมายเหตุ |
| ------- | ---------------------- | --------------------------------- | --------------------------- | ---------------------------- | ---------------- |
| SFR-001 | [ชื่อหน้าจอ] | [ระบบต้องสามารถ...] | [input / output สำคัญ] | [BRD section / table / rule] | [ถ้ามี]     |
| SFR-002 | [ชื่อหน้าจอ] | [ระบบต้องสามารถ...] | [input / output สำคัญ] | [BRD section / table / rule] | [ถ้ามี]     |

### 4.2 Report Function Requirement

| Req ID  | Report Name            | System Requirement Description    | Filter / Output Summary      | Source BRD Reference         | หมายเหตุ |
| ------- | ---------------------- | --------------------------------- | ---------------------------- | ---------------------------- | ---------------- |
| RFR-001 | [ชื่อรายงาน] | [ระบบต้องสามารถ...] | [filter / output สำคัญ] | [BRD section / table / rule] | [ถ้ามี]     |

### 4.3 System Integration Requirement

| Req ID  | Integration Target               | System Requirement Description | Direction / Trigger                     | Source BRD Reference                  | หมายเหตุ |
| ------- | -------------------------------- | ------------------------------ | --------------------------------------- | ------------------------------------- | ---------------- |
| SIR-001 | [ชื่อระบบปลายทาง] | [ระบบต้อง...]          | [inbound / outbound / realtime / batch] | [BRD section / rule / additional req] | [ถ้ามี]     |
| SIR-002 | [ชื่อระบบปลายทาง] | [ระบบต้อง...]          | [inbound / outbound / realtime / batch] | [BRD section / rule / additional req] | [ถ้ามี]     |

### 4.4 Screen List

| Screen ID | Screen Name | Primary Users | Purpose Summary | Related Requirement IDs | Source BRD Reference | หมายเหตุ |
| --------- | ----------- | ------------- | --------------- | ----------------------- | -------------------- | -------- |
| SCR-001 | [ชื่อหน้าจอ] | [Actor] | [วัตถุประสงค์หลักของหน้าจอ] | [SFR-001, SFR-002] | [BRD section / use case] | [ถ้ามี] |
| SCR-002 | [ชื่อหน้าจอ] | [Actor] | [วัตถุประสงค์หลักของหน้าจอ] | [SFR-003] | [BRD section / use case] | [ถ้ามี] |

### 4.5 Interface List

| Interface ID | Interface Name | Source System | Target System | Direction / Pattern | Data Summary | Trigger / Frequency | Related Requirement IDs | Source BRD Reference | หมายเหตุ |
| ------------ | -------------- | ------------- | ------------- | ------------------- | ------------ | ------------------- | ----------------------- | -------------------- | -------- |
| IF-001 | [ชื่อ interface] | [System A] | [System B] | [Inbound / Outbound / Realtime / Batch] | [ข้อมูลหลักที่รับส่ง] | [trigger / schedule] | [SIR-001, TR-001] | [BRD rule / additional req] | [ถ้ามี] |
| IF-002 | [ชื่อ interface] | [System A] | [System B] | [Inbound / Outbound / Realtime / Batch] | [ข้อมูลหลักที่รับส่ง] | [trigger / schedule] | [SIR-002] | [BRD rule / additional req] | [ถ้ามี] |

### 4.6 Validation Rule Matrix

| Validation ID | Related Screen / Process | Validation Rule Description | Expected Result / Error Handling | Related Requirement IDs | Source BRD Reference | หมายเหตุ |
| ------------- | ------------------------ | --------------------------- | -------------------------------- | ----------------------- | -------------------- | -------- |
| VR-001 | [หน้าจอ / process] | [กติกาการตรวจสอบ] | [ผลที่ระบบต้องทำเมื่อผ่าน/ไม่ผ่าน] | [SFR-001, SFR-002] | [BRD rule / use case] | [ถ้ามี] |
| VR-002 | [หน้าจอ / process] | [กติกาการตรวจสอบ] | [ผลที่ระบบต้องทำเมื่อผ่าน/ไม่ผ่าน] | [SFR-003] | [BRD rule / use case] | [ถ้ามี] |

## 5. Non-Functional Requirement

> หมายเหตุ:
> หากมี requirement เพิ่มเติมจาก user input ที่เป็นลักษณะ Non-Functional
> ให้รวมไว้ใน section นี้และระบุ source ให้ trace ได้ชัดเจน

| Req ID  | Category                                                   | Requirement Description | Measure / Acceptance Basis                                                          | Source BRD Reference        | หมายเหตุ |
| ------- | ---------------------------------------------------------- | ----------------------- | ----------------------------------------------------------------------------------- | --------------------------- | ---------------- |
| NFR-001 | [Performance / Security / Availability / Usability / etc.] | [ข้อกำหนด NFR]  | [เกณฑ์วัด / หากไม่พอให้ระบุข้อมูลไม่เพียงพอ] | [BRD section / note / rule] | [ถ้ามี]     |
| NFR-002 | [Category]                                                 | [ข้อกำหนด NFR]  | [เกณฑ์วัด]                                                                  | [BRD section / note / rule] | [ถ้ามี]     |

## 6. Technical Requirement

> หมายเหตุ:
> หากมี requirement เพิ่มเติมจาก user input ที่เป็นลักษณะ Technical
> ให้รวมไว้ใน section นี้และระบุ source ให้ trace ได้ชัดเจน

| Req ID | Category                                                       | Technical Requirement Description    | Constraint / Standard                         | Source BRD Reference         | หมายเหตุ |
| ------ | -------------------------------------------------------------- | ------------------------------------ | --------------------------------------------- | ---------------------------- | ---------------- |
| TR-001 | [Platform / Browser / Integration / Security / Infrastructure] | [ข้อกำหนดทางเทคนิค] | [มาตรฐาน / ข้อจำกัด / version] | [BRD section / rule / scope] | [ถ้ามี]     |
| TR-002 | [Category]                                                     | [ข้อกำหนดทางเทคนิค] | [มาตรฐาน / ข้อจำกัด / version] | [BRD section / rule / scope] | [ถ้ามี]     |

## 7. Assumptions / Open Issues

| ประเภท | รายการ                                       | ผลกระทบต่อ SRS | สิ่งที่ต้องยืนยันเพิ่ม |
| ------------ | -------------------------------------------------- | ------------------------ | -------------------------------------------- |
| Assumption   | [สมมติฐาน]                                 | [ผลกระทบ]         | [ถ้ามี]                                 |
| Open Issue   | [ประเด็นที่ข้อมูลยังไม่พอ] | [ผลกระทบ]         | [สิ่งที่ต้องยืนยัน]         |

## 8. Traceability Matrix

| BRD Reference        | BRD Requirement Summary             | SRS Summary Requirement ID | Requirement Type                          | หมายเหตุ |
| -------------------- | ----------------------------------- | -------------------------- | ----------------------------------------- | ---------------- |
| [BRD section / item] | [สรุป requirement ต้นทาง] | [SFR-001, NFR-001, TR-001] | [Functional / Non-Functional / Technical] | [ถ้ามี]     |
| [BRD section / item] | [สรุป requirement ต้นทาง] | [SIR-001]                  | [Functional - Integration]                | [ถ้ามี]     |

## 9. Source Reference

- `[BRD file path]`
- `[summary / QA / policy / other supporting file]`
