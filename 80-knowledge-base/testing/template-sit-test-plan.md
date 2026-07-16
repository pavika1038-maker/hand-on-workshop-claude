# SIT Test Plan — [Module Name]

**Module:** [ชื่อ module]
**Version:** 1.0
**วันที่สร้าง:** 
**สร้างโดย:** 
**Source Artifacts:** [SRS version, Design version, SIT Scenario version]

---

## 1. Entry Criteria

- [ ] Unit Test ผ่าน coverage ≥ 70%
- [ ] ไม่มี P1 defect ค้างจาก UT
- [ ] Application deploy บน SIT environment สำเร็จ
- [ ] Seed data พร้อม (master data, user accounts, initial balance)
- [ ] SIT Scenario, Test Data, Automation Draft ได้รับ review แล้ว

## 2. Exit Criteria

- [ ] Scenario priority High ผ่าน 100%
- [ ] Scenario priority Medium ผ่าน ≥ 90%
- [ ] ไม่มี Critical defect ค้าง
- [ ] High defect ค้างไม่เกิน 2 รายการ และมี workaround ชัดเจน
- [ ] Evidence (screenshot) ครบทุก scenario ที่ผ่าน

---

## 3. Test Wave Table

### Wave 1 — [ชื่อ Wave: Validation / Screen Flow]
**หลักการ:** ทดสอบ validation และ screen behavior ที่ dependency ต่ำ รันแยกกันได้

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W1 | TP-W1-01 | | None | | | | | |

---

### Wave 2 — [ชื่อ Wave: Core Transaction]
**หลักการ:** ทดสอบ happy path ของ core business flow

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W2 | TP-W2-01 | | Low | | | | | |

---

### Wave 3 — [ชื่อ Wave: Approval / State Flow]
**หลักการ:** ทดสอบ state transition ที่ depend on Wave 2

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W3 | TP-W3-01 | | Medium | | | W2 PASS | | |

---

### Wave 4 — [ชื่อ Wave: Report / Integration]
**หลักการ:** ทดสอบ report และ interface หลังมีข้อมูล transaction จาก Wave 1–3

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W4 | TP-W4-01 | | High | | | W1–W3 PASS | | |

---

### Wave 5 — End-to-End Verification
**หลักการ:** ทดสอบ full business flow เชื่อม screen, report, interface, API

| Wave | Plan ID | Test Scope | Dependency Level | Related Components | Related Scenarios | Entry Criteria | Expected Outcome | Risk / Note |
|------|---------|-----------|-----------------|-------------------|------------------|---------------|-----------------|-------------|
| W5 | TP-W5-01 | Full E2E | Very High | ทุก component | ทุก scenario | W1–W4 PASS | ข้อมูลทุกจุดสอดคล้องกัน | |

---

## 4. UT Issues ที่กระทบ SIT

| Issue ID | รายละเอียด | กระทบ Wave | Priority | สถานะ |
|----------|-----------|-----------|---------|-------|
| ISS-001 | | | P1 | Open |

---

## 5. Risk Summary

| Risk | Wave ที่กระทบ | Severity | Mitigation |
|------|-------------|---------|-----------|
| | | Critical/High/Medium | |

---

## 6. Test Environment

| รายการ | Required Value |
|--------|--------------|
| Base URL | |
| Auth Method | |
| Browser | |
| Evidence Folder | |
| Seed Data File | |
