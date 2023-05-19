using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace AnySoftDesktop.Utils.Validation;

public class DigitValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        var regex = new Regex(@"^[0-9]+$");
        return regex.IsMatch(value as string ?? string.Empty)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, "Wrong numerical values entered.");
    }
}