
namespace Common.ValueObjects;
public enum ActorKind
{
    User,
    System,
    Job,
    Worker,
    Consumer
}
public readonly record struct Actor(ActorKind Kind, string Value)
{
    public static Actor User(string value) => new Actor(ActorKind.User, value);
    public static Actor System(string value) => new Actor(ActorKind.System, value);
    public static Actor Job(string value) => new Actor(ActorKind.Job, value);
    public static Actor Worker(string value) => new Actor(ActorKind.Worker, value);
    public static Actor Consumer(string value) => new Actor(ActorKind.Consumer, value);
    public override string ToString()
    {
        if (Kind == ActorKind.User) return Value!;
        return $"{Kind.ToString().ToLowerInvariant()}:{Value}";
    }
}
