namespace TuyaCardFlow.Modules.Cards.Application.UseCases;

public sealed class GetCardById(ICardRepository repository)
{
    public async Task<CardResponse> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var card = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new CardNotFoundException(id);

        return CardResponse.From(card);
    }
}
