namespace FitTrackPro.Domain.Entities;

using FitTrackPro.Domain.Common;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = default!;
    public string JwtId { get; set; } = default!;
    public DateTime CreationDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    
    public bool Used { get; set; } 
    public bool Invalidated { get; set; } 

    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
}