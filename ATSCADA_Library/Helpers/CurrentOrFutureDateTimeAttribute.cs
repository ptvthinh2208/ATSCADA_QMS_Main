using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATSCADA_Library.Helpers
{
    public class CurrentOrFutureDateTimeAttribute : ValidationAttribute
    {
        public CurrentOrFutureDateTimeAttribute(string errorMessage = "The value must be today or in the future.")
        {
            ErrorMessage = errorMessage;
        }
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            // Check if the value is DateTime
            if (value is DateTime dateValue)
            {
                // Validate that the date is today or in the future
                if (dateValue < DateTime.Today)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            // Check if the value is TimeSpan
            else if (value is TimeSpan timeValue)
            {
                // Validate that the time is today or in the future (compare with the current time of the day)
                if (timeValue < DateTime.Now.TimeOfDay)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success!;
        }
    }
}
