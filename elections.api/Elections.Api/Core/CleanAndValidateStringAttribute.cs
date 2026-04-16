using System.ComponentModel.DataAnnotations;

namespace Elections.Api.Core
{
    public class CleanAndValidateStringAttribute : ValidationAttribute
    {
        public int Min { get; set; } = -1;
        public int Max { get; set; } = -1;
        public bool IsRequired { get; set; } = false;

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            string fieldName = $"Field '{validationContext.DisplayName}'";
            string? str = value as string;

            str = str?.Clean();

            if (str.IsWEmpty())
            {
                if (IsRequired)
                    return new ValidationResult($"{fieldName} is required but is empty.");

                return ValidationResult.Success;
            }

            if (Min != -1 && Max != -1 && (str!.Length < Min || str.Length > Max))
                return new ValidationResult($"{fieldName} must be between {Min}-{Max} characters.");

            if (Min != -1 && str!.Length < Min)
                return new ValidationResult($"{fieldName} must be at least {Min} characters.");

            if (Max != -1 && str!.Length > Max)
                return new ValidationResult($"{fieldName} must be at most {Max} characters.");

            var property = validationContext.ObjectType.GetProperty(validationContext.MemberName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(validationContext.ObjectInstance, str);
            }

            return ValidationResult.Success;
        }
    }
}
