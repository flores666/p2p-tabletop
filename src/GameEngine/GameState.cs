using P2PVTT.Events;

namespace P2PVTT.GameEngine;

public class GameState
{
    private Dictionary<Guid, uint> _tokensWithTextures;
    private Dictionary<Guid, uint> _mapsWithTextures;
    private List<Guid> _players;

    public GameState()
    {
        _tokensWithTextures = new Dictionary<Guid, uint>();
        _mapsWithTextures = new Dictionary<Guid, uint>();
        _players = new List<Guid>();
    }

    public void HandleTokenCreatedEvent(object? sender, TokenCreatedEvent e)
    {
        _tokensWithTextures[e.Id] = e.TextureId;
    }
}
