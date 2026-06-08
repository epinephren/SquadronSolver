namespace SquadronSolver.Models;

public sealed class MissionSolution
{
    public required IReadOnlyList<SquadronMember> Members { get; init; }

    public int Strength => this.Members.Sum(x => x.Strength);
    public int Mental => this.Members.Sum(x => x.Mental);
    public int Tactical => this.Members.Sum(x => x.Tactical);

    public int MissingStrength(SquadronMission mission) => Math.Max(0, mission.RequiredStrength - this.Strength);
    public int MissingMental(SquadronMission mission) => Math.Max(0, mission.RequiredMental - this.Mental);
    public int MissingTactical(SquadronMission mission) => Math.Max(0, mission.RequiredTactical - this.Tactical);

    public int MissingTotal(SquadronMission mission)
        => this.MissingStrength(mission)
           + this.MissingMental(mission)
           + this.MissingTactical(mission);

    public bool IsSuccess(SquadronMission mission)
        => this.MissingTotal(mission) == 0;
}