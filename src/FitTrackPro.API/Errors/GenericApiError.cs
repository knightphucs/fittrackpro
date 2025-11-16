namespace FitTrackPro.API.Errors;

public class GenericApiError : ApiErrorResponse
{
    // Use [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    // in a real app to hide this when null
    public string? Details { get; set; }
}