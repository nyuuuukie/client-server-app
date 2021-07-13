using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace ObserverService
{
    static class Validator
    {
        static System.Text.RegularExpressions.Regex rg;
        public static bool isPasswordCorrect(string password)
        {
            if (String.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            rg = new System.Text.RegularExpressions.Regex(@"[A-Za-z0-9._-]{2,24}");
            if (!rg.IsMatch(password))
            {
                return false;
            }
            return true;
        }

        public static bool isLoginCorrect(string login)
        {
            if (String.IsNullOrWhiteSpace(login))
            {
                return false;
            }
            if (Char.IsDigit(login[0]))
            {
                return false;
            }
            rg = new System.Text.RegularExpressions.Regex(@"[A-Za-z0-9_]{4,24}");
            if (!rg.IsMatch(login))
            {
                return false;
            }
            return true;
        }
    }
}
