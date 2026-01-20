using Catalog.Application.Services;

namespace Catalog.Application.Features.Product.System;

public sealed record InitialDataCommand(Actor Actor): ICommand<bool>;
public sealed class InitialDataCommandHandler(IDocumentSession session, ISeedDataServices seedDataService) : ICommandHandler<InitialDataCommand, bool>
{
    public async Task<bool> Handle(InitialDataCommand request, CancellationToken cancellationToken)
    {
        var result = await seedDataService.SeedDataAsync(session, cancellationToken);
        return result;
    }
}