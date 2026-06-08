using Dalamud.Configuration;
using Dalamud.Plugin;

namespace SquadronSolver;

[Serializable]
public sealed class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool AutoOpenWithSquadronMissions { get; set; } = true;

    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
        this.pluginInterface?.SavePluginConfig(this);
    }
}