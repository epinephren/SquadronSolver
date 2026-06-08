using Dalamud.Bindings.ImGui;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using SquadronSolver.Services;
using SquadronSolver.Windows;

namespace SquadronSolver;

public sealed class SquadronSolverPlugin : IDalamudPlugin
{
    private const string MainCommand = "/squadsolver";
    private const string ShortCommand = "/sqs";

    [PluginService] private static IDalamudPluginInterface PluginInterface { get; set; } = null!;
    [PluginService] private static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] private static IGameGui GameGui { get; set; } = null!;
    [PluginService] private static IPluginLog PluginLog { get; set; } = null!;

    private readonly Configuration configuration;
    private readonly WindowSystem windowSystem = new("SquadronSolver");
    private readonly GcArmyExpeditionReader expeditionReader;
    private readonly GcArmyMemberListReader memberListReader;
    private readonly MainWindow mainWindow;
    private readonly ConfigWindow configWindow;

    public SquadronSolverPlugin()
    {
        this.configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        this.configuration.Initialize(PluginInterface);

        this.expeditionReader = new GcArmyExpeditionReader(GameGui);
        this.memberListReader = new GcArmyMemberListReader(GameGui);

        this.mainWindow = new MainWindow(
            this.expeditionReader,
            this.memberListReader);

        this.configWindow = new ConfigWindow(this.configuration);

        this.windowSystem.AddWindow(this.mainWindow);
        this.windowSystem.AddWindow(this.configWindow);

        CommandManager.AddHandler(MainCommand, new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Open the Squadron Solver window.",
        });

        CommandManager.AddHandler(ShortCommand, new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Shortcut for opening the Squadron Solver window.",
        });

        PluginInterface.UiBuilder.Draw += this.DrawUi;
        PluginInterface.UiBuilder.OpenMainUi += this.OpenMainUi;
        PluginInterface.UiBuilder.OpenConfigUi += this.OpenConfigUi;

        PluginLog.Information("SquadronSolver loaded.");
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= this.DrawUi;
        PluginInterface.UiBuilder.OpenMainUi -= this.OpenMainUi;
        PluginInterface.UiBuilder.OpenConfigUi -= this.OpenConfigUi;

        CommandManager.RemoveHandler(MainCommand);
        CommandManager.RemoveHandler(ShortCommand);

        this.windowSystem.RemoveAllWindows();
        this.mainWindow.Dispose();
    }

    private void OnCommand(string command, string arguments)
    {
        this.OpenMainUi();
    }

    private void DrawUi()
    {
        this.configWindow.Flags = this.configuration.IsConfigWindowMovable
            ? ImGuiWindowFlags.None
            : ImGuiWindowFlags.NoMove;

        if (this.configuration.AutoOpenWithSquadronMissions
            && this.expeditionReader.TryRead(out _))
        {
            this.mainWindow.IsOpen = true;
        }

        this.windowSystem.Draw();
    }

    private void OpenMainUi()
    {
        this.mainWindow.IsOpen = true;
    }

    private void OpenConfigUi()
    {
        this.configWindow.IsOpen = true;
    }
}