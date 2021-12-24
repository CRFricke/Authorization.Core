using System;

namespace Authorization.Core.UI.Tests.Integration.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTimeOffset? TrimMilliseconds(this DateTimeOffset? dtOffset)
        {
            return (dtOffset.HasValue) 
                ? dtOffset.Value.AddTicks(-dtOffset.Value.Ticks % TimeSpan.FromSeconds(1).Ticks) 
                : default;
        }
    }
}
