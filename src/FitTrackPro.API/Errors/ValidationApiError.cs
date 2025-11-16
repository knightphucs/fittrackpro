namespace FitTrackPro.API.Errors;

public class ValidationApiError : ApiErrorResponse
{
    public object? Errors { get; set; }
}
