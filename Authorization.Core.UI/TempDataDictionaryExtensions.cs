using Fricke.Authorization.Core.UI.Pages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Fricke.Authorization.Core.UI
{
    public static class TempDataDictionaryExtensions
    {
        /// <summary>
        /// Saves the specified <see cref="Notification"/> collection in the TempData Dictionary.
        /// </summary>
        /// <param name="tempDataDictionary">The <see cref="ITempDataDictionary"/> to be updated.</param>
        /// <param name="key">The key to be used when saving the Notification collection.</param>
        /// <param name="notifications">The Notification collection to be saved.</param>
        public static void SetNotifications(this ITempDataDictionary tempDataDictionary, string key, List<Notification> notifications)
        {
            if (tempDataDictionary == null)
            {
                throw new ArgumentNullException(nameof(tempDataDictionary));
            }

            tempDataDictionary[key] = JsonSerializer.Serialize(notifications);  // JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// Retrieves the <see cref="Notification"/> collection with the specified key from the TempData Dictionary.
        /// </summary>
        /// <param name="tempDataDictionary">The <see cref="ITempDataDictionary"/> that contains the collection to be retrieved.</param>
        /// <param name="key">The key to use when retrieving the collection from the TempData Dictionary.</param>
        /// <returns>The requested <see cref="Notification"/> collection; <em>null</em>, if not found.</returns>
        public static List<Notification> GetNotifications(this ITempDataDictionary tempDataDictionary, string key)
        {
            if (tempDataDictionary == null)
            {
                throw new ArgumentNullException(nameof(tempDataDictionary));
            }

            var jsonString = (string)tempDataDictionary[key];
            if (string.IsNullOrEmpty(jsonString))
            {
                return default;
            }

            return JsonSerializer.Deserialize<List<Notification>>(jsonString);
        }
    }
}
