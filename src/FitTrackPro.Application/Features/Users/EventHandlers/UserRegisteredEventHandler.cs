namespace FitTrackPro.Application.Features.Users.EventHandlers;

using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.Events;
using FitTrackPro.Application.Common.Interfaces;

public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredEventHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly UserManager<User> _userManager;

    public UserRegisteredEventHandler(
        ILogger<UserRegisteredEventHandler> logger, 
        IEmailService emailService,
        UserManager<User> userManager)
    {
        _logger = logger;
        _emailService = emailService;
        _userManager = userManager;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(notification.UserId.ToString());
        if (user == null) return;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        
        var encodedToken = Uri.EscapeDataString(token); 

        await _emailService.SendEmailConfirmationAsync(user.Email!, encodedToken);
    }
}