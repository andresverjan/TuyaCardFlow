using TuyaCardFlow.Modules.Cards.Domain;

namespace TuyaCardFlow.Modules.Cards.Application.UseCases;

public sealed class CreateCard(ICardRepository repository)
{
    public async Task<CardResponse> ExecuteAsync(
        CreateCardCommand command, CancellationToken cancellationToken = default)
    {
        var card = Card.Create(
            command.UserId, command.HolderName, command.Brand, command.Last4,
            command.ExpirationMonth, command.ExpirationYear);

        await repository.AddAsync(card, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return CardResponse.From(card);
    }
}
