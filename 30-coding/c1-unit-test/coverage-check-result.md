# Coverage Check Result — LeaveRequestService

> ผลลัพธ์จาก Part 2 ขั้นที่ 1  
> วันที่: 2026-06-16  
> Test code: LeaveRequest/tests/LeaveRequest.Application.Tests/Services/LeaveRequestServiceTests.cs

---

## 1. Method Coverage

| # | Method | Design Spec | In TC List | In Test Code | Runnable | Skipped |
|---|--------|-------------|------------|--------------|----------|---------|
| 1 | SubmitLeaveRequestAsync | §4.4 | ✅ | ✅ | 18 | 13 |
| 2 | CancelAsync | §4.4 + §4.6 (merged) | ✅ | ✅ | 9 | 0 |
| 3 | ApproveAsync | §4.5 | ✅ | ✅ | 14 | 1 |
| 4 | RejectAsync | §4.5 | ✅ | ✅ | 11 | 2 |
| 5 | GetMyRequestsAsync | §4.4 | ❌ | ❌ | 0 | — |
| 6 | GetDetailAsync | §4.4 | ❌ | ❌ | 0 | — |
| 7 | GetPendingByManagerAsync | §4.4 | ❌ | ❌ | 0 | — |
| 8 | GetAllForHrAsync | §4.4 | ❌ | ❌ | 0 | — |
| 9 | ApproveCancelAsync | §4.6 | ❌ | ❌ | 0 | — |
| 10 | RejectCancelAsync | §4.6 | ❌ | ❌ | 0 | — |
| 11 | GetCancelRequestsByManagerAsync | §4.6 | ❌ | ❌ | 0 | — |

---

## 2. Coverage Score

| Dimension | จำนวน | % | หมายเหตุ |
|-----------|-------|---|---------|
| Methods ที่มี test | 4 / 11 | 36% | Submit, Cancel, Approve, Reject |
| Methods ที่ไม่มี test | 7 / 11 | 64% | รวม 2 high-risk |
| TCs runnable และ pass | 52 / 68 | 76% | |
| TCs skipped (intentional) | 16 / 68 | 24% | 9 unimplemented VR, 4 FluentValidation, 3 no INotificationService |
| TCs fail | 0 / 68 | 0% | ✅ |
| VR coverage | 2 / 7 | 29% | VR-001, VR-002 เท่านั้น |
| Business logic (covered methods) | ~85% | — | status paths, FORBIDDEN, null balance ครบ |

---

## 3. Coverage Gaps — 30 items

| Gap ID | Method | Scenario | Type | สาเหตุ | Priority |
|--------|--------|----------|------|--------|---------|
| GAP-01 | SubmitLeaveRequestAsync | VR-003: Probation | BusinessRule | ไม่ implement ใน service | P2 |
| GAP-02 | SubmitLeaveRequestAsync | VR-004: อายุงาน < 1 ปี | BusinessRule | ไม่ implement | P2 |
| GAP-03 | SubmitLeaveRequestAsync | VR-005: แจ้งล่วงหน้า Annual ≥ 1 วัน | BusinessRule | ไม่ implement | P2 |
| GAP-04 | SubmitLeaveRequestAsync | VR-006: แจ้งล่วงหน้า Business ≥ 3 วัน | BusinessRule | ไม่ implement | P2 |
| GAP-05 | SubmitLeaveRequestAsync | VR-007: Sick ≥ 3 วัน + ใบแพทย์ | BusinessRule | ไม่ implement | P2 |
| GAP-06 | SubmitLeaveRequestAsync | StartDate ในอดีต | Boundary | ทำที่ FluentValidation | P3 |
| GAP-07 | SubmitLeaveRequestAsync | EndDate < StartDate | Boundary | ทำที่ FluentValidation | P3 |
| GAP-08 | SubmitLeaveRequestAsync | HalfDay spans multiple days | BusinessRule | ไม่ implement ใน service | P2 |
| GAP-09 | SubmitLeaveRequestAsync | Event publish หลัง commit | Integration | ไม่มี INotificationService | P2 |
| GAP-10 | SubmitLeaveRequestAsync | EmployeeId > 50 chars | MaxLength | FluentValidation | P3 |
| GAP-11 | SubmitLeaveRequestAsync | Reason > 500 chars | MaxLength | FluentValidation | P3 |
| GAP-12 | SubmitLeaveRequestAsync | default(DateOnly) | DataType | FluentValidation | P3 |
| GAP-13 | CancelAsync | null balance ใน Cancel Pending path | Boundary | ยังไม่มี test | P2 |
| GAP-14 | CancelAsync | TC-SPEC-020 conflict (Approved→throw vs CancelRequest) | BusinessRule | ยังไม่ resolve กับ PO | P1 |
| GAP-15 | ApproveAsync | Event publish หลัง commit | Integration | ไม่มี INotificationService | P2 |
| GAP-16 | RejectAsync | Event publish หลัง commit | Integration | ไม่มี INotificationService | P2 |
| GAP-17 | RejectAsync | RejectionReason > 500 chars | MaxLength | FluentValidation | P3 |
| GAP-18 | ApproveCancelAsync | HappyPath: approve → restore UsedDays | HappyPath | ยังไม่มี test เลย | **P1** |
| GAP-19 | ApproveCancelAsync | CancelRequest not found | ExceptionFlow | ยังไม่มี test | **P1** |
| GAP-20 | ApproveCancelAsync | CancelRequest status != Pending | ExceptionFlow | ยังไม่มี test | **P1** |
| GAP-21 | ApproveCancelAsync | Manager ไม่ match | BusinessRule | ยังไม่มี test | **P1** |
| GAP-22 | ApproveCancelAsync | ApprovalHistory INSERT verification | BusinessRule | ยังไม่มี test | **P1** |
| GAP-23 | RejectCancelAsync | HappyPath: reject → LeaveRequest กลับ Approved | HappyPath | ยังไม่มี test เลย | **P1** |
| GAP-24 | RejectCancelAsync | CancelRequest not found / invalid status | ExceptionFlow | ยังไม่มี test | **P1** |
| GAP-25 | RejectCancelAsync | Manager ไม่ match | BusinessRule | ยังไม่มี test | **P1** |
| GAP-26 | GetMyRequestsAsync | HappyPath paged list | HappyPath | ยังไม่มี test | P3 |
| GAP-27 | GetDetailAsync | HappyPath + NotFoundException | HappyPath/Exception | ยังไม่มี test | P2 |
| GAP-28 | GetPendingByManagerAsync | HappyPath paged list | HappyPath | ยังไม่มี test | P3 |
| GAP-29 | GetCancelRequestsByManagerAsync | HappyPath paged list | HappyPath | ยังไม่มี test | P3 |
| GAP-30 | GetAllForHrAsync | HappyPath paged list | HappyPath | ยังไม่มี test | P3 |

---

## 4. Duplicate / Overlap — ไม่มีที่ควรลบ

| Pair | เหตุผลที่คงไว้ |
|------|--------------|
| TC-SPEC-021 ↔ TC-KB-028 | SPEC = full happy path, KB = regression guard BUG-002 |
| TC-SPEC-027 ↔ TC-KB-035 | เหตุผลเดียวกัน |
| TC-KB-024 ↔ TC-KB-025 | สามารถรวมเป็น [Theory] ได้แต่ไม่ critical |
| TC-SPEC-012 ↔ TC-KB-015 | SPEC = clearly insufficient, KB = boundary just-under |
