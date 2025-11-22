namespace FitTrackPro.Application.Features.Foods.Queries.GetFoodById;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;

public record GetFoodByIdQuery(Guid FoodId) : IRequest<Result<FoodDetailDto>>;
