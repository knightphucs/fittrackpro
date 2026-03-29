namespace FitTrackPro.Application.Features.DataImport.Queries.GetFoodImport;

using MediatR;
using FitTrackPro.Application.Common.Models;

public record GetFoodImportTemplateQuery() : IRequest<Result<byte[]>>;