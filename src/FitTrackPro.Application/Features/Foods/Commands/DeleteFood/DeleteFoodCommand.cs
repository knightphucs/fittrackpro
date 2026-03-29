using FitTrackPro.Application.Common.Models;
using MediatR;

namespace FitTrackPro.Application.Features.Foods.Commands.DeleteFood;

public record DeleteFoodCommand(Guid FoodId) : IRequest<Result<Unit>>;