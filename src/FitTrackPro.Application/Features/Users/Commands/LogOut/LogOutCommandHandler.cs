namespace FitTrackPro.Application.Features.Users.Commands.LogOut;

using FitTrackPro.Application.Common.Interfaces;
using FitTrackPro.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class LogOutCommandHandler : IRequestHandler<LogoutCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;

    public LogOutCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == request.RefreshToken, cancellationToken);

        if (storedToken == null)
        {
            return Result<string>.Success("Logged out successfully.");
        }

        storedToken.Invalidated = true;

        // Nếu bạn muốn xóa khỏi DB để tiết kiệm dung lượng:
        // _context.RefreshTokens.Remove(storedToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Logged out successfully.");
    }
}