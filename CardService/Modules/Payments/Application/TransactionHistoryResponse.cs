namespace TuyaCardFlow.Modules.Payments.Application;

public sealed record TransactionHistoryResponse(
    int Page, int PageSize, int TotalCount, IReadOnlyList<TransactionResponse> Items);
