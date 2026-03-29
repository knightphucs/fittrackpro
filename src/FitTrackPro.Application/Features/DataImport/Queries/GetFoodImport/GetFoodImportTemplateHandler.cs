namespace FitTrackPro.Application.Features.DataImport.Queries.GetFoodImport;

using MediatR;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Common.Interfaces;

public class GetFoodImportTemplateHandler : IRequestHandler<GetFoodImportTemplateQuery, Result<byte[]>>
{
    private readonly IDataImportService _importService;

    public GetFoodImportTemplateHandler(IDataImportService importService)
    {
        _importService = importService;
    }

    public async Task<Result<byte[]>> Handle(GetFoodImportTemplateQuery request, CancellationToken cancellationToken)
    {
        var fileContent = await _importService.GetFoodImportTemplateAsync(cancellationToken);
        return Result<byte[]>.Success(fileContent);
    }
}