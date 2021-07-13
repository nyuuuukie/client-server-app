using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ObserverService
{
    class Tracker
    {
        private static Point last;
        public static bool checkIfShifted(Point current)
        {
            if (Math.Abs(current.X - last.X) >= 10 ||
                Math.Abs(current.Y - last.Y) >= 10)
            {
                updateLastPoint(current);
                return true;
            }
            return false;
        }

        public static void updateLastPoint(Point current)
        {
            last.X = current.X;
            last.Y = current.Y;
        }
    }
}
