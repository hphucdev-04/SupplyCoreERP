using System;

namespace SupplyCoreERP.Utilities
{
    public static class Utility
    {
        public static class Code
        {
            #region Code
            public static string Generate(string prefix)
            {
                var suffix = Guid.NewGuid().ToString()[..4].ToUpper();
                return $"{prefix}-{DateTime.Now:yyyyMMdd}-{suffix}";
            }
            #endregion
        }
    }
}
