---
title: "SRS Change Log (Central Index)"
document_type: "Change Log"
version: "1.2"
language: "th"
last_updated: "2026-07-15"
---
# SRS Change Log: Leave Request and Approval

เอกสารนี้เป็น central index สำหรับติดตามการเปลี่ยนแปลงของเอกสาร SRS ทั้งหมดในระบบ Leave Request and Approval
ใช้สำหรับวิเคราะห์ว่าข้อมูลใดถูกเพิ่ม แก้ไข หรือลบหลังจาก BRD baseline เดิม

## วิธีใช้เอกสารนี้

- **Change Type**: `Added` = เพิ่มใหม่, `Updated` = แก้ไขของเดิม, `Deleted` = ลบออก, `Created` = สร้างเอกสารครั้งแรก
- **Source**: ระบุที่มาของการเปลี่ยนแปลง เช่น BRD baseline, stakeholder feedback, QA review, design decision, security review, meeting note
- **Post-BRD**: `Y` = ข้อมูลที่เพิ่มหลัง BRD baseline, `N` = มาจาก BRD baseline เดิม
- ใช้ column `Post-BRD` เพื่อกรองเฉพาะข้อมูลที่เพิ่มหลัง BRD ได้ทันที

## Document Registry

| # | Document | File Path | Current Version | Created Date |
|---|----------|-----------|-----------------|--------------|
| 1 | SRS Summary | `leave-request-and-approval-system-requirement-specification-summary.md` | 0.3 | 2026-04-15 |
| 2 | Screen SRS | `v100/leave-request-and-approval-screen-srs-v100.md` | 2.4 | 2026-04-16 |
| 3 | Interface SRS | `v100/leave-request-and-approval-interface-srs-v100.md` | 2.3 | 2026-04-16 |
| 4 | Non-Functional / Technical SRS | `v100/leave-request-and-approval-non-functional-tech-srs-v100.md` | 2.8 | 2026-04-16 |
| 5 | Report SRS | `v100/leave-request-and-approval-report-srs-v100.md` | 2.3 | 2026-04-16 |

### Archive (v001)

| # | Document | File Path | Version | Archived Date |
|---|----------|-----------|---------|---------------|
| 1 | Screen SRS | `v001/leave-request-and-approval-screen-srs-v001.md` | 1.0 | 2026-04-16 |
| 2 | Interface SRS | `v001/leave-request-and-approval-interface-srs-v001.md` | 1.0 | 2026-04-16 |
| 3 | Non-Functional / Technical SRS | `v001/leave-request-and-approval-non-functional-tech-srs-v001.md` | 1.0 | 2026-04-16 |
| 4 | Report SRS | `v001/leave-request-and-approval-report-srs-v001.md` | 1.0 | 2026-04-16 |

## Change Log

| # | Date | Document | Version | Section | Change Type | Description | Source | Post-BRD |
|---|------|----------|---------|---------|-------------|-------------|--------|----------|
| 1 | 2026-04-15 | SRS Summary | 0.3 | All | Created | สร้างเอกสาร SRS Summary จาก BRD — SFR-001~011, RFR-001~002, SIR-001~003, NFR-001~004, TR-001~006 | BRD baseline | N |
| 2 | 2026-04-16 | Screen SRS | 1.0 | All | Created | สร้างเอกสาร Screen SRS — 6 screen functions (SF-001~006) พร้อม SAP ERP ASCII mockup | SRS Summary v0.3 | N |
| 3 | 2026-04-16 | Interface SRS | 1.0 | All | Created | สร้างเอกสาร Interface SRS — 3 interfaces (IF-001~003) | SRS Summary v0.3 | N |
| 4 | 2026-04-16 | Non-Functional / Technical SRS | 1.0 | All | Created | สร้างเอกสาร NFR/Tech SRS — NFR-001~004, TR-001~006 | SRS Summary v0.3 | N |
| 5 | 2026-04-16 | All SRS Documents | — | Change Log | Added | เพิ่ม Change Log section ในทุกเอกสาร SRS | Process improvement | Y |
| 6 | 2026-04-16 | Screen SRS | 2.0 | All | Updated | Archive v001 → v001 folder, สร้าง v002 เป็น working copy | Version management | Y |
| 7 | 2026-04-16 | Interface SRS | 2.0 | All | Updated | Archive v001 → v001 folder, สร้าง v002 เป็น working copy | Version management | Y |
| 8 | 2026-04-16 | Non-Functional / Technical SRS | 2.0 | All | Updated | Archive v001 → v001 folder, สร้าง v002 เป็น working copy | Version management | Y |
| 9 | 2026-04-16 | Report SRS | 2.0 | All | Updated | Archive v001 → v001 folder, สร้าง v002 เป็น working copy, เพิ่ม Change Log section | Version management | Y |
| 10 | 2026-04-16 | All SRS Documents | — | — | Added | สร้าง srs-missing-information.yaml — รวบรวม 30 รายการ "ข้อมูลไม่เพียงพอ" จาก SRS ทั้ง 4 ฉบับ | Gap analysis | Y |
| 11 | 2026-04-16 | Non-Functional / Technical SRS (v002) | 2.1 | 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 | Updated | อัปเดตข้อมูลจาก srs-missing-information.yaml — แทนที่ 'ข้อมูลไม่เพียงพอ' ด้วยคำตอบที่ยืนยันแล้ว (MISS-001~010, MISS-029, MISS-030): architecture, performance, availability, security, accessibility, browser, tech stack, database, integration protocols, reliability, scalability, monitoring, deployment, compliance | srs-missing-information.yaml (ISO 27001:2022, OWASP ASVS, กฎหมายแรงงานไทย) | Y |
| 12 | 2026-04-16 | Interface SRS (v002) | 2.1 | 1.2, 2.1, 2.2, 2.3, 3 | Updated | อัปเดตข้อมูลจาก srs-missing-information.yaml — แทนที่ 'ข้อมูลไม่เพียงพอ' ด้วยคำตอบที่ยืนยันแล้ว (MISS-011~022): IF-001 field mapping/schedule/protocol/inactive handling, IF-002 protocol/schedule/delta-full sync/field mapping, IF-003 protocol/timeout/response code/request fields | srs-missing-information.yaml (ISO 27001:2022, OWASP ASVS, กฎหมายแรงงานไทย) | Y |
| 13 | 2026-04-16 | Screen SRS (v002) | 2.1 | 1.2, 2.3, 2.4, 2.5, 2.6, 3 | Updated | อัปเดตข้อมูลจาก srs-missing-information.yaml — แทนที่ 'ข้อมูลไม่เพียงพอ' ด้วยคำตอบที่ยืนยันแล้ว (MISS-023~028): validation matrix/overlap/half-day, file upload limits, leave type mapping, HR override rules, notification matrix, processing history fields | srs-missing-information.yaml (ISO 27001:2022, OWASP ASVS, กฎหมายแรงงานไทย) | Y |
| 14 | 2026-04-16 | Report SRS (v002) | 2.1 | 2.1, 2.2 | Updated | อัปเดตข้อมูลที่เกี่ยวข้องจาก srs-missing-information.yaml — sorting method, default values, footer, retention period (MISS-029) | srs-missing-information.yaml (ISO 27001:2022, OWASP ASVS, กฎหมายแรงงานไทย) | Y |
| 15 | 2026-04-16 | Interface SRS (v002) | 2.2 | Change Log | Updated | รวม Change Log entries v2.1 ที่ซ้ำซ้อน (3 แถว) เป็นแถวเดียว — ปรับให้กระชับและอ่านง่ายขึ้น | Change Log consolidation | Y |
| 16 | 2026-04-17 | Screen SRS (v100) | 2.4 | Front-matter, File Path | Updated | Promote approved baseline จาก `v002` เป็น `v100` และย้ายไฟล์ไป `v100/` เพื่อใช้ต่อใน Function/System Design | Approval baseline for design handoff | Y |
| 17 | 2026-04-17 | Interface SRS (v100) | 2.3 | Front-matter, File Path | Updated | Promote approved baseline จาก `v002` เป็น `v100` และย้ายไฟล์ไป `v100/` เพื่อใช้ต่อใน Function/System Design | Approval baseline for design handoff | Y |
| 18 | 2026-04-17 | Non-Functional / Technical SRS (v100) | 2.8 | Front-matter, File Path | Updated | Promote approved baseline จาก `v002` เป็น `v100` และย้ายไฟล์ไป `v100/` เพื่อใช้ต่อใน Function/System Design | Approval baseline for design handoff | Y |
| 19 | 2026-04-17 | Report SRS (v100) | 2.3 | Front-matter, File Path | Updated | Promote approved baseline จาก `v002` เป็น `v100` และย้ายไฟล์ไป `v100/` เพื่อใช้ต่อใน Function/System Design | Approval baseline for design handoff | Y |
| 20 | 2026-07-15 | Interface SRS | 1.1 | 2.3.2, 2.3.6, 2.3.7, 2.3.8 (IF-003) | Added | บันทึกกฎ threshold rollback ของ Excel import Outsource — record ผิด > 50% ของทั้งไฟล์ → rollback ทั้งไฟล์ + HTTP 422 (BR-IF003-001, ERR-IF003-007); พฤติกรรมมีในโค้ด ImportService แต่เดิม SRS ไม่มี requirement รองรับ — threshold 50% ยังต้องให้ HR/BA ยืนยัน | Code review finding | Y |
| 21 | 2026-07-15 | Screen SRS | 1.2 | SF-012 §2.12.5, §2.12.6, §2.12.7 | Added | บันทึกกฎ threshold rollback เดียวกันฝั่งหน้าจอ Import (BR-012-IMP, ERR-IMP-005) ให้สอดคล้องกับ IF-003 BR-IF003-001 | Code review finding | Y |

## Analysis Guide

### กรองเฉพาะข้อมูลที่เพิ่มหลัง BRD baseline
กรอง column `Post-BRD` = `Y` เพื่อดูเฉพาะข้อมูลที่เพิ่มเข้ามาหลังจาก BRD baseline เดิม

### กรองตาม Change Type
- `Added` — ข้อมูลใหม่ที่ไม่มีใน BRD เดิม (เช่น stakeholder feedback, design decision)
- `Updated` — ข้อมูลที่แก้ไขจาก baseline เดิม
- `Deleted` — ข้อมูลที่ถูกลบออก

### กรองตาม Source
ใช้ column `Source` เพื่อวิเคราะห์ว่าข้อมูลมาจากแหล่งใด เช่น:
- `BRD baseline` — มาจาก BRD เดิม
- `Stakeholder feedback` — มาจากการประชุมหรือ feedback
- `QA review` — มาจากการตรวจสอบคุณภาพ
- `Design decision` — มาจากการตัดสินใจเชิงออกแบบ
- `Security review` — มาจากการตรวจสอบด้านความปลอดภัย
