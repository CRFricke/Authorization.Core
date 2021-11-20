namespace Fricke.Authorization.Core.UI.Pages
{
    /// <summary>
    /// The severity associated with a <see cref="Notification"/>.
    /// </summary>
    public enum Severity
    {
        /// <summary>
        /// It is a normal message.
        /// </summary>
        Normal,

        /// <summary>
        /// It is a high severity message.
        /// </summary>
        High
    }

    /// <summary>
    /// A notification message to be displayed on another Razor page.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// The <see cref="Severity"/> of the messsage.
        /// </summary>
        public Severity Severity { get; set; }

        /// <summary>
        /// The message to be displayed on the other Razor page.
        /// </summary>
        public string Message { get; set; }
    }
}
