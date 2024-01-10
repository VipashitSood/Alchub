using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Nop.Web.Alchub.SyncMasterCatalog.Authorization
{
    /// <summary>
    /// Represents a sync catalog system access filter attribute
    /// </summary>
    public sealed class AthorizeSyncCatalogAttribute : TypeFilterAttribute
    {
        #region Ctor

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public AthorizeSyncCatalogAttribute() : base(typeof(AthorizeSyncCatalogFilter))
        {
        }

        #endregion

        #region Nested filter

        /// <summary>
        /// Represents a sync catalog system access filter attribute
        /// </summary>
        private class AthorizeSyncCatalogFilter : IAsyncAuthorizationFilter
        {
            #region Constants

            private const string AUTHORIZATION_KEY = "Authorization";
            private const string BEARER_TOKEN_NAME = "Bearer";
            private const string BEARER_TOKEN_VALUE = "sync-catolog-system-alchub-cn4a9HVCjhyHkT9cAlhD";

            #endregion

            #region Utilities

            /// <summary>
            /// Called early in the filter pipeline to confirm request is authorized
            /// </summary>
            /// <param name="context">Authorization filter context</param>
            /// <returns>A task that represents the asynchronous operation</returns>
            private async Task AuthorizeRequest(AuthorizationFilterContext context)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                var token = context.HttpContext.Request.Headers[AUTHORIZATION_KEY].ToString()?.Split(" ");
                if (string.IsNullOrEmpty(token[0]) || string.IsNullOrEmpty(token[1]))
                {
                    // current user hasn't access
                    context.Result = new StatusCodeResult(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden);
                }

                if (!token[0].Equals(BEARER_TOKEN_NAME) || !token[1].Equals(BEARER_TOKEN_VALUE))
                {
                    // current user hasn't access
                    context.Result = new StatusCodeResult(Microsoft.AspNetCore.Http.StatusCodes.Status403Forbidden);
                }

                //access allowed.
                await Task.CompletedTask;
            }

            #endregion

            #region Methods

            /// <summary>
            /// Called early in the filter pipeline to confirm request is authorized
            /// </summary>
            /// <param name="context">Authorization filter context</param>
            /// <returns>A task that represents the asynchronous operation</returns>
            public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
            {
                await AuthorizeRequest(context);
            }

            #endregion
        }

        #endregion
    }
}
