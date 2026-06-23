namespace LeaveRequest.Application.Tests.Services;

using FluentAssertions;
using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Services;
using LeaveRequest.Domain.Entities;
using LeaveRequest.Domain.Exceptions;
using LeaveRequest.Domain.Interfaces;
using LeaveRequest.Domain.Interfaces.Repositories;
using Moq;

public class LeaveTypeServiceTests
{
    private readonly Mock<ILeaveTypeRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly LeaveTypeService _sut;

    public LeaveTypeServiceTests()
    {
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

        _sut = new LeaveTypeService(_repoMock.Object, _uowMock.Object);
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_WhenEntitiesExist_ReturnsMappedDtos()
    {
        // Arrange
        var entities = new List<LeaveType>
        {
            new()
            {
                LeaveTypeId = 1,
                TypeCode = "ANNUAL",
                TypeNameTh = "ลาพักร้อน",
                TypeNameEn = "Annual Leave",
                MaxDaysPerYear = 10,
                IsAvailableForOutsource = false,
                RequiresMedicalCert = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "SYSTEM"
            },
            new()
            {
                LeaveTypeId = 2,
                TypeCode = "SICK",
                TypeNameTh = "ลาป่วย",
                TypeNameEn = "Sick Leave",
                MaxDaysPerYear = 30,
                IsAvailableForOutsource = true,
                RequiresMedicalCert = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "SYSTEM"
            }
        };

        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(entities);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].TypeCode.Should().Be("ANNUAL");
        result[0].TypeNameTh.Should().Be("ลาพักร้อน");
        result[1].TypeCode.Should().Be("SICK");
        result[1].RequiresMedicalCert.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_WhenNoEntities_ReturnsEmptyList()
    {
        _repoMock.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<LeaveType>());

        var result = await _sut.GetAllAsync();

        result.Should().BeEmpty();
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_HappyPath_AddsEntityAndSaves()
    {
        // Arrange
        var request = new CreateLeaveTypeRequest(
            TypeCode: "PERSONAL",
            TypeNameTh: "ลากิจ",
            TypeNameEn: "Personal Leave",
            MaxDaysPerYear: 3,
            IsAvailableForOutsource: false,
            RequiresMedicalCert: false
        );

        _repoMock.Setup(x => x.ExistsByCodeAsync("PERSONAL", null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

        _repoMock.Setup(x => x.AddAsync(It.IsAny<LeaveType>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((LeaveType lt, CancellationToken _) =>
                 {
                     lt.LeaveTypeId = 3;
                     return lt;
                 });

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.TypeCode.Should().Be("PERSONAL");
        result.TypeNameTh.Should().Be("ลากิจ");
        result.MaxDaysPerYear.Should().Be(3);

        _repoMock.Verify(x => x.AddAsync(
            It.Is<LeaveType>(lt => lt.TypeCode == "PERSONAL" && lt.CreatedBy == "SYSTEM"),
            It.IsAny<CancellationToken>()), Times.Once);

        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateTypeCode_ThrowsBusinessException()
    {
        // Arrange
        var request = new CreateLeaveTypeRequest(
            TypeCode: "ANNUAL",
            TypeNameTh: "ลาพักร้อน",
            TypeNameEn: "Annual Leave",
            MaxDaysPerYear: 10,
            IsAvailableForOutsource: false,
            RequiresMedicalCert: false
        );

        _repoMock.Setup(x => x.ExistsByCodeAsync("ANNUAL", null, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true); // duplicate!

        // Act
        var act = async () => await _sut.CreateAsync(request);

        // Assert
        await act.Should()
            .ThrowAsync<BusinessException>()
            .WithMessage("*ANNUAL*")
            .Where(ex => ex.Code == "DUPLICATE_TYPE_CODE");

        _repoMock.Verify(x => x.AddAsync(It.IsAny<LeaveType>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Delete (soft delete) ─────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingId_SetsIsDeletedAndSaves()
    {
        // Arrange
        var existingEntity = new LeaveType
        {
            LeaveTypeId = 1,
            TypeCode = "ANNUAL",
            TypeNameTh = "ลาพักร้อน",
            TypeNameEn = "Annual Leave",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "SYSTEM"
        };

        _repoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(existingEntity);

        // Act
        await _sut.DeleteAsync(1, "admin@company.com");

        // Assert — soft delete markers set
        existingEntity.IsDeleted.Should().BeTrue();
        existingEntity.DeletedBy.Should().Be("admin@company.com");
        existingEntity.DeletedAt.Should().NotBeNull();
        existingEntity.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));

        _repoMock.Verify(x => x.Update(existingEntity), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFoundId_ThrowsNotFoundException()
    {
        _repoMock.Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                 .ReturnsAsync((LeaveType?)null);

        var act = async () => await _sut.DeleteAsync(99, "admin@company.com");

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*99*");
    }
}
