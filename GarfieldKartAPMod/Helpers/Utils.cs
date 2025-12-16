using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarfieldKartAPMod.Helpers
{
    internal class Utils
    {
        // Source - https://stackoverflow.com/a
        // Posted by grenade, modified by community. See post 'Timeline' for change history
        // Retrieved 2025-12-16, License - CC BY-SA 4.0

        private static Random rng = new();

        public static void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
    }
}
