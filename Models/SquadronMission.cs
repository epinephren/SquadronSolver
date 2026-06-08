namespace SquadronSolver.Models;

public sealed record SquadronMission(
    string Name,
    int Level,
    int RequiredStrength,
    int RequiredMental,
    int RequiredTactical);