using System.Collections.Generic;
using System.Linq;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Describes the failure of an authorization request.
    /// </summary>
    public class AuthorizationFailure
    {
        /// <summary>
        /// Defines the reason codes that can be returned as a <see cref="FailureReason"/>.
        /// </summary>
        public class Reason
        {
            /// <summary>
            /// The user is not authorized (is missing one or more required claims). 
            /// </summary>
            public static readonly string NotAuthorized = "NotAuthorized";

            /// <summary>
            /// The request would elevate the user's privileges.
            /// </summary>
            public static readonly string Elevation = "Elevation";

            /// <summary>
            /// The current user could not be determined.
            /// </summary>
            public static readonly string NoUserId = "NoUserId";

            /// <summary>
            /// The user is attempting to perform a restricted operation on a system object.
            /// </summary>
            public static readonly string SystemObject = "SystemObject";
        }

        /// <summary>
        /// Creates a new instance of the AuthorizationFailure class with default values.
        /// </summary>
        public AuthorizationFailure()
        { }

        /// <summary>
        /// A code that describes the reason for the failure.
        /// </summary>
        public string? FailureReason { get; private set; }

        /// <summary>
        /// The missing claims which caused authorization to fail.
        /// </summary>
        public string[]? FailingClaims { get; private set; }

        /// <summary>
        /// Returns a string representing the current <see cref="AuthorizationFailure"/> object.
        /// </summary>
        /// <returns>A string representing the current <see cref="AuthorizationFailure"/> object.</returns>
        public override string ToString()
        {
            return FailureReason ?? nameof(AuthorizationResult.Failed);
        }

        /// <summary>
        /// Returns a new <see cref="AuthorizationFailure"/> object with a failure reason of "NotAuthorized".
        /// </summary>
        /// <param name="failingClaims">The claims that prevented authorization.</param>
        /// <returns>A new <see cref="AuthorizationFailure"/> object with a failure reason of "NotAuthorized".</returns>
        public static AuthorizationFailure NotAuthorized(IEnumerable<string>? failingClaims = null)
            => new() { FailureReason = Reason.NotAuthorized, FailingClaims = failingClaims?.ToArray() };

        /// <summary>
        /// Returns a new <see cref="AuthorizationFailure"/> object with a failure reason of "Elevation".
        /// </summary>
        /// <param name="failingClaims">The claims that prevented authorization.</param>
        /// <returns>A new <see cref="AuthorizationFailure"/> object with a failure reason of "Elevation".</returns>
        public static AuthorizationFailure Elevation(IEnumerable<string>? failingClaims = null)
            => new() { FailureReason = Reason.Elevation, FailingClaims = failingClaims?.ToArray() };

        /// <summary>
        /// Returns a new <see cref="AuthorizationFailure"/> object with a failure reason of "NoUserId".
        /// </summary>
        /// <param name="failingClaims">The claims that prevented authorization.</param>
        /// <returns>A new <see cref="AuthorizationFailure"/> object with a failure reason of "NoUserId".</returns>
        public static AuthorizationFailure NoUserId(IEnumerable<string>? failingClaims = null)
            => new() { FailureReason = Reason.NoUserId, FailingClaims = failingClaims?.ToArray() };

        /// <summary>
        /// Returns a new <see cref="AuthorizationFailure"/> object with a failure reason of "SystemObject".
        /// </summary>
        /// <param name="failingClaims">The claims that prevented authorization.</param>
        /// <returns>A new <see cref="AuthorizationFailure"/> object with a failure reason of "SystemObject".</returns>
        public static AuthorizationFailure SystemObject(IEnumerable<string>? failingClaims = null)
            => new() { FailureReason = Reason.SystemObject, FailingClaims = failingClaims?.ToArray() };
    }
}
