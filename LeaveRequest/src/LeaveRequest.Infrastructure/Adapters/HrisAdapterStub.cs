namespace LeaveRequest.Infrastructure.Adapters;

using LeaveRequest.Application.DTOs;
using LeaveRequest.Application.Interfaces;

/// <summary>
/// Dev/test stub — returns hardcoded employee data without calling the real HRIS API.
/// Activated via appsettings.json: "Hris": { "UseStub": true }
/// </summary>
public sealed class HrisAdapterStub : IHrisAdapter
{
    private static readonly IReadOnlyList<HrisEmployeeDto> StubData =
    [
        new("EMP001", "E001", "สมชาย ใจดี",      "Somchai Jaidee",   "Information Technology", "Software Engineer", "somchai@abc.com", new DateOnly(2022,  1, 15), "MGR001", true),
        new("EMP002", "E002", "สมหญิง รักดี",     "Somying Rakdee",   "Human Resources",        "HR Specialist",     "somying@abc.com", new DateOnly(2021,  6,  1), "MGR002", true),
        new("EMP003", "E003", "ประสิทธิ์ มั่นใจ",  "Prasit Manjay",    "Information Technology", "QA Engineer",       "prasit@abc.com",  new DateOnly(2023,  3, 20), "MGR001", true),
        new("MGR001", "M001", "วิชัย สุขใจ",       "Vichai Sukchai",   "Information Technology", "IT Manager",        "vichai@abc.com",  new DateOnly(2019,  3,  1), null,     true),
        new("MGR002", "M002", "ประภา รุ่งเรือง",   "Prapa Rungruang",  "Human Resources",        "HR Manager",        "prapa@abc.com",   new DateOnly(2018,  9,  1), null,     true),
    ];

    public Task<IEnumerable<HrisEmployeeDto>> GetAllEmployeesAsync(CancellationToken ct = default)
        => Task.FromResult<IEnumerable<HrisEmployeeDto>>(StubData);

    public Task<IEnumerable<HrisEmployeeDto>> ParseBatchFileAsync(Stream csvStream, CancellationToken ct = default)
        => Task.FromResult<IEnumerable<HrisEmployeeDto>>(StubData);
}
