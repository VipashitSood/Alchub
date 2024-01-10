using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Plugin.Api.Configuration;
using Nop.Plugin.Api.Domain;
using Nop.Plugin.Api.Infrastructure;
using Nop.Plugin.Api.Models.Authentication;
using Nop.Plugin.Api.Models.BaseModels;
using Nop.Plugin.Api.Services;
using Nop.Services.Authentication;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Api.Controllers
{
    public class TokenController : Controller
    {
        private readonly ApiSettings _apiSettings;
        private readonly ICustomerApiService _customerApiService;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;

        public TokenController(
            ICustomerService customerService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerActivityService customerActivityService,
            IShoppingCartService shoppingCartService,
            IAuthenticationService authenticationService,
            CustomerSettings customerSettings,
            ApiSettings apiSettings,
            ICustomerApiService customerApiService,
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService)
        {
            _customerService = customerService;
            _customerRegistrationService = customerRegistrationService;
            _customerActivityService = customerActivityService;
            _shoppingCartService = shoppingCartService;
            _authenticationService = authenticationService;
            _customerSettings = customerSettings;
            _apiSettings = apiSettings;
            _customerApiService = customerApiService;
            _localizationService = localizationService;
            _genericAttributeService = genericAttributeService;
        }

        //Note: Token create is Login api
        [HttpPost]
        [AllowAnonymous]
        [Route("/token", Name = "RequestToken")]
        [ProducesResponseType(typeof(TokenResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Forbidden)]
        public async Task<BaseResponseModel> Create([FromBody] TokenRequest model)
        {
            Customer oldCustomer = await _authenticationService.GetAuthenticatedCustomerAsync();
            Customer newCustomer;

            if (model.Guest)
            {
                newCustomer = await _customerService.InsertGuestCustomerAsync();

                if (!await _customerService.IsInCustomerRoleAsync(newCustomer, Constants.Roles.API_ROLE_SYSTEM_NAME))
                {
                    //add to 'ApiUserRole' role if not yet present
                    var apiRole = await _customerService.GetCustomerRoleBySystemNameAsync(Constants.Roles.API_ROLE_SYSTEM_NAME);
                    if (apiRole == null)
                        throw new InvalidOperationException($"'{Constants.Roles.API_ROLE_SYSTEM_NAME}' role could not be loaded");
                    await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping { CustomerId = newCustomer.Id, CustomerRoleId = apiRole.Id });
                }
            }
            else
            {
                if (string.IsNullOrEmpty(model.Username))
                    return ErrorResponse("Missing username", HttpStatusCode.BadRequest);

                if (string.IsNullOrEmpty(model.Password))
                    return ErrorResponse("Missing password", HttpStatusCode.BadRequest);

                //validate customer
                var loginResult = await _customerRegistrationService.ValidateCustomerAsync(model.Username, model.Password);
                switch (loginResult)
                {
                    case CustomerLoginResults.Successful:
                        {
                            newCustomer = await (_customerSettings.UsernamesEnabled
                                      ? _customerService.GetCustomerByUsernameAsync(model.Username)
                                      : _customerService.GetCustomerByEmailAsync(model.Username));
                            break;
                        }
                    case CustomerLoginResults.CustomerNotExist:
                        return ErrorResponse(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));
                    case CustomerLoginResults.Deleted:
                        return ErrorResponse(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted"));
                    case CustomerLoginResults.NotActive:
                        return ErrorResponse(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive"));
                    case CustomerLoginResults.NotRegistered:
                        return ErrorResponse(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered"));
                    case CustomerLoginResults.LockedOut:
                        return ErrorResponse(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut"));
                    case CustomerLoginResults.WrongPassword:
                    default:
                        return ErrorResponse(await _localizationService.GetResourceAsync("Account.Login.WrongCredentials"));
                }

                //confirm not null
                if (newCustomer is null)
                    return ErrorResponse("Wrong username or password", HttpStatusCode.Forbidden);
            }

            // migrate shopping cart, if the user is different
            if (oldCustomer is not null && oldCustomer.Id != newCustomer.Id)
            {
                await _shoppingCartService.MigrateShoppingCartAsync(oldCustomer, newCustomer, true); // migrate shopping cart items to newly logged in customer
            }

            //token response
            var tokenResponse = await GenerateToken(newCustomer);

            await _authenticationService.SignInAsync(newCustomer, model.RememberMe); // update cookie-based authentication - not needed for api, avoids automatic generation of guest customer with each request to api

            // activity log
            await _customerActivityService.InsertActivityAsync(newCustomer, "Api.TokenRequest", "API token request", newCustomer);

            //registeration otp not verified error
            var message = "Token";
            if (!model.Guest && !tokenResponse.IsRegistrationOtpVerified)
                message = await _localizationService.GetResourceAsync("API.Account.Login.WrongCredentials.RegistrationOtp.NotVerified");

            //send success even if registeration otp not verified error exist.
            return SuccessResponse(message, tokenResponse);
        }

        [HttpGet]
        [Authorize(Policy = JwtBearerDefaults.AuthenticationScheme, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // this validates token
        [Route("/token/check", Name = "ValidateToken")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public async Task<BaseResponseModel> ValidateToken()
        {
            Customer currentCustomer = await _authenticationService.GetAuthenticatedCustomerAsync(); // this gets customer entity from db if it exists
            if (currentCustomer is null)
                return ErrorResponse("Invalid! Customer not found", HttpStatusCode.NotFound);

            return SuccessResponse("Valid token", "Authorized");
        }

        #region Private methods

        /// <summary>
        /// Return response for error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="stausCode"></param>
        /// <returns></returns>
        [NonAction]
        private BaseResponseModel ErrorResponse(string message = "", HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            var response = new BaseResponseModel
            {
                StatusCode = (int)statusCode,
                Message = message,
            };

            //response.Errors.Add(message);
            return response;
        }

        /// <summary>
        /// Return response for success
        /// </summary>
        /// <param name="message"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [NonAction]
        private BaseResponseModel SuccessResponse(string message = "", object data = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new BaseResponseModel
            {
                StatusCode = (int)statusCode,
                Message = message,
                Data = data == null ? new List<object>() : data
            };
        }

        //private async Task<Customer> LoginAsync(string username, string password, bool rememberMe)
        //{
        //    var result = await _customerRegistrationService.ValidateCustomerAsync(username, password);

        //    if (result == CustomerLoginResults.Successful)
        //    {
        //        var customer = await (_customerSettings.UsernamesEnabled
        //                           ? _customerService.GetCustomerByUsernameAsync(username)
        //                           : _customerService.GetCustomerByEmailAsync(username));
        //        return customer;
        //    }

        //    return null;
        //}

        private int GetTokenExpiryInDays()
        {
            return _apiSettings.TokenExpiryInDays <= 0
                       ? Constants.Configurations.DEFAULT_ACCESS_TOKEN_EXPIRATION_IN_DAYS
                       : _apiSettings.TokenExpiryInDays;
        }

        private async Task<TokenResponse> GenerateToken(Customer customer)
        {
            var currentTime = DateTimeOffset.Now;
            var expirationTime = currentTime.AddDays(GetTokenExpiryInDays());

            var claims = new List<Claim>
                         {
                             new Claim(JwtRegisteredClaimNames.Nbf, currentTime.ToUnixTimeSeconds().ToString()),
                             new Claim(JwtRegisteredClaimNames.Exp, expirationTime.ToUnixTimeSeconds().ToString()),
                             new Claim("CustomerId", customer.Id.ToString()),
                             new Claim(ClaimTypes.NameIdentifier, customer.CustomerGuid.ToString()),
                         };

            if (!string.IsNullOrEmpty(customer.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, customer.Email));
            }

            if (_customerSettings.UsernamesEnabled)
            {
                if (!string.IsNullOrEmpty(customer.Username))
                {
                    claims.Add(new Claim(ClaimTypes.Name, customer.Username));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(customer.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Name, customer.Email));
                }
            }
            var apiConfiguration = Singleton<AppSettings>.Instance.Get<ApiConfiguration>();
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(apiConfiguration.SecurityKey)), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(new JwtHeader(signingCredentials), new JwtPayload(claims));
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            //get otp verifed flag value
            var isRegistrationOtpVerified = true;
            if (_apiSettings.OtpRequiredOnRegistration)
                isRegistrationOtpVerified = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerApiDefaults.IsRegistrationOtpVerifiedAttribute);

            return await Task.FromResult(new TokenResponse(accessToken, currentTime.UtcDateTime, expirationTime.UtcDateTime)
            {
                CustomerId = customer.Id,
                CustomerGuid = customer.CustomerGuid,
                Username = _customerSettings.UsernamesEnabled ? customer.Username : customer.Email,
                TokenType = "Bearer",
                IsRegistrationOtpVerified = isRegistrationOtpVerified //Note: This flag is usefull for registered customer only.
            });
        }

        #endregion
    }
}
