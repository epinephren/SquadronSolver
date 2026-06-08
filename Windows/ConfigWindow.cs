using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using System.Numerics;

namespace SquadronSolver.Windows;

public sealed class ConfigWindow : Window
{
    private readonly Configuration configuration;

    public ConfigWindow(Configuration configuration)
        : base("Squadron Solver Settings###SquadronSolverConfig")
    {
        this.configuration = configuration;

        this.Size = new Vector2(420, 180) * ImGuiHelpers.GlobalScale;
        this.SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var movable = this.configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Config window movable", ref movable))
            this.configuration.IsConfigWindowMovable = movable;

        var autoOpen = this.configuration.AutoOpenWithSquadronMissions;
        if (ImGui.Checkbox("Auto-open with Squadron Missions", ref autoOpen))
            this.configuration.AutoOpenWithSquadronMissions = autoOpen;

        ImGui.Separator();

        if (ImGui.Button("Save"))
            this.configuration.Save();
    }
}