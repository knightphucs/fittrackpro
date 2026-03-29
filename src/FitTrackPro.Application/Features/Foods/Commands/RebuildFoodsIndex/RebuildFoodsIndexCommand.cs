using FitTrackPro.Application.Common.Models;
using MediatR;

namespace FitTrackPro.Application.Features.Foods.Commands.RebuildFoodsIndex;

public class RebuildFoodsIndexCommand : IRequest<Result<bool>>
{
}
