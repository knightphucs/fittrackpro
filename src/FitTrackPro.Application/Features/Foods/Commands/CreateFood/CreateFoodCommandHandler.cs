using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Common.Utils;
using FitTrackPro.Application.Features.Foods.DTOs;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.ValueObjects;
using MediatR;

namespace FitTrackPro.Application.Features.Foods.Commands.CreateFood;

public class CreateFoodCommandHandler : IRequestHandler<CreateFoodCommand, Result<FoodDto>>
{
    private readonly ISearchService _searchService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IApplicationDbContext _context;

    public CreateFoodCommandHandler(
        ISearchService searchService, 
        IFileStorageService fileStorageService,
        IApplicationDbContext context)
    {
        _searchService = searchService;
        _fileStorageService = fileStorageService;
        _context = context;
    }

    public async Task<Result<FoodDto>> Handle(CreateFoodCommand request, CancellationToken cancellationToken)
    {
        string? imageUrl = null;
        if (request.ImageFile != null)
        {
            var fileName = $"foods/{Guid.NewGuid()}{Path.GetExtension(request.ImageFile.FileName)}";

            imageUrl = await _fileStorageService.UploadWithCompressionAsync(
                request.ImageFile, 
                fileName, 
                maxWidth: 800,
                quality: 80, 
                cancellationToken: cancellationToken
            );
        }

        var nameInput = request.Name?.Trim();
        var nameViInput = request.NameVi?.Trim();
        
        string finalName = nameInput ?? string.Empty;
        string? finalNameVi = nameViInput;

        if (string.IsNullOrEmpty(finalNameVi) && !string.IsNullOrEmpty(finalName))
        {
            finalNameVi = finalName;
            finalName = StringUtils.RemoveDiacritics(finalName);
        }
        else if (string.IsNullOrEmpty(finalName) && !string.IsNullOrEmpty(finalNameVi))
        {
            finalName = StringUtils.RemoveDiacritics(finalNameVi);
        }

        var macros = new MacroNutrients(request.Protein, request.Carbohydrates, request.Fats);
        var caloriesValue = macros.CalculateCalories();

        var food = Food.Create(
            finalName,
            finalNameVi,
            request.Category,
            request.ServingSize,
            request.ServingUnit,
            caloriesValue,
            macros,
            request.Fiber,
            request.Sugar,
            imageUrl
        );

        _context.Foods.Add(food);
        await _context.SaveChangesAsync(cancellationToken);

        var foodDto = new FoodDto
        {
            Id = food.Id,
            Name = food.Name,
            NameVi = food.NameVi,
            Category = food.Category,
            ServingSize = food.ServingSize,
            ServingUnit = food.ServingUnit,
            Calories = food.Calories,
            Protein = food.Macros.Protein,
            Carbs = food.Macros.Carbs,
            Fat = food.Macros.Fat,
            Fiber = food.Fiber,
            Sugar = food.Sugar,
            ImageUrl = food.ImageUrl
        };

        // Elasticsearch (Search Service)
        try
        {
            await _searchService.IndexFoodAsync(foodDto, cancellationToken);
        }
        catch
        {
            return Result<FoodDto>.Failure("Saved to DB but failed to index search.");
        }

        return Result<FoodDto>.Success(foodDto);
    }
}