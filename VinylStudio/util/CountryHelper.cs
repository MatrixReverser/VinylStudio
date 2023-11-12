using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VinylStudio.util
{
    internal static class CountryHelper
    {
        public static List<string> GetCountryNames()
        {
            var countries = new List<string>();

            foreach (CultureInfo info in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                RegionInfo region = new RegionInfo(info.LCID);
                if (!countries.Contains(region.EnglishName))
                {
                    countries.Add(region.EnglishName);
                }
            }

            countries.Sort(); 

            return countries;
        }
    }
}
