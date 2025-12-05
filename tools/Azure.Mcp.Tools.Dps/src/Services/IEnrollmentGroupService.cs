using Azure.Mcp.Tools.Dps.Models;

namespace Azure.Mcp.Tools.Dps.Services;

public interface IEnrollmentGroupService
{
    Task<IList<EnrollmentGroup>> ListAllEnrollmentGroupsAsync(
        string dpsHostName,
        CancellationToken cancellationToken = default);

    Task<EnrollmentGroup> GetEnrollmentGroupAsync(
        string dpsHostName,
        string enrollmentGroupId,
        CancellationToken cancellationToken = default);

    Task<EnrollmentGroup> CreateOrUpdateAsync(
        string dpsHostName,
        EnrollmentGroup enrollmentGroup,
        CancellationToken cancellationToken = default);
}
