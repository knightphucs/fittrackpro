using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using MediatR;

namespace FitTrackPro.Application.Features.DataImport.Commands.ImportFoods;

public class ImportFoodsCommandHandler : IRequestHandler<ImportFoodsCommand, Result<ImportResult>>
{
    private readonly IDataImportService _importService;

    public ImportFoodsCommandHandler(IDataImportService importService)
    {
        _importService = importService;
    }

    public async Task<Result<ImportResult>> Handle(ImportFoodsCommand request, CancellationToken cancellationToken)
    {
        var result = await _importService.ImportFoodsFromCsvAsync(request.UserId, request.FileStream, cancellationToken);
        return Result<ImportResult>.Success(result);
    }
}