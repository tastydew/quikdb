using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace QuikDB.Validation
{
    class ProperRestorePathRule : ValidationRule
    {

        private string[] InvalidRestorePaths = new string[]
        {
            @"C:\ProgramData",
            @"C:\Program Files",
            @"C:\Program Files",
            @"C:\Windows",
            @"C:\Users",
            @"C:\UserData",

        };

        Regex pattern = new Regex(@"^.*\.(bak|BAK)$");

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string selectedPath = value as string;
            Match match = pattern.Match(selectedPath);

            if (!String.IsNullOrEmpty(selectedPath))
            {
                foreach (string s in InvalidRestorePaths)
                {
                    if (selectedPath.Contains(s))
                    {
                        return new ValidationResult(false, "The selected path is not a valid path which you can restore from.");
                    }
                }

                if (!match.Success)
                {
                    return new ValidationResult(false, "You have not selected a valid backup file or file location");   
                }
            }

            return ValidationResult.ValidResult;

        }
    }
}
