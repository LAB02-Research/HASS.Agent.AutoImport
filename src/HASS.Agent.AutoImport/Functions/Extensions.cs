using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HASS.Agent.AutoImport.Functions
{
    public static class Extensions
    {
        /// <summary>
        /// Replaces all double backslashes into singles
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string RemoveBackslashEscaping(this string text) => text.Replace(@"\\", @"\");

        /// <summary>
        /// Replaces all single backslashes into doubles
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string AddBackslashEscaping(this string text) => text.Replace(@"\", @"\\");
    }
}
