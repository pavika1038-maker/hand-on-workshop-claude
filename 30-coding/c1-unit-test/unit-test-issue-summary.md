# Unit Test Issue Summary — Session 4

> Part 2 ขั้นที่ 3 — Issue Summary และ Correction Loop  
> วันที่: 2026-06-16  
> สรุปโดย: Pavika (Workshop Day 2 Session 4)

---

## สถานะโดยรวม

| รายการ | ค่า |
|--------|-----|
| Test cases ทั้งหมด | 68 |
| Pass | 52 (76%) |
| Skip (intentional) | 16 (24%) |
| Fail | 0 |
| Method coverage | 4 / 11 (36%) |
| Methods ที่ยังไม่มี test | 7 (รวม 2 high-risk) |

---

## Issue ที่พบ — แบ่งตาม Priority

### P1 — ต้องแก้ก่อน (Blocking)

| Issue ID | ประเภท | รายละเอียด | แนวทางแก้ไข |
|----------|--------|-----------|------------|
| ISS-001 | Gap — ไม่มี test | `ApproveCancelAsync` ไม่มี test เลย (GAP-18~22) — business logic ซับซ้อนเท่ากับ ApproveAsync | เพิ่ม TC และ test code สำหรับ ApproveCancelAsync |
| ISS-002 | Gap — ไม่มี test | `RejectCancelAsync` ไม่มี test เลย (GAP-23~25) — มี status rollback ที่ต้องตรวจ | เพิ่ม TC และ test code สำหรับ RejectCancelAsync |
| ISS-003 | Conflict unresolved | GAP-14: TC-SPEC-020 vs TC-KB-020 — spec บอก Approved→throw แต่ implementation สร้าง CancelRequest แทน | resolve กับ PO/BA ว่า behavior ที่ถูกต้องคืออะไร |

### P2 — ควรแก้ก่อน go-live

| Issue ID | ประเภท | รายละเอียด | แนวทางแก้ไข |
|----------|--------|-----------|------------|
| ISS-004 | Skipped — ยังไม่ implement | VR-003~007 ใน SubmitLeaveRequestAsync (GAP-01~05, 08) — business rule ใน spec แต่ service ไม่มี code | implement VR-003~007 ใน service แล้ว unskip test |
| ISS-005 | Skipped — ไม่มี dependency | Event publish (GAP-09, 15, 16) — `INotificationService` ไม่ได้ inject เข้า service | inject INotificationService แล้ว unskip TC-SPEC-015/026/032 |
| ISS-006 | Gap — ไม่มี test | `GetDetailAsync` ยังไม่มี test (GAP-27) — NotFoundException path เป็น bug-prone | เพิ่ม HappyPath + NotFoundException test |
| ISS-007 | Gap — boundary | `CancelAsync` ไม่มี test สำหรับ null balance path (GAP-13) | เพิ่ม TC-KB สำหรับ null balance ใน Cancel flow |

### P3 — Nice to have

| Issue ID | ประเภท | รายละเอียด |
|----------|--------|-----------|
| ISS-008 | Gap — FluentValidation | GAP-06~07, 10~12, 17: validation ทำที่ FluentValidation/Controller แทน service — อาจต้องมี integration test |
| ISS-009 | Gap — query methods | GAP-26, 28~30: GetMyRequests, GetPending, GetAllForHr, GetCancelRequests ยังไม่มี test — risk ต่ำ |
| ISS-010 | Refactor | TC-KB-024/025 สามารถรวมเป็น `[Theory][InlineData(null)][InlineData(" ")]` ได้ |

---

## 16 Skipped Tests — แบ่งตามสาเหตุ

| กลุ่ม | จำนวน | Test IDs | แนวทาง |
|-------|-------|----------|--------|
| Business rule ยังไม่ implement (VR-003~007, HalfDay spans) | 9 | TC-SPEC-006~011, skip ใน test code | implement service แล้ว unskip |
| FluentValidation รับผิดชอบ (MaxLength, date format) | 4 | TC-KB-004, 008, 009, 010~012 บางส่วน | เพิ่ม Integration Test แทน Unit Test |
| INotificationService ไม่ได้ inject | 3 | TC-SPEC-015, 026, 032 | inject dependency แล้ว unskip |

---

## Correction Loop — สิ่งที่ทำได้ในช่วงนี้ vs carry forward

### ✅ แก้ได้ทันที (ถ้ามีเวลา)
- ISS-007: เพิ่ม test null balance ใน CancelAsync
- ISS-010: refactor TC-KB-024/025 เป็น [Theory]

### 📋 Carry Forward — บันทึกเป็น backlog
- ISS-001/002: ApproveCancelAsync + RejectCancelAsync test
- ISS-003: Resolve conflict TC-SPEC-020 vs TC-KB-020 กับ PO
- ISS-004: Implement VR-003~007 ใน service
- ISS-005: Inject INotificationService
- ISS-006: GetDetailAsync test
- ISS-008: Integration test สำหรับ FluentValidation rules
- ISS-009: Query method tests (P3)

---

## สรุป Lesson Learned จาก Session 4

| บทเรียน | รายละเอียด |
|---------|-----------|
| Test generation พบ bug จริง | TC-KB-030~032 และ TC-KB-010/011 พบว่า service ไม่มี null guard → fix ก่อน generate test |
| TC-SPEC vs TC-KB ต่างกันชัด | SPEC test ครอบคลุม happy/exception path, KB test ครอบคลุม boundary/regression — ขาดอย่างใดอย่างหนึ่งได้ gap |
| 16 skipped ไม่ใช่ปัญหา | skipped อย่างมีเหตุผลดีกว่า test ที่ pass แต่ assert ผิดสิ่ง |
| Method coverage 36% ไม่ได้แปลว่าแย่ | 4 method ที่ test มี business logic 85%+ ของระบบ — query wrapper method risk ต่ำกว่า |
| Coverage Check ต้องทำทุกครั้ง | ถ้าไม่รัน Coverage Check จะไม่รู้ว่า ApproveCancelAsync/RejectCancelAsync ไม่มี test เลย ทั้งที่ risk สูง |
