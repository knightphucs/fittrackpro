using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FitTrackPro.Application.Features.Foods.Commands.CreateFood;

public class CreateFoodCommand : IRequest<Result<FoodDto>>
{
    public string Name { get; set; } = null!;
    public string? NameVi { get; set; }
    public string Category { get; set; } = null!;
    public decimal ServingSize { get; set; } = 1;
    public string ServingUnit { get; set; } = "serving";
    public decimal? Fiber { get; set; }
    public decimal? Sugar { get; set; }
    public int Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Fats { get; set; }
    public IFormFile? ImageFile { get; set; }
}