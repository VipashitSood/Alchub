using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.Authorization.Attributes;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.DTOs.Register;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Models.Address;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Models.Customer;
using Nop.Plugin.Api.RegisterResponses;
using Nop.Plugin.Api.Services;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Controllers
{
    public class AccountController : BaseApiController
    {
        #region Fields

        private readonly ApiSettings _apiSettings;
        private readonly ICustomerApiService _customerApiService;
        private readonly IAuthenticationService _authenticationService;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly Nop.Services.Messages.IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;

        #endregion

        #region Ctor

        public AccountController(
            IJsonFieldsSerializer jsonFieldsSerializer,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            ICustomerRegistrationService customerRegistrationService,
            IDiscountService discountService,
            CustomerSettings customerSettings,
            ICustomerApiService customerApiService,
            IGenericAttributeService genericAttributeService,
            IAuthenticationService authenticationService,
            IAclService aclService,
            ApiSettings apiSettings,
            IStoreContext storeContext,
            ICustomerService customerService,
            Nop.Services.Messages.IWorkflowMessageService workflowMessageService,
            IWorkContext workContext,
            LocalizationSettings localizationSettings,
            INewsLetterSubscriptionService newsLetterSubscriptionService)
            : base(jsonFieldsSerializer,
            aclService,
            customerService,
            storeMappingService,
            storeService,
            discountService,
            customerActivityService,
            localizationService,
            pictureService)
        {
            _customerSettings = customerSettings;
            _genericAttributeService = genericAttributeService;
            _customerRegistrationService = customerRegistrationService;
            _authenticationService = authenticationService;
            _storeContext = storeContext;
            _apiSettings = apiSettings;
            _customerApiService = customerApiService;
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
            _localizationSettings = localizationSettings;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Generate OTP
        /// Note: Currently We are not sending SMS to customer. Just passing in response.
        /// </summary>
        /// <returns></returns>
        private string GenrateOtp()
        {
            char[] charArr = "0123456789".ToCharArray();
            string strrandom = string.Empty;
            Random objran = new Random();
            for (int i = 0; i < 4; i++)
            {
                //It will not allow Repetation of Characters
                int pos = objran.Next(1, charArr.Length);
                if (!strrandom.Contains(charArr.GetValue(pos).ToString()))
                    strrandom += charArr.GetValue(pos);
                else
                    i--;
            }
            return strrandom;
        }

        private async Task<bool> SecondAdminAccountExistsAsync(Customer customer)
        {
            var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id });

            return customers.Any(c => c.Active && c.Id != customer.Id);
        }

        #endregion

        #region Methods

        #region Register - Login/Logout

        [HttpPost]
        [Route("/api/account/register", Name = "API_RegisterCustomer")]
        [ProducesResponseType(typeof(RegisterResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> Register([FromBody] RegisterModel model)
        {
            try
            {
                var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
                if (customer is null)
                    return ErrorResponse("Unauthorized!", HttpStatusCode.Unauthorized);

                if (await _customerService.IsRegisteredAsync(customer))
                    return ErrorResponse(await _localizationService.GetResourceAsync("Api.Register.Error.AlreadyRegistered"), HttpStatusCode.BadRequest);

                var store = await _storeContext.GetCurrentStoreAsync();
                customer.RegisteredInStoreId = store.Id;

                if (ModelState.IsValid)
                {
                    var customerUserName = model.Email?.Trim();
                    var customerEmail = model.Email?.Trim();

                    //var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                    //var isApproved = !_apiSettings.OtpRequiredOnRegistration; //approve if otp not required.
                    //if (!isApproved)
                    //{
                    //    //set otp not verified flag. further it will set to true in ValidateOTP API
                    //    await _genericAttributeService.SaveAttributeAsync<bool>(customer, NopCustomerApiDefaults.IsRegistrationOtpVerifiedAttribute, false);
                    //}
                    var isApproved = _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                    var registrationRequest = new CustomerRegistrationRequest(customer,
                        customerEmail,
                        _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
                        model.Password,
                        _customerSettings.DefaultPasswordFormat,
                        store.Id,
                        isApproved);
                    var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(registrationRequest);
                    if (registrationResult.Success)
                    {
                        //api customer role
                        if (!await _customerService.IsInCustomerRoleAsync(customer, Constants.Roles.API_ROLE_SYSTEM_NAME))
                        {
                            var apiRole = await _customerService.GetCustomerRoleBySystemNameAsync(Infrastructure.Constants.Roles.API_ROLE_SYSTEM_NAME);
                            if (apiRole == null)
                                throw new InvalidOperationException($"'{Constants.Roles.API_ROLE_SYSTEM_NAME}' role could not be loaded");
                            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = apiRole.Id });
                        }

                        //form fields
                        if (_customerSettings.GenderEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.GenderAttribute, model.Gender);
                        if (_customerSettings.FirstNameEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.FirstNameAttribute, model.FirstName);
                        if (_customerSettings.LastNameEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.LastNameAttribute, model.LastName);
                        if (_customerSettings.DateOfBirthEnabled)
                        {
                            var dateOfBirth = Convert.ToDateTime(model.DateOfBirth);
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.DateOfBirthAttribute, dateOfBirth);
                        }
                        if (_customerSettings.PhoneEnabled)
                            await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.PhoneAttribute, model.Phone);

                        // Genrate OTP
                        // Note: We are not going to use OTP in Aclchub. 
                        //var otp = GenrateOtp();
                        //await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerApiDefaults.OtpAttribute, otp);

                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerApiDefaults.AppVersionAttribute, model.AppVersion);
                        //await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerApiDefaults.DeviceTypeAttribute, model.DeviceType);
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerApiDefaults.LanguageIdAttribute, model.languageId);

                        //notifications
                        if (_customerSettings.NotifyNewCustomerRegistration)
                            await _workflowMessageService.SendCustomerRegisteredNotificationMessageAsync(customer,
                                _localizationSettings.DefaultAdminLanguageId);

                        //prepare response
                        var data = new RegisterResponse
                        {
                            CustomerId = customer.Id,
                            OtpRequiredOnRegistration = _apiSettings.OtpRequiredOnRegistration
                        };

                        var currentLanguage = await _workContext.GetWorkingLanguageAsync();
                        switch (_customerSettings.UserRegistrationType)
                        {
                            case UserRegistrationType.EmailValidation:
                                data.UserRegistrationTypeId = (int)UserRegistrationType.EmailValidation;
                                //email validation message
                                await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerDefaults.AccountActivationTokenAttribute, Guid.NewGuid().ToString());
                                await _workflowMessageService.SendCustomerEmailValidationMessageAsync(customer, currentLanguage.Id);

                                //result
                                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.api.Account.Register.Result.EmailValidation"), data);

                            case UserRegistrationType.AdminApproval:
                                data.UserRegistrationTypeId = (int)UserRegistrationType.AdminApproval;
                                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.api.Account.Register.Result.AdminApproval"), data);

                            case UserRegistrationType.Standard:
                                data.UserRegistrationTypeId = (int)UserRegistrationType.Standard;
                                //send customer welcome message
                                await _workflowMessageService.SendCustomerWelcomeMessageAsync(customer, currentLanguage.Id);

                                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.api.Register.successfully"), data);

                            default:
                                data.UserRegistrationTypeId = (int)UserRegistrationType.Standard;
                                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.api.Register.successfully"), data);
                        }
                    }

                    //errors
                    var errorMessage = string.Join(",", registrationResult.Errors.ToList());
                    return ErrorResponse(errorMessage, HttpStatusCode.BadRequest, registrationResult.Errors.ToList());
                }

                //model not valid errors
                var modelErrorMessage = string.Join(",", ModelState?.Values.SelectMany(x => x.Errors)?.Select(x => x.ErrorMessage)?.ToList());
                return ErrorResponse(modelErrorMessage, HttpStatusCode.BadRequest, Errors: ModelState?.Values.SelectMany(x => x.Errors)?.Select(x => x.ErrorMessage)?.ToList());
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.NotFound);
            }
        }

        [HttpPost]
        [Route("/api/account/otp", Name = "API_ValidateOtp")]
        [ProducesResponseType(typeof(OtpResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> ValidateOtp([FromBody] OtpModel model)
        {
            if (model.Userid <= 0)
                return ErrorResponse(await _localizationService.GetResourceAsync("invalid userid", 1), HttpStatusCode.BadRequest);

            //customer
            var customer = await _customerApiService.GetCustomerEntityByIdAsync(model.Userid);
            if (customer == null)
                return ErrorResponse("Customer not found", HttpStatusCode.NotFound);

            //get otp from generic attributes
            var otp = await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerApiDefaults.OtpAttribute);

            //verify otp
            if (!otp.Equals(model.OTP.ToString()))
                return ErrorResponse(await _localizationService.GetResourceAsync("account.incorrect.otp"), HttpStatusCode.Unauthorized);

            //active customer
            customer.Active = true;
            await _customerService.UpdateCustomerAsync(customer);

            //set otp verified flag
            await _genericAttributeService.SaveAttributeAsync<bool>(customer, NopCustomerApiDefaults.IsRegistrationOtpVerifiedAttribute, true);

            //activity log
            await _customerActivityService.InsertActivityAsync("CustomerActivated", $"Customer OTP {otp} has been verified & customer account has been activated", customer);

            //success
            return SuccessResponse(await _localizationService.GetResourceAsync("account.verified"), "OTP has been verified & customer account has been activated");
        }

        //[HttpPost]
        //[Route("/api/resend_otp", Name = "Resendotp")]
        //public async Task<BaseResponseModel> Resendotp(ResendOtpModel model)
        //{
        //    var currentCustomer = await _customerApiService.GetCustomerEntityByIdAsync(model.Userid);

        //    // Genrate OTP     
        //    string otp = GenrateOtp();
        //    // Save OTP
        //    await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerApiDefaults.OtpAttribute, otp);
        //    //send email
        //    await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(currentCustomer, otp,
        //        (await _workContext.GetWorkingLanguageAsync()).Id);
        //    //if (_customerSettings.OtpEnabled = false)
        //    await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerApiDefaults.OtpAttribute, otp);

        //    return SuccessResponse(await _localizationService.GetResourceAsync("nop.api.resend.otp"), otp);
        //}

        [HttpPost]
        [Route("/api/account/logout", Name = "API_Logout")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public virtual async Task<BaseResponseModel> Logout([FromBody] LogoutRequestModel logoutRequest)
        {
            if (logoutRequest.CustomerId == 0)
            {
                return ErrorResponse(await _localizationService.GetResourceAsync("Nop.Api.InvalidUserId"));
            }
            try
            {
                var customer = await _authenticationService.GetAuthenticatedCustomerAsync();
                if (customer.Id != logoutRequest.CustomerId)
                {
                    return ErrorResponse("CustomerId doesn't match", HttpStatusCode.BadRequest);
                }
                await _authenticationService.SignOutAsync();
                return SuccessResponse(await _localizationService.GetResourceAsync("Nop.Api.LogoutSuccessfully"));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.BadRequest);
            }
        }

        #endregion

        #region Password

        [HttpPost]
        [Route("/api/account/forget_password", Name = "API_ForgetPassword")]
        [ProducesResponseType(typeof(ForgetPasswordResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> ForgetPassword(ForgetPasswordModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var customer = await _customerService.GetCustomerByEmailAsync(model.Email.Trim());
                    if (customer != null && customer.Active && !customer.Deleted)
                    {
                        //save token and current date
                        var passwordRecoveryToken = Guid.NewGuid();
                        await _genericAttributeService.SaveAttributeAsync(customer, NopCustomerApiDefaults.PasswordRecoveryTokenAttribute,
                            passwordRecoveryToken.ToString());
                        DateTime? generatedDateTime = DateTime.UtcNow;
                        await _genericAttributeService.SaveAttributeAsync(customer,
                            NopCustomerApiDefaults.PasswordRecoveryTokenDateGeneratedAttribute, generatedDateTime);

                        //send email
                        await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(customer,
                            (await _workContext.GetWorkingLanguageAsync()).Id);

                        return SuccessResponse("Forget password", await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailHasBeenSent"));
                    }
                    else
                    {
                        return ErrorResponse(await _localizationService.GetResourceAsync("Account.PasswordRecovery.EmailNotFound"), HttpStatusCode.NotFound);
                    }
                }

                //model not valid errors
                var modelErrorMessage = string.Join(",", ModelState?.Values.SelectMany(x => x.Errors)?.Select(x => x.ErrorMessage)?.ToList());
                return ErrorResponse(modelErrorMessage, HttpStatusCode.BadRequest, Errors: ModelState?.Values.SelectMany(x => x.Errors)?.Select(x => x.ErrorMessage)?.ToList());
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.NotImplemented);
            }
        }

        [HttpPost]
        [Route("/api/account/reset_password", Name = "API_ResetPassword")]
        [ProducesResponseType(typeof(ResetPasswordResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> ResetPassword(ResetPasswordModel model)
        {
            try
            {
                //fetch customer
                var customer = await _customerApiService.GetCustomerEntityByIdAsync(model.Userid);


                if (customer == null)
                    return ErrorResponse(await _localizationService.GetResourceAsync("Account.PasswordRecovery.WrongToken"));

                //validate token expiration date
                if (await _customerService.IsPasswordRecoveryLinkExpiredAsync(customer))
                {
                    return ErrorResponse(await _localizationService.GetResourceAsync("Account.PasswordRecovery.LinkExpired"));
                }

                if (ModelState.IsValid)
                {
                    var response = await _customerRegistrationService
                        .ChangePasswordAsync(new ChangePasswordRequest(customer.Email, false, _customerSettings.DefaultPasswordFormat, model.NewPassword));

                    if (!response.Success)
                    {
                        return ErrorResponse(string.Join(';', response.Errors), HttpStatusCode.BadRequest);
                    }

                    var data = new ResetPasswordResponse
                    {
                        CustomerId = model.Userid
                    };
                    return SuccessResponse(await _localizationService.GetResourceAsync("Account.PasswordRecovery.PasswordHasBeenChanged"), data);
                }
                return ErrorResponse(await _localizationService.GetResourceAsync("Account.PasswordRecovery.NotFound"), HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.NotImplemented);
            }
        }

        [HttpPost]
        [Route("/api/account/change_password", Name = "API_ChangePassword")]
        [AuthorizePermission("PublicStoreAllowNavigation")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [GetRequestsErrorInterceptorActionFilter]
        public virtual async Task<BaseResponseModel> ChangePassword([FromBody] ChangePasswordModel model)
        {
            // var customer = await _workContext.GetCurrentCustomerAsync();
            var customer = await _customerApiService.GetCustomerEntityByIdAsync(model.UserId);
            if (!await _customerService.IsRegisteredAsync(customer))
                return ErrorResponse(await _localizationService.GetResourceAsync("api.User.NotFound"), HttpStatusCode.NotFound);

            if (ModelState.IsValid)
            {
                var changePasswordRequest = new ChangePasswordRequest(customer.Email,
                    true, _customerSettings.DefaultPasswordFormat, model.NewPassword, model.OldPassword);
                var changePasswordResult = await _customerRegistrationService.ChangePasswordAsync(changePasswordRequest);

                var data = new ResetPasswordResponse
                {
                    CustomerId = model.UserId
                };

                if (changePasswordResult.Success)
                {
                    return SuccessResponse(await _localizationService.GetResourceAsync("Account.ChangePassword.Success"), data);
                }

                //errors
                foreach (var error in changePasswordResult.Errors)
                    ModelState.AddModelError("", error);

                return ErrorResponse(string.Join(",", changePasswordResult.Errors), HttpStatusCode.BadRequest);
            }
            return ErrorResponse(await _localizationService.GetResourceAsync("api.Address.NotFound"), HttpStatusCode.NotFound);
        }

        [HttpDelete]
        [Route("/api/account/delete/{id}", Name = "API_DeleteCustomer")]
        [ProducesResponseType(typeof(RegisterResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> DeleteCustomer([FromRoute] int id)
        {
            try
            {
                if (id <= 0)
                    return ErrorResponse("invalid id!", HttpStatusCode.BadRequest);

                //try to get a customer with the specified id
                var customer = await _customerService.GetCustomerByIdAsync(id);
                if (customer == null)
                {
                    return ErrorResponse("Unauthorized! Customer not found", HttpStatusCode.Unauthorized);
                }

                //prevent attempts to delete the user, if it is the last active administrator
                if (await _customerService.IsAdminAsync(customer) && !await SecondAdminAccountExistsAsync(customer))
                {
                    return ErrorResponse(await _localizationService.GetResourceAsync("Admin.Customers.Customers.AdminAccountShouldExists.DeleteAdministrator"), HttpStatusCode.Unauthorized);
                }

                //ensure that the current customer cannot delete "Administrators" if he's not an admin himself
                if (await _customerService.IsAdminAsync(customer) && !await _customerService.IsAdminAsync(await _authenticationService.GetAuthenticatedCustomerAsync()))
                {
                    return ErrorResponse(await _localizationService.GetResourceAsync("Admin.Customers.Customers.OnlyAdminCanDeleteAdmin"), HttpStatusCode.Unauthorized);
                }

                await _customerService.DeleteCustomerAsync(customer);

                //remove newsletter subscription (if exists)
                foreach (var store in await _storeService.GetAllStoresAsync())
                {
                    var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                    if (subscription != null)
                    {
                        await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(subscription);
                    }
                }

                //activity log
                await _customerActivityService.InsertActivityAsync("DeleteCustomer", await _localizationService.GetResourceAsync("Api.ActivityLog.DeleteCustomer"), customer);

                return SuccessResponse("Account deleted!", await _localizationService.GetResourceAsync("Nop.api.account.customer.deleted.successfully"));
            }
            catch (Exception ex)
            {
                return ErrorResponse(ex.Message, HttpStatusCode.InternalServerError);
            }
        }

        #endregion

        #endregion
    }
}