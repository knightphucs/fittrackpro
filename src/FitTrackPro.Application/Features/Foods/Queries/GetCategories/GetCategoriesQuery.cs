namespace FitTrackPro.Application.Features.Foods.Queries.GetCategories;

using MediatR;
using FitTrackPro.Application.Common.Models;

public record GetCategoriesQuery : IRequest<Result<List<string>>>;
