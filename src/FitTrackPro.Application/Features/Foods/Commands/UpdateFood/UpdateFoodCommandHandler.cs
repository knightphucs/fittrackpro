using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using FitTrackPro.Application.Common.Utils;
using FitTrackPro.Application.Features.Foods.DTOs;
using FitTrackPro.Domain.ValueObjects;
using MediatR;

namespace FitTrackPro.Application.Features.Foods.Commands.UpdateFood;

public class UpdateFoodCommandHandler : IRequestHandler<UpdateFoodCommand, Result<FoodDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ISearchService _searchService;
    private readonly IFileStorageService _fileStorageService;

    public UpdateFoodCommandHandler(
        IApplicationDbContext context,
        ISearchService searchService,
        IFileStorageService fileStorageService)
    {
        _context = context;
        _searchService = searchService;
        _fileStorageService = fileStorageService;
    }

    public async Task<Result<FoodDto>> Handle(UpdateFoodCommand request, CancellationToken cancellationToken)
    {
        var food = await _context.Foods.FindAsync([request.FoodId], cancellationToken);
        if (food == null)
        {
            return Result<FoodDto>.Failure("Food not found.");
        }

        // Handle image upload if provided
        string? imageUrl = food.ImageUrl;
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

        MacroNutrients macrosToUpdate;
        int caloriesValue;

        if (request.Protein.HasValue && request.Carbohydrates.HasValue && request.Fats.HasValue)
        {
            macrosToUpdate = new MacroNutrients(request.Protein.Value, request.Carbohydrates.Value, request.Fats.Value);
            caloriesValue = macrosToUpdate.CalculateCalories();
        }
        else
        {
            macrosToUpdate = food.Macros;
            caloriesValue = request.Calories;
        }

        food.Update(
            finalName,
            finalNameVi,
            request.Category,
            request.ServingSize,
            request.ServingUnit,
            caloriesValue,
            macrosToUpdate,
            request.Fiber,
            request.Sugar,
            imageUrl);

        _context.Foods.Update(food);
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

        try
        {
            var indexed = await _searchService.UpdateFoodInIndexAsync(foodDto, cancellationToken);
            
            if (!indexed)
                return Result<FoodDto>.Failure("Updated in DB but failed to update search index.");
        }
        catch
        {
            return Result<FoodDto>.Failure("Updated in DB but failed to update search index.");
        }

        return Result<FoodDto>.Success(foodDto);
    }
}