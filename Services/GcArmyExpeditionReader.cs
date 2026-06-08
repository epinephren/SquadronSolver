using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using SquadronSolver.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SquadronSolver.Services;

public sealed unsafe class GcArmyExpeditionReader
{
    private const string AddonName = "GcArmyExpedition";
    private const int FirstMissionValueIndex = 10;
    private const int MissionValueStride = 4;

    private readonly IGameGui gameGui;

    public GcArmyExpeditionReader(IGameGui gameGui)
    {
        this.gameGui = gameGui;
    }

    public bool TryRead(out GcArmyExpeditionState state)
    {
        state = default;

        var addon = this.gameGui.GetAddonByName<AddonGcArmyExpedition>(AddonName);

        if (addon is null)
            return false;

        if (!addon->AtkUnitBase.IsVisible || !addon->AtkUnitBase.IsReady)
            return false;

        var missionList = addon->MissionList;

        if (missionList is null)
            return false;

        var selectedIndex = missionList->SelectedItemIndex;
        var valueIndex = FirstMissionValueIndex + selectedIndex * MissionValueStride;

        var missionName = ReadString(addon, valueIndex + 1);
        var missionLevelText = ReadString(addon, valueIndex + 2);
        var missionLevel = ParseFirstInt(missionLevelText);

        var required = ReadAttributeValues(addon->RequiredAttributesComponentNode);
        var current = ReadAttributeValues(addon->CurrentAttributesComponentNode);

        SquadronMission? mission = null;

        if (required.Count >= 3)
        {
            mission = new SquadronMission(
                string.IsNullOrWhiteSpace(missionName) ? "Current Squadron Mission" : missionName,
                missionLevel,
                required[0],
                required[1],
                required[2]);
        }

        state = new GcArmyExpeditionState(
            selectedIndex,
            missionList->FirstVisibleItemIndex,
            missionList->ListLength,
            missionList->VisibleRowCount,
            missionName,
            missionLevelText,
            required,
            current,
            mission);

        return true;
    }

    private static List<int> ReadAttributeValues(AtkComponentBase* component)
    {
        var values = new List<int>();

        if (component is null)
            return values;

        ReadTextValuesFromUldManager(&component->UldManager, values, 0);

        return values
            .Where(value => value > 0)
            .Take(3)
            .Reverse()
            .ToList();
    }

    private static void ReadTextValuesFromUldManager(AtkUldManager* uldManager, List<int> values, int depth)
    {
        if (uldManager is null || depth > 4)
            return;

        var count = uldManager->NodeListCount;

        for (var i = 0; i < count; i++)
        {
            var node = uldManager->NodeList[i];

            if (node is null)
                continue;

            ReadTextValuesFromNode(node, values, depth + 1);
        }
    }

    private static void ReadTextValuesFromNode(AtkResNode* node, List<int> values, int depth)
    {
        if (node is null || depth > 4)
            return;

        if (node->Type == NodeType.Text)
        {
            var textNode = (AtkTextNode*)node;
            var text = textNode->NodeText.ToString();

            if (TryParseNumber(text, out var value))
                values.Add(value);
        }

        if (node->Type >= (NodeType)1000)
        {
            var componentNode = (AtkComponentNode*)node;

            if (componentNode->Component is not null)
                ReadTextValuesFromUldManager(&componentNode->Component->UldManager, values, depth + 1);
        }

        var child = node->ChildNode;

        while (child is not null)
        {
            ReadTextValuesFromNode(child, values, depth + 1);
            child = child->PrevSiblingNode;
        }
    }

    private static bool TryParseNumber(string text, out int value)
    {
        value = 0;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var match = Regex.Match(text, @"\d[\d,\.]*");

        if (!match.Success)
            return false;

        var normalized = match.Value
            .Replace(",", string.Empty)
            .Replace(".", string.Empty);

        return int.TryParse(
            normalized,
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static int ParseFirstInt(string text)
    {
        return TryParseNumber(text, out var value) ? value : 0;
    }

    private static string ReadString(AddonGcArmyExpedition* addon, int index)
    {
        if (index < 0 || index >= addon->AtkUnitBase.AtkValuesCount)
            return string.Empty;

        var value = addon->AtkUnitBase.AtkValues[index];

        return value.Type switch
        {
            AtkValueType.String => value.String.ToString(),
            AtkValueType.ConstString => value.String.ToString(),
            AtkValueType.ManagedString => value.String.ToString(),
            _ => string.Empty,
        };
    }
}

public readonly record struct GcArmyExpeditionState(
    int SelectedMissionIndex,
    int FirstVisibleMissionIndex,
    int MissionCount,
    int VisibleRowCount,
    string MissionName,
    string MissionLevelText,
    IReadOnlyList<int> RequiredAttributes,
    IReadOnlyList<int> CurrentAttributes,
    SquadronMission? Mission);