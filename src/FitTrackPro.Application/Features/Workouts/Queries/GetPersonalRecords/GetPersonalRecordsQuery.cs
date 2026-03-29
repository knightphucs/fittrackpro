using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Workouts.DTOs;
using MediatR;

namespace FitTrackPro.Application.Features.Workouts.Queries.GetPersonalRecords;

public record GetPersonalRecordsQuery(
    Guid UserId,
    Guid? ExerciseId = null) : IRequest<Result<List<PersonalRecordDto>>>;
