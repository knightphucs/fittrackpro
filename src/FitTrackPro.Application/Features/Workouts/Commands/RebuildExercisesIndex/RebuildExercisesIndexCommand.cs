namespace FitTrackPro.Application.Features.Workouts.Commands.RebuildExercisesIndex;

using FitTrackPro.Application.Common.Models;
using MediatR;

public record RebuildExercisesIndexCommand : IRequest<Result<Unit>>;
