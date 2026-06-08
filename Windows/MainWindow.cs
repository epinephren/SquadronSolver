using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using SquadronSolver.Models;
using SquadronSolver.Services;
using System.Numerics;

namespace SquadronSolver.Windows;

public sealed class MainWindow : Window, IDisposable
{
    private static readonly Vector4 SuccessColor = new(0.25f, 1.0f, 0.25f, 1.0f);
    private static readonly Vector4 ErrorColor = new(1.0f, 0.25f, 0.25f, 1.0f);
    private static readonly Vector4 WarningColor = new(1.0f, 0.75f, 0.2f, 1.0f);

    private readonly SquadronSolverService solverService = new();
    private readonly GcArmyExpeditionReader expeditionReader;
    private readonly GcArmyMemberListReader memberListReader;

    public MainWindow(
        GcArmyExpeditionReader expeditionReader,
        GcArmyMemberListReader memberListReader)
        : base("Squadron Solver###SquadronSolverMain")
    {
        this.expeditionReader = expeditionReader;
        this.memberListReader = memberListReader;

        this.Size = new Vector2(760, 460) * ImGuiHelpers.GlobalScale;
        this.SizeCondition = ImGuiCond.FirstUseEver;
        this.RespectCloseHotkey = true;
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("Squadron Solver");
        ImGui.Separator();

        if (!this.expeditionReader.TryRead(out var expeditionState))
        {
            ImGui.TextColored(WarningColor, "Open your Squadron Mission window.");
            return;
        }

        if (expeditionState.Mission is null)
        {
            ImGui.TextColored(WarningColor, "Unable to read mission requirements.");
            return;
        }

        if (!this.memberListReader.TryRead(out var members))
        {
            ImGui.TextColored(WarningColor, "Open your Squadron Member List.");
            return;
        }

        var mission = expeditionState.Mission;
        var solution = this.solverService.Solve(mission, members);

        if (solution is null)
        {
            ImGui.TextColored(ErrorColor, "NO SUCCESS");
            ImGui.TextColored(WarningColor, "Not enough squadron members.");
            return;
        }

        var shownStrength = expeditionState.CurrentAttributes.Count >= 3
            ? expeditionState.CurrentAttributes[0]
            : solution.Strength;

        var shownMental = expeditionState.CurrentAttributes.Count >= 3
            ? expeditionState.CurrentAttributes[1]
            : solution.Mental;

        var shownTactical = expeditionState.CurrentAttributes.Count >= 3
            ? expeditionState.CurrentAttributes[2]
            : solution.Tactical;

        var success =
            shownStrength >= mission.RequiredStrength &&
            shownMental >= mission.RequiredMental &&
            shownTactical >= mission.RequiredTactical;

        ImGui.TextColored(success ? SuccessColor : ErrorColor, success ? "SUCCESS" : "NO SUCCESS");

        ImGui.Spacing();

        DrawAttributeLine("STR", shownStrength, mission.RequiredStrength);
        DrawAttributeLine("MEN", shownMental, mission.RequiredMental);
        DrawAttributeLine("TAC", shownTactical, mission.RequiredTactical);

        ImGui.Spacing();

        DrawMissingValues(
            mission,
            shownStrength,
            shownMental,
            shownTactical);

        ImGui.TextUnformatted("Recommended Party");

        foreach (var member in solution.Members)
            ImGui.BulletText(member.Name);

        ImGui.Spacing();
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Squadron Roster"))
            this.DrawRoster(members);
    }

    public void Dispose()
    {
    }

    private static void DrawAttributeLine(
        string label,
        int current,
        int required)
    {
        var success = current >= required;

        ImGui.TextColored(
            success ? SuccessColor : ErrorColor,
            $"{label} {current} / {required}");
    }

    private static void DrawMissingValues(
        SquadronMission mission,
        int currentStrength,
        int currentMental,
        int currentTactical)
    {
        var missingStrength = Math.Max(0, mission.RequiredStrength - currentStrength);
        var missingMental = Math.Max(0, mission.RequiredMental - currentMental);
        var missingTactical = Math.Max(0, mission.RequiredTactical - currentTactical);

        if (missingStrength <= 0 && missingMental <= 0 && missingTactical <= 0)
            return;

        ImGui.TextUnformatted("Missing");

        if (missingStrength > 0)
            ImGui.TextColored(ErrorColor, $"STR +{missingStrength}");

        if (missingMental > 0)
            ImGui.TextColored(ErrorColor, $"MEN +{missingMental}");

        if (missingTactical > 0)
            ImGui.TextColored(ErrorColor, $"TAC +{missingTactical}");

        ImGui.Spacing();
    }

    private void DrawRoster(IReadOnlyList<SquadronMember> members)
    {
        ImGui.TextUnformatted("Roster");

        if (!ImGui.BeginTable("###SquadronRosterTable", 4))
            return;

        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("STR");
        ImGui.TableSetupColumn("MEN");
        ImGui.TableSetupColumn("TAC");
        ImGui.TableHeadersRow();

        foreach (var member in members)
        {
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(member.Name);

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(member.Strength.ToString());

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(member.Mental.ToString());

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(member.Tactical.ToString());
        }

        ImGui.EndTable();
    }
}