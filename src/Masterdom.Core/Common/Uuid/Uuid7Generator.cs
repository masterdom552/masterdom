namespace Masterdom.Core.Common.Uuid;

public sealed class Uuid7Generator : IUuidGenerator
{
    public Guid New()
    {
        return Guid.CreateVersion7();
    }
}
