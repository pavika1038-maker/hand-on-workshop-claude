# Unit Test Output Template

> Template สำหรับ unit test ที่ครอบคลุม happy path และ error path  
> Tech Stack: xUnit, Moq (หรือ NSubstitute), FluentAssertions  
> Target: Service layer (business logic) เป็นหลัก

---

## 1. Document Info

| รายการ | รายละเอียด |
|--------|-----------|
| Feature / Use Case | {use_case_name} |
| Class Under Test | `{ClassName}` |
| Related SRS | {requirement_id} |

---

## 2. Test File Structure

```csharp
// File: tests/LeaveRequest.Application.Tests/Services/{ClassName}Tests.cs
namespace LeaveRequest.Application.Tests.Services;

public class {ClassName}Tests
{
    // --- Mocks ---
    private readonly Mock<I{Repository}> _{mockRepository};
    private readonly Mock<ILogger<{ClassName}>> _mockLogger;
    private readonly AppDbContext _dbContext;  // ใช้ InMemory หรือ SQLite in-memory

    // --- System Under Test (SUT) ---
    private readonly {ClassName} _sut;

    public {ClassName}Tests()
    {
        _{mockRepository} = new Mock<I{Repository}>();
        _mockLogger = new Mock<ILogger<{ClassName}>>();

        // Setup in-memory DB
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        _dbContext = new AppDbContext(options);
        _dbContext.Database.EnsureCreated();

        _sut = new {ClassName}(
            _{mockRepository}.Object,
            _dbContext,
            _mockLogger.Object
        );
    }

    // --- Test Methods ---
}
```

---

## 3. Template: Happy Path Test

```csharp
[Fact]
public async Task {MethodName}_Should_{ExpectedBehavior}_When_{Condition}()
{
    // Arrange
    var request = new {RequestDTO}(
        {property1} = {value1},
        {property2} = {value2}
    );

    _{mockRepository}
        .Setup(x => x.{MethodName}(It.IsAny<{Type}>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync({mockReturnValue});

    // Act
    var result = await _sut.{MethodName}Async(request, "test-user");

    // Assert
    result.Should().NotBeNull();
    result.{Property}.Should().Be({expectedValue});

    _{mockRepository}.Verify(
        x => x.AddAsync(It.IsAny<{Entity}>(), It.IsAny<CancellationToken>()),
        Times.Once);
}
```

---

## 4. Template: Business Rule Violation Test

```csharp
[Fact]
public async Task {MethodName}_Should_ThrowBusinessException_When_{BusinessRuleViolated}()
{
    // Arrange
    var request = new {RequestDTO}(
        {property} = {value_that_violates_rule}
    );

    _{mockRepository}
        .Setup(x => x.{MethodName}(It.IsAny<{Type}>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(({ReturnType}?)null);  // simulate not found / rule violated

    // Act
    var act = async () => await _sut.{MethodName}Async(request, "test-user");

    // Assert
    await act.Should()
        .ThrowAsync<BusinessException>()
        .WithMessage("{expected_error_message}");
}
```

---

## 5. Template: Not Found Test

```csharp
[Fact]
public async Task Get{Entity}ById_Should_ThrowNotFoundException_When_EntityDoesNotExist()
{
    // Arrange
    const int nonExistentId = 99999;

    _{mockRepository}
        .Setup(x => x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
        .ReturnsAsync((Domain.Entities.{Entity}?)null);

    // Act
    var act = async () => await _sut.Get{Entity}ByIdAsync(nonExistentId);

    // Assert
    await act.Should()
        .ThrowAsync<NotFoundException>()
        .WithMessage($"*{nonExistentId}*");
}
```

---

## 6. Template: Theory (Parameterized) Test

```csharp
[Theory]
[InlineData({param1_case1}, {param2_case1}, "{expected_error_case1}")]
[InlineData({param1_case2}, {param2_case2}, "{expected_error_case2}")]
public async Task {MethodName}_Should_Fail_When_{Condition}(
    {Type} {param1},
    {Type} {param2},
    string expectedError)
{
    // Arrange
    var request = new {RequestDTO}({param1}, {param2});

    // Act
    var act = async () => await _sut.{MethodName}Async(request, "test-user");

    // Assert
    await act.Should()
        .ThrowAsync<BusinessException>()
        .WithMessage(expectedError);
}
```

---

## 7. Test Naming Convention

```
{MethodName}_Should_{ExpectedBehavior}_When_{Condition}

ตัวอย่าง:
✅ SubmitLeaveRequest_Should_CreateLeaveRequest_When_ValidRequestProvided
✅ SubmitLeaveRequest_Should_ThrowBusinessException_When_InsufficientBalance
✅ SubmitLeaveRequest_Should_ThrowBusinessException_When_StartDateIsInPast
✅ GetLeaveRequestById_Should_ReturnNull_When_EntityNotFound
✅ CancelLeaveRequest_Should_ThrowBusinessException_When_AlreadyApproved
```

---

## 8. Test Coverage Checklist

สำหรับแต่ละ Service method ต้องมี test ครอบคลุม:

- [ ] Happy path (ทำงานสำเร็จ)
- [ ] Validation rule แต่ละข้อ (ถ้า method มี business validation)
- [ ] Not found case (ถ้า method อ่านข้อมูลจาก DB)
- [ ] State transition ที่ไม่ถูกต้อง (เช่น cancel ใบลาที่ approved แล้ว)
- [ ] Repository method ถูกเรียก/ไม่ถูกเรียกตามที่คาดไว้ (Verify)
- [ ] SaveChangesAsync ถูกเรียก (สำหรับ write operation)
- [ ] Log ถูกเรียกใน error path (LogError)

---

## 9. Dependencies

```xml
<!-- tests/LeaveRequest.Application.Tests/LeaveRequest.Application.Tests.csproj -->
<PackageReference Include="xunit" Version="2.9.*" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.8.*" />
<PackageReference Include="Moq" Version="4.20.*" />
<PackageReference Include="FluentAssertions" Version="6.12.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.*" />
<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
```
