using System.Text;

namespace P2PVTT.Modules.Events;

public record TokenCreatedEvent(string TokenName, uint TextureId)
{
    public TokenCreatedEvent(byte[] tokenName, uint textureId)
        : this(Encoding.UTF8.GetString(tokenName), textureId) { }
}
