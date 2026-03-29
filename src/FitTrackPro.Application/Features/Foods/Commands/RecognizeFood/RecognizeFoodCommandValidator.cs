namespace FitTrackPro.Application.Features.Foods.Commands.RecognizeFood;
 
using FluentValidation;
 
public class RecognizeFoodCommandValidator : AbstractValidator<RecognizeFoodCommand>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
 
    public RecognizeFoodCommandValidator()
    {
        RuleFor(x => x.Image)
            .NotNull()
            .WithMessage("Vui lòng chọn ảnh.");
 
        RuleFor(x => x.Image.Length)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("Ảnh không được vượt quá 10MB.");
 
        RuleFor(x => x.Image.FileName)
            .Must(HaveAllowedExtension)
            .WithMessage("Chỉ chấp nhận ảnh định dạng JPG, JPEG, PNG hoặc WebP.");
 
        RuleFor(x => x.Image.ContentType)
            .Must(BeImageContentType)
            .WithMessage("Content type không hợp lệ. Phải là image/jpeg, image/png hoặc image/webp.");
    }
 
    private static bool HaveAllowedExtension(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(ext);
    }
 
    private static bool BeImageContentType(string contentType)
    {
        return contentType is "image/jpeg" or "image/png" or "image/webp" or "image/jpg";
    }
}