using CineFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Movies.Commands;

public record DeleteMovieCommand(Guid Id) : IRequest<bool>;

public class DeleteMovieCommandHandler : IRequestHandler<DeleteMovieCommand, bool>
{
    private readonly ICineFlowDbContext _context;

    public DeleteMovieCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .Include(m => m.Schedules)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (movie == null) return false;

        _context.Movies.Remove(movie);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
