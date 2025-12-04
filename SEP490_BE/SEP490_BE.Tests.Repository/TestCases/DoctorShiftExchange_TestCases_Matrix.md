# Test Cases Matrix - Doctor Shift Exchange Controller

## Overview
This document contains test cases for 4 main methods: **GetDoctorsBySpecialty**, **CreateShiftSwapRequest**, **GetRequestsByDoctorId**, and **GetDoctorShifts**.

---

## 1. CreateShiftSwapRequest

| UTCID | Test Case Name | Precondition | Input | Expected Status | Expected Response | Exception | Notes |
|-------|----------------|--------------|-------|-----------------|-------------------|-----------|-------|
| **UTCID01** | Valid request | Valid request data with different doctors, valid shifts | Doctor1Id=1, Doctor2Id=2, Doctor1ShiftRefId=10, Doctor2ShiftRefId=20, ExchangeDate=2025-12-01, SwapType="Temporary" | 200 OK | success=true, message="Yêu cầu đổi ca đã được tạo thành công" | None | Happy path |
| **UTCID02** | Invalid: Same doctor | Request with same doctor for both Doctor1Id and Doctor2Id | Doctor1Id=1, Doctor2Id=1, Doctor1ShiftRefId=10, Doctor2ShiftRefId=20 | 400 BadRequest | success=false, message="Invalid shift swap request" | ArgumentException | Validation error |

---

## 2. GetRequestsByDoctorId

| UTCID | Test Case Name | Precondition | Input | Expected Status | Expected Response | Exception | Notes |
|-------|----------------|--------------|-------|-----------------|-------------------|-----------|-------|
| **UTCID03** | Doctor has requests | Doctor has multiple shift swap requests | doctorId=1 | 200 OK | success=true, data=[list of 2 requests] | None | Returns list with data |
| **UTCID04** | Doctor has no requests | Doctor has no shift swap requests | doctorId=999 | 200 OK | success=true, data=[] | None | Returns empty list |

---

## 3. GetDoctorShifts

| UTCID | Test Case Name | Precondition | Input | Expected Status | Expected Response | Exception | Notes |
|-------|----------------|--------------|-------|-----------------|-------------------|-----------|-------|
| **UTCID05** | Doctor has shifts | Doctor has shifts in the specified date range | doctorId=1, from=2025-01-01, to=2025-12-31 | 200 OK | success=true, data=[list of shifts] | None | Returns shifts list |
| **UTCID06** | Doctor has no shifts | Doctor has no shifts in the specified date range | doctorId=999, from=2025-01-01, to=2025-12-31 | 200 OK | success=true, data=[] | None | Returns empty list |

---

## 4. GetDoctorsBySpecialty

| UTCID | Test Case Name | Precondition | Input | Expected Status | Expected Response | Exception | Notes |
|-------|----------------|--------------|-------|-----------------|-------------------|-----------|-------|
| **UTCID07** | Specialty has doctors | Specialty exists and has doctors | specialty="Nội khoa" | 200 OK | success=true, data=[list of doctors] | None | Returns doctors list |
| **UTCID08** | Specialty has no doctors | Specialty exists but has no doctors, or specialty doesn't exist | specialty="Không tồn tại" | 200 OK | success=true, data=[] | None | Returns empty list |

---

## Test Coverage Summary

| Test Method | Total Test Cases | Implemented | Coverage |
|-------------|------------------|-------------|----------|
| CreateShiftSwapRequest | 2 | 2 | 100% |
| GetRequestsByDoctorId | 2 | 2 | 100% |
| GetDoctorShifts | 2 | 2 | 100% |
| GetDoctorsBySpecialty | 2 | 2 | 100% |
| **TOTAL** | **8** | **8** | **100%** |

---

## Test Implementation Status

### ✅ All Test Cases Implemented
- CreateShiftSwapRequest (2/2) ✅
- GetRequestsByDoctorId (2/2) ✅
- GetDoctorShifts (2/2) ✅
- GetDoctorsBySpecialty (2/2) ✅

---

## Notes
- All test cases use Moq for mocking service dependencies
- Reflection is used to assert anonymous type responses from controllers
- Test methods follow AAA pattern (Arrange-Act-Assert)
- Helper methods are used to reduce code duplication
