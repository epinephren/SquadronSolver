using SquadronSolver.Models;

namespace SquadronSolver.Services;

public sealed class SquadronSolverService
{
    public MissionSolution? Solve(SquadronMission mission, IReadOnlyList<SquadronMember> members)
    {
        if (members.Count < 4)
            return null;

        MissionSolution? best = null;

        for (var a = 0; a < members.Count - 3; a++)
        for (var b = a + 1; b < members.Count - 2; b++)
        for (var c = b + 1; c < members.Count - 1; c++)
        for (var d = c + 1; d < members.Count; d++)
        {
            var solution = new MissionSolution
            {
                Members =
                [
                    members[a],
                    members[b],
                    members[c],
                    members[d],
                ],
            };

            if (best is null || solution.MissingTotal(mission) < best.MissingTotal(mission))
                best = solution;
        }

        return best;
    }
}