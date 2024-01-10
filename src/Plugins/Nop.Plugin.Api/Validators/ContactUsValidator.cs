using Microsoft.AspNetCore.Http;
using Nop.Plugin.Api.Helpers;
using Nop.Plugin.Api.Models.Common;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Api.Validators
{
    public class ContactUsValidator : BaseDtoValidator<ContactUsModel>
    {
        #region Constructors

        public ContactUsValidator(IHttpContextAccessor httpContextAccessor, IJsonHelper jsonHelper, Dictionary<string, object> requestJsonDictionary) :
            base(httpContextAccessor, jsonHelper, requestJsonDictionary)

        {
            SetUserIdRule();
            SetNameRule();
            SetEmailRule();
            SetSubjectRule();
            SetEnquiryRule();
        }

        #endregion

        #region Private Methods

        private void SetUserIdRule()
        {
            SetNotNullOrEmptyCreateOrUpdateRule(a => a.UserId.ToString(), "user_id required", "user_id");
        }

        private void SetNameRule()
        {
            SetNotNullOrEmptyCreateOrUpdateRule(a => a.Name, "name required", "name");
        }

        private void SetEmailRule()
        {
            SetNotNullOrEmptyCreateOrUpdateRule(a => a.Email, "email required", "email");
        }

        private void SetSubjectRule()
        {
            SetNotNullOrEmptyCreateOrUpdateRule(a => a.Subject, "subject required", "subject");
        }

        private void SetEnquiryRule()
        {
            SetNotNullOrEmptyCreateOrUpdateRule(a => a.Enquiry, "enquiry required", "enquiry");
        }

        #endregion
    }
}
