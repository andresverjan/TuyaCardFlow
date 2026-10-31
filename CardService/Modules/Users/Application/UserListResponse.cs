namespace TuyaCardFlow.Modules.Users.Application;

public sealed record UserListResponse(
    int Page, int PageSize, int TotalCount, IReadOnlyList<UserResponse> Items);
