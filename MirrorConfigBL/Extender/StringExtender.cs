using System;

namespace MirrorConfigBL.Extender
{
    public static class StringExtender
    {

        public static bool IsNullOrEmpty(this string String)
        {
            if (String == null)
                return true;

            if (String == String.Empty)
                return true;

            return false;
        }


        public static bool IsNotNullOrEmpty(this string String)
        {
            return !String.IsNullOrEmpty();
        }

    }
}
