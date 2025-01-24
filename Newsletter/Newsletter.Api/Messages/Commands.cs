namespace Newsletter.Api.Messages;

public record SendWelcomeEmail(Guid SubscriberId, string Email);

public record SendFollowUpEmail(Guid SubscriberId, string Email);

public record SubscribeToNewsLetter(string Email);