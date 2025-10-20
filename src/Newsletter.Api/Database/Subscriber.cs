namespace Newsletter.Api.Database;

public class Subscriber
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime SubscribedOnUtc { get; set; }
}
