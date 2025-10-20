namespace Newsletter.Api.Messages;

// Advanced onboarding commands
public record CompleteProfile(Guid SubscriberId, string Email, string FirstName, string LastName);

public record SelectPreferences(Guid SubscriberId, string Email, string[] Topics);

public record SendWelcomePackage(Guid SubscriberId, string Email, string FirstName, string LastName, string[] Topics);

public record ScheduleEngagementEmail(Guid SubscriberId, string Email, DateTime ScheduledAt);

// Compensation commands
public record CompensateProfileCompletion(Guid SubscriberId, string Email);

public record CompensatePreferencesSelection(Guid SubscriberId, string Email);
