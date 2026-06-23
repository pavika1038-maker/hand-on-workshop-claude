namespace LeaveRequest.Infrastructure.Adapters;

public sealed class HrisOptions
{
    public const string SectionName = "Hris";

    public bool UseStub { get; set; } = false;
    public string BaseUrl { get; set; } = string.Empty;
    public string TokenEndpoint { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string EmployeesEndpoint { get; set; } = "/api/v1/employees";
}
