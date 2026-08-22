using System.Security.Claims;

namespace TaskTaskerAPI.Utilities
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Reads the authenticated person's id from the JWT (the "sub" claim,
        /// mapped by default to NameIdentifier). Throws 401 if it is missing.
        /// </summary>
        public static int GetPersonId(this ClaimsPrincipal user)
        {
            string? value =
                user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;

            if (value == null || !int.TryParse(value, out int id))
                throw new Exception("401;Invalid or missing token");

            return id;
        }
    }
}
