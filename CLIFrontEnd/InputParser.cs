using MMR_Tracker_V3;
using TDMUtils;

namespace CLIFrontEnd
{
    internal class InputParser
    {

        public static List<int> ParseIndicesString(string input)
        {
            List<int> Indexes = [];
            var sections = input.Split(',').Select(x => x.Trim());
            foreach (var i in sections)
            {
                if (i.IsIntegerRange(out Tuple<int, int> Range))
                {
                    for (var q = Range.Item1; q <= Range.Item2; q++) { Indexes.Add(q); }
                }
                else if (int.TryParse(i, out int I3))
                {
                    Indexes.Add(I3);
                }
            }
            return Indexes;

        }
    }
}
