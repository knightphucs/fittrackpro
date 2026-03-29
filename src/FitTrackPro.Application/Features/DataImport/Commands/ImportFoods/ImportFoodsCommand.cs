using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using MediatR;

namespace FitTrackPro.Application.Features.DataImport.Commands.ImportFoods;

public record ImportFoodsCommand(Guid UserId, Stream FileStream) : IRequest<Result<ImportResult>>;