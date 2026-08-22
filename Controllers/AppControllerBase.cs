using Microsoft.AspNetCore.Mvc;
using TaskTaskerAPI.DAL.Entities;
using TaskTaskerAPI.Utilities;

namespace TaskTaskerAPI.Controllers
{
    /// <summary>
    /// Shared base for the app (household) controllers: exposes the caller's
    /// identity from the token, home-permission guards, and the common error
    /// translation used across the API.
    /// </summary>
    public abstract class AppControllerBase : ControllerBase
    {
        #region IDENTITY

        protected int GetPersonId()
        {
            return User.GetPersonId();
        }

        #endregion

        #region PERMISSION GUARDS

        /// <summary>Throws 403 when the caller is not a member of the home.</summary>
        protected static void RequireMembership(Member? caller)
        {
            if (caller == null)
                throw new Exception("403;You are not a member of this home");
        }

        /// <summary>Throws 403 when the caller is missing or its role is not allowed.</summary>
        protected static void RequireRole(Member? caller, params string[] allowedRoles)
        {
            if (caller == null || !allowedRoles.Contains(caller.role.name))
                throw new Exception("403;You do not have permission");
        }

        #endregion

        #region ERROR HANDLING

        protected IActionResult CatchReturn(Exception ex)
        {
            string[] error = ex.Message.Split(';');

            if (error.Length == 1)
            {
                return BadRequest(new ApiResponse<string>
                {
                    StatusCode = 400,
                    Message = ex.Message,
                    Data = String.Empty
                });
            }

            return StatusCode(Convert.ToInt32(error[0]), new ApiResponse<string>
            {
                StatusCode = Convert.ToInt32(error[0]),
                Message = error[1],
                Data = String.Empty
            });
        }

        #endregion

    }
}
