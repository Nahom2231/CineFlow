using System;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Domain.Entities;
using MediatR;

namespace CineFlow.Application.Directors.Commands;

public record CreateDirectorCommand(string Name, string Bio) : IRequest<Guid>;

public class CreateDirectorCommandHandler : IRequestHandler<CreateDirectorCommand, Guid>
{
    private readonly ICineFlowDbContext _context;

    public CreateDirectorCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateDirectorCommand request, CancellationToken cancellationToken)
    {
        var director = new Director
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Bio = request.Bio?.Trim() ?? string.Empty
        };

        _context.Directors.Add(director);
        await _context.SaveChangesAsync(cancellationToken);

        return director.Id;
    }
}
