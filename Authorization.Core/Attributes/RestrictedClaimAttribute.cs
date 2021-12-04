using System;

namespace CRFricke.Authorization.Core.Attributes
{
    /// <summary>
    /// Identifies claims that are restricted for system entities.
    /// </summary>
    /// <remarks>
    /// This attribute is used to prevent system entities (e.g. the Administrator user or role) from being deleted.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class RestrictedClaimAttribute : Attribute
    {
    }
}
