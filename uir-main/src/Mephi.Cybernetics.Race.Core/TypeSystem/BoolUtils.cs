namespace Mephi.Cybernetics.Race.Core.TypeSystem
{
    public static class BoolUtils
    {
        public static bool Implies(this bool a, bool b)
        {
            return !a || b;
        }
    }
}