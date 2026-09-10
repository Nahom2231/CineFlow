using System;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Stars.Commands;

public record UpdateStarCommand(Guid Id, string Name, string Bio) : IRequest<bool>;

public class UpdateStarCommandHandler : IRequestHandler<UpdateStarCommand, bool>
{
    private readonly ICineFlowDbContext _context;

    public UpdateStarCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateStarCommand request, CancellationToken cancellationToken)
    {
        var star = await _context.Stars
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (star == null) return false;

        star.Name = request.Name.Trim();
        star.Bio = request.Bio?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
