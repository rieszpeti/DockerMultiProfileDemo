using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationTest
{
    public static class DotEnv
    {
        public static Dictionary<string, string> Load(string filePath)
        {
            if (!File.Exists(filePath))
            { 
                throw new FileNotFoundException(filePath);
            }

            var dict = new Dictionary<string, string>();

            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(
                    '=',
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                {
                    continue;
                }

                if (!parts[0].StartsWith("DOMAIN_"))
                {
                    continue;
                }

                dict.Add(parts[0], parts[1]);
            }

            return dict;
        }
    }
}
