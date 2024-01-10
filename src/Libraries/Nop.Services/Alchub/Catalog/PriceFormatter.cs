using System;
using System.Globalization;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Tax;
using Nop.Services.Directory;
using Nop.Services.Localization;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Price formatter
    /// </summary>
    public partial class PriceFormatter : IPriceFormatter
    {
        #region Utilities

        /// <summary>
        /// Gets currency string
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="showCurrency">A value indicating whether to show a currency</param>
        /// <param name="targetCurrency">Target currency</param>
        /// <returns>Currency string without exchange rate</returns>
        protected virtual string GetCurrencyString(decimal amount,
            bool showCurrency, Currency targetCurrency)
        {
            if (targetCurrency == null)
                throw new ArgumentNullException(nameof(targetCurrency));

            string result;
            if (!string.IsNullOrEmpty(targetCurrency.CustomFormatting))
                //custom formatting specified by a store owner
                result = amount.ToString(targetCurrency.CustomFormatting);
            else
            {
                if (!string.IsNullOrEmpty(targetCurrency.DisplayLocale))
                {
                    //default behavior
                    result = amount.ToString("C", new CultureInfo(targetCurrency.DisplayLocale));
                    //++Alchub - handle nagative value showing rounds
                    if (amount < 0)
                    {
                        amount = amount * -1;
                        result = amount.ToString("C", new CultureInfo(targetCurrency.DisplayLocale));
                        result = $"-{result}"; // add manual minus here.kds
                    }
                    //--Alchub
                }
                else
                {
                    //not possible because "DisplayLocale" should be always specified
                    //but anyway let's just handle this behavior
                    result = $"{amount:N} ({targetCurrency.CurrencyCode})";
                    return result;
                }
            }

            //display currency code?
            if (showCurrency && _currencySettings.DisplayCurrencyLabel)
                result = $"{result} ({targetCurrency.CurrencyCode})";
            return result;
        }

        #endregion
    }
}