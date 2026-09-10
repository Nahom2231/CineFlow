using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Stars.Commands;

public record DeleteStarCommand(Guid Id) : IRequest<bool>;

public class DeleteStarCommandHandler : IRequestHandler<DeleteStarCommand, bool>
{
    private readonly ICineFlowDbContext _context;

    public DeleteStarCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteStarCommand request, CancellationToken cancellationToken)
    {
        var star = await _context.Stars
            .Include(s => s.Movies)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (star == null) return false;

        _context.Stars.Remove(star);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
