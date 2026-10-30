namespace TuyaCardFlow.Modules.Cards.Application.UseCases;

public sealed class UpdateCard(ICardRepository repository)
{
    public async Task<CardResponse> ExecuteAsync(
        Guid id, UpdateCardCommand command, CancellationToken cancellationToken = default)
    {
        var card = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new CardNotFoundException(id);

        card.Update(command.HolderName, command.Brand, command.Last4,
            command.ExpirationMonth, command.ExpirationYear);
        await repository.SaveChangesAsync(cancellationToken);

        return CardResponse.From(card);
    }
}
