using Microsoft.AspNetCore.Http;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.Models.ProductsParameters;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Validators
{
    public class ProductEmailAFriendValidator : BaseDtoValidator<ProductEmailAFriend>
    {
        #region Constructors

        public ProductEmailAFriendValidator(IHttpContextAccessor httpContextAccessor, IJsonHelper jsonHelper, Dictionary<string, object> requestJsonDictionary) :
            base(httpContextAccessor, jsonHelper, requestJsonDictionary)

        {
            SetUserIdRule();
            SetYourEmailRule();
            SetFriendEmail();
        }

        #endregion

        #region Private Methods

        private void SetUserIdRule()
        {
            SetNotNullOrEmptyCreateOrUpdateRule(a => a.UserId.ToString(), "user_id required", "user_id");
        }
        private void SetFriendEmail()
        {
            SetNotNullOrEmptyCreateOrUpdateRule(a => a.FriendEmailAddress, "friend email is required", "friendemail");
        }
        private void SetYourEmailRule()
        {
            SetNotNullOrEmptyCreateOrUpdateRule(a => a.YourEmailAddress, "email address is required", "email");
        }

        #endregion
    }
}
