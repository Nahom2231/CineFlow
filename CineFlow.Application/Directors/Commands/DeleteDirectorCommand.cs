using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Directors.Commands;

public record DeleteDirectorCommand(Guid Id) : IRequest<bool>;

public class DeleteDirectorCommandHandler : IRequestHandler<DeleteDirectorCommand, bool>
{
    private readonly ICineFlowDbContext _context;

    public DeleteDirectorCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteDirectorCommand request, CancellationToken cancellationToken)
    {
        var director = await _context.Directors
            .Include(d => d.Movies)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (director == null) return false;

        foreach (var movie in director.Movies)
        {
            movie.DirectorId = null;
        }

        _context.Directors.Remove(director);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
