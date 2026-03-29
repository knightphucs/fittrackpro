using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using MediatR;

namespace FitTrackPro.Application.Features.Foods.Commands.DeleteFood;

public class DeleteFoodCommandHandler : IRequestHandler<DeleteFoodCommand, Result<Unit>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISearchService _searchService;

    public DeleteFoodCommandHandler(
        IApplicationDbContext context,
        ISearchService searchService)
    {
        _context = context;
        _searchService = searchService;
    }

    public async Task<Result<Unit>> Handle(DeleteFoodCommand request, CancellationToken cancellationToken)
    {
        var food = _context.Foods.Find(request.FoodId);
        if (food == null)
        {
            return Result<Unit>.Failure("Food not found.");
        }

        _context.Foods.Remove(food);
        await _context.SaveChangesAsync(cancellationToken);

        var indexed =  await _searchService.RemoveFoodFromIndexAsync(request.FoodId, cancellationToken);
        if (!indexed)
            return Result<Unit>.Failure("Deleted from DB but failed to remove from search index.");

        return Result<Unit>.Success(Unit.Value);
    }
}