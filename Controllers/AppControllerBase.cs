using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    /// <summary>
    /// Shared base for the app (household) controllers: exposes the caller's
    /// identity from the token and the home-permission guards. Errors thrown as
    /// <see cref="ApiException"/> are translated by the exception middleware.
    /// </summary>
    public abstract class AppControllerBase : ControllerBase
    {
        #region IDENTITY

        protected int GetPersonId()
        {
            return User.GetPersonId();
        }

        #endregion

        #region PAGING

        /// <summary>Normalizes paging input: page >= 1, pageSize within [1, 100].</summary>
        protected static (int page, int pageSize) NormalizePaging(int page, int pageSize)
        {
            if (page < 1) page = 1;
            pageSize = Math.Clamp(pageSize, 1, 100);
            return (page, pageSize);
        }

        #endregion

        #region PERMISSION GUARDS

        /// <summary>Throws 403 when the caller is not a member of the home.</summary>
        protected static void RequireMembership(Member? caller)
        {
            if (caller == null)
                throw new ApiException(403, "You are not a member of this home");
        }

        /// <summary>Throws 403 when the caller is missing or its role is not allowed.</summary>
        protected static void RequireRole(Member? caller, params string[] allowedRoles)
        {
            if (caller == null || !allowedRoles.Contains(caller.role.name))
                throw new ApiException(403, "You do not have permission");
        }

        #endregion

    }
}
