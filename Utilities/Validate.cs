namespace TaskTaskerAPI.Utilities
{
    /// <summary>
    /// Small guard helpers for request data. Kept in the repositories (rather than
    /// as DataAnnotations on the DTOs) because the DTOs are reused as nested
    /// references (e.g. { id } only), where full-object validation must not run.
    /// </summary>
    public static class Validate
    {
        public static void Required(string field, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ApiException(400, $"{field} is required");
        }

        public static void Text(string field, string? value, int maxLength, int minLength = 1)
        {
            value ??= string.Empty;

            if (value.Trim().Length < minLength)
                throw new ApiException(400, minLength == 1
                    ? $"{field} is required"
                    : $"{field} must be at least {minLength} characters");

            if (value.Length > maxLength)
                throw new ApiException(400, $"{field} must be at most {maxLength} characters");
        }

        public static void Positive(string field, int value)
        {
            if (value <= 0)
                throw new ApiException(400, $"{field} must be greater than zero");
        }
    }
}
