using System;
using System.IO;
using System.Linq;

namespace Kityme.Utils
{
    public static class DotEnv
    {
        public static void Load(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            foreach (var line in File.ReadAllLines(filePath))
                        {
                            var parts = line.Split(
                                '=',
                                StringSplitOptions.RemoveEmptyEntries);

                            string variable = parts[0];
                            string value = parts[1];
                            
                            if (parts.Length < 2)
                                continue;

                            if (parts.Length > 3)
                            {
                                value = line.Replace($"{variable}=", "");
                            }
            
                            Environment.SetEnvironmentVariable(variable, value);
                        } 
        }
    }
}