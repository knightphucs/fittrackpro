namespace FitTrackPro.API.Errors;
public abstract class ApiErrorResponse
{
    public int StatusCode { get; set; }
    public string? Message { get; set; }
}
