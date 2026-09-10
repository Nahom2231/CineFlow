using System;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Domain.Entities;
using MediatR;

namespace CineFlow.Application.Stars.Commands;

public record CreateStarCommand(string Name, string Bio) : IRequest<Guid>;

public class CreateStarCommandHandler : IRequestHandler<CreateStarCommand, Guid>
{
    private readonly ICineFlowDbContext _context;

    public CreateStarCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStarCommand request, CancellationToken cancellationToken)
    {
        var star = new Star
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Bio = request.Bio?.Trim() ?? string.Empty
        };

        _context.Stars.Add(star);
        await _context.SaveChangesAsync(cancellationToken);

        return star.Id;
    }
}
