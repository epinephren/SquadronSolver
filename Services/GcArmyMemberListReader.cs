using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using SquadronSolver.Models;

namespace SquadronSolver.Services;

public sealed unsafe class GcArmyMemberListReader
{
    private const string AddonName = "GcArmyMemberList";

    private const int HeaderValueCount = 6;
    private const int MemberValueStride = 15;
    private const int MaxMembers = 8;

    private readonly IGameGui gameGui;

    public GcArmyMemberListReader(IGameGui gameGui)
    {
        this.gameGui = gameGui;
    }

    public bool TryRead(out IReadOnlyList<SquadronMember> members)
    {
        members = [];

        var addon = this.gameGui.GetAddonByName<AtkUnitBase>(AddonName);

        if (addon is null)
            return false;

        if (!addon->IsVisible || !addon->IsReady)
            return false;

        var result = new List<SquadronMember>();

        for (var i = 0; i < MaxMembers; i++)
        {
            var offset = HeaderValueCount + i * MemberValueStride;

            var name = ReadString(addon, offset);
            var strength = ReadInt(addon, offset + 7);
            var mental = ReadInt(addon, offset + 8);
            var tactical = ReadInt(addon, offset + 9);

            if (string.IsNullOrWhiteSpace(name))
                continue;

            result.Add(new SquadronMember(
                name,
                strength,
                mental,
                tactical));
        }

        members = result;
        return result.Count > 0;
    }

    private static int ReadInt(AtkUnitBase* addon, int index)
    {
        if (index < 0 || index >= addon->AtkValuesCount)
            return 0;

        var value = addon->AtkValues[index];

        return value.Type is AtkValueType.Int
            ? value.Int
            : 0;
    }

    private static string ReadString(AtkUnitBase* addon, int index)
    {
        if (index < 0 || index >= addon->AtkValuesCount)
            return string.Empty;

        var value = addon->AtkValues[index];

        return value.Type switch
        {
            AtkValueType.String => value.String.ToString(),
            AtkValueType.ConstString => value.String.ToString(),
            AtkValueType.ManagedString => value.String.ToString(),
            _ => string.Empty,
        };
    }
}