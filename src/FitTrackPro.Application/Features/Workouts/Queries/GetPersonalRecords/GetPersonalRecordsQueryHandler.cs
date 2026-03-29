using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using FitTrackPro.Domain.Repositories;
using MediatR;

namespace FitTrackPro.Application.Features.Workouts.Queries.GetPersonalRecords;

public class GetPersonalRecordsQueryHandler 
    : IRequestHandler<GetPersonalRecordsQuery, Result<List<PersonalRecordDto>>>
{
    private readonly IPersonalRecordRepository _prRepository;

    public GetPersonalRecordsQueryHandler(IPersonalRecordRepository prRepository)
    {
        _prRepository = prRepository;
    }

    public async Task<Result<List<PersonalRecordDto>>> Handle(
        GetPersonalRecordsQuery request,
        CancellationToken cancellationToken)
    {
        var records = await _prRepository.GetByUserIdAsync(
            request.UserId, 
            request.ExerciseId, 
            cancellationToken
        );

        var dtos = records.Select(pr => new PersonalRecordDto
        {
            Id = pr.Id,
            ExerciseId = pr.ExerciseId,
            ExerciseName = pr.ExerciseName,
            Type = pr.Type.ToString(),
            Value = pr.Value,
            Unit = pr.Unit,
            AchievedAt = pr.AchievedAt
        }).ToList();

        return Result<List<PersonalRecordDto>>.Success(dtos);
    }
}
