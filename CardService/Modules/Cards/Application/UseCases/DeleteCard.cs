namespace TuyaCardFlow.Modules.Cards.Application.UseCases;

public sealed class DeleteCard(ICardRepository repository)
{
    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var card = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new CardNotFoundException(id);

        repository.Remove(card);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
