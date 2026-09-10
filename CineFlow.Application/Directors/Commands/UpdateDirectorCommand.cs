using System;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Directors.Commands;

public record UpdateDirectorCommand(Guid Id, string Name, string Bio) : IRequest<bool>;

public class UpdateDirectorCommandHandler : IRequestHandler<UpdateDirectorCommand, bool>
{
    private readonly ICineFlowDbContext _context;

    public UpdateDirectorCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(UpdateDirectorCommand request, CancellationToken cancellationToken)
    {
        var director = await _context.Directors
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (director == null) return false;

        director.Name = request.Name.Trim();
        director.Bio = request.Bio?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
