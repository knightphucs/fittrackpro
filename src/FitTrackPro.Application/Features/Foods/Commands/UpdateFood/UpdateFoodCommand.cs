namespace FitTrackPro.Application.Features.Foods.Commands.UpdateFood;

using System;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Features.Foods.DTOs;
using FitTrackPro.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Http;

public class UpdateFoodCommand : IRequest<Result<FoodDto>>
{
    public Guid FoodId { get; set; }
    public string Name { get; set; } = default!;
    public string? NameVi { get; set; }
    public string? Category { get; set; }
    public decimal ServingSize { get; set; }
    public string ServingUnit { get; set; } = default!;
    public int Calories { get; set; }
    public decimal? Protein { get; set; }
    public decimal? Carbohydrates { get; set; }
    public decimal? Fats { get; set; }
    public decimal? Fiber { get; set; }
    public decimal? Sugar { get; set; }
    public IFormFile? ImageFile { get; set; }
} 