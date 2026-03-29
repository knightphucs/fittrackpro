namespace FitTrackPro.Application.Features.Foods.Commands.RecognizeFood;
 
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
 
public record RecognizeFoodCommand(
    IFormFile Image
) : IRequest<Result<FoodRecognitionDto>>;