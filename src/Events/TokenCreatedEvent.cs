using System.Text;

namespace P2PVTT.Events;

public record TokenCreatedEvent(Guid Id, string TokenName, uint TextureId)
{
    public TokenCreatedEvent(Guid id, byte[] tokenName, uint textureId)
        : this(id, Encoding.UTF8.GetString(tokenName), textureId) { }
}
