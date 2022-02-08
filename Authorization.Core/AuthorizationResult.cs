using System.Collections.Generic;

namespace CRFricke.Authorization.Core
{
    /// <summary>
    /// Represents the result of an authorization request.
    /// </summary>
    public class AuthorizationResult
    {
        /// <summary>
        /// Creates a new instance of the AuthorizationResult class with default values.
        /// </summary>
        public AuthorizationResult()
        { }

        /// <summary>
        /// Flag indicating whether the request succeeded or not.
        /// </summary>
        public bool Succeeded { get; private set; }

        /// <summary>
        /// Returns an <see cref="AuthorizationFailure"/> object that describes why the user is not authorized.
        /// </summary>
        public AuthorizationFailure? Failure { get; private set; }

        /// <summary>
        /// Returns a string representing the current <see cref="AuthorizationResult"/> object.
        /// </summary>
        /// <returns>A string representing the current <see cref="AuthorizationResult"/> object.</returns>
        public override string ToString()
        {
            return Succeeded
                ? nameof(Succeeded)
                : Failure?.FailureReason ?? nameof(Failed);
        }


        /// <summary>
        /// Returns an AuthorizationResult object indicating success.
        /// </summary>
        /// <returns></returns>
        public static AuthorizationResult Success() => new() { Succeeded = true };

        /// <summary>
        /// Creates an AuthorizationResult object indicating an authorization failure, with a list of failing claims if applicable.
        /// </summary>
        /// <param name="failingClaims">An optional collection of the required claims which were not met.</param>
        /// <returns>An AuthorizationResult object describing the reason for the failure.</returns>
        public static AuthorizationResult Failed(IEnumerable<string>? failingClaims = null)
            => new() { Failure = AuthorizationFailure.NotAuthorized(failingClaims) };

        /// <summary>
        /// Creates an AuthorizationResult object indicating an attempt to elevate privileges, with a list of the offending claims.
        /// </summary>
        /// <param name="failingClaims">A collection of the requested claims that would elevate privileges.</param>
        /// <returns>An AuthorizationResult object describing the elevation error.</returns>
        public static AuthorizationResult Elevation(IEnumerable<string>? failingClaims = null)
            => new() { Failure = AuthorizationFailure.Elevation(failingClaims) };

        /// <summary>
        /// Creates an AuthorizationResult object indicating a failure to determine the current user, with a list of the required claims.
        /// </summary>
        /// <param name="failingClaims">A collection of the required claims.</param>
        /// <returns>An AuthorizationResult object describing the reason for the failure.</returns>
        public static AuthorizationResult NoUserId(IEnumerable<string>? failingClaims = null)
            => new() { Failure = AuthorizationFailure.NoUserId(failingClaims) };

        /// <summary>
        /// Creates an AuthorizationResult object indicating an invalid attempt to update a system object, with a list of the offending claims.
        /// </summary>
        /// <param name="failingClaims">A collection of the requested claims that are privileged.</param>
        /// <returns>An AuthorizationResult object describing the reason for the failure.</returns>
        public static AuthorizationResult SystemObject(IEnumerable<string>? failingClaims = null)
            => new() { Failure = AuthorizationFailure.SystemObject(failingClaims) };
    }
}
