namespace MokomoGamesLib.Runtime.Extensions
{
    public static class IntExtensions
    {
        public static (int x, int y) IndexToXY(this int self, int row)
        {
            var x = self % row;
            var y = self / row;
            return (x, y);
        }

        public static int XYToIndex(int x, int y, int row)
        {
            return x * row + y;
        }
    }
}