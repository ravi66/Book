namespace Book.Shared
{
    public static class Utils
    {
        public static string RandomColour() => String.Format("#{0:X6}", new Random().Next(0x1000000));

        public static string BalanceColour(decimal value, bool darkMode) => value < 0 ? "color:red;" : darkMode ? "color:white;" : "color:black;";
    }
}