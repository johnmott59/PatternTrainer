using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using SchwabLib;
using System.Collections.Generic;
using SchwabLib.Models;
using System.Collections.Generic;

namespace CandlePatternML
{
    public partial class Program
    {
        public string GetAuthKey()
        {
            string accessToken;
            int age = GetAge();
            if (age > 20)
            {
                accessToken = oAPIWrapper.GetAccessToken();
                SetKey(accessToken);
            }
            else
            {
                accessToken = GetKey();
            }

            return accessToken;
        }

        public int GetAge()
        {
            try
            {
                string filePath = @"c:\work\access.txt";
                if (!File.Exists(filePath))
                    return 100;

                string content = File.ReadAllText(filePath);
                string[] parts = content.Split('=');
                if (parts.Length != 2)
                    return 100;

                // Remove quotes and trim
                string timestampStr = parts[1].Trim().Trim('"');
                if (string.IsNullOrEmpty(timestampStr))
                    return 100;

                DateTime timestamp = DateTime.Parse(timestampStr);
                return (int)(DateTime.Now - timestamp).TotalMinutes;
            }
            catch
            {
                return 100;
            }
        }

        public void SetKey(string value)
        {
            try
            {
                string filePath = @"c:\work\access.txt";
                string content = $"\"{value}\"=\"{DateTime.Now}\"";
                File.WriteAllText(filePath, content);
            }
            catch
            {
                // Handle any file access errors silently
            }
        }

        public string GetKey()
        {
            try
            {
                string filePath = @"c:\work\access.txt";
                if (!File.Exists(filePath))
                    return string.Empty;

                string content = File.ReadAllText(filePath);
                string[] parts = content.Split('=');
                if (parts.Length != 2)
                    return string.Empty;

                // Remove quotes and trim
                return parts[0].Trim().Trim('"');
            }
            catch
            {
                return string.Empty;
            }
        }


    }
}

