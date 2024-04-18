namespace LocalLeaderboard.Utils
{
    public class ScoreUtil
    {
        internal static int CalculateV2MaxScore(int noteCount)
        {
            int effectiveNoteCount = 0;
            int multiplier;
            for (multiplier = 1; multiplier < 8; multiplier *= 2)
            {
                if (noteCount < multiplier * 2)
                {
                    effectiveNoteCount += multiplier * noteCount;
                    noteCount = 0;
                    break;
                }
                effectiveNoteCount += multiplier * multiplier * 2 + multiplier;
                noteCount -= multiplier * 2;
            }
            effectiveNoteCount += noteCount * multiplier;
            return effectiveNoteCount * 115;
        }
    }
}