namespace Authorization.Core.UI.Tests.Integration.Infrastructure
{
    public class UIPageContext : ContextBase
    {
        public UIPageContext()
        { }

        public UIPageContext(UIPageContext currentContext) : base(currentContext)
        { }


        public UIPageContext WithAuthenticatedUser() =>
            new(this) { UserAuthenticated = true };

        public UIPageContext WithAnonymousUser() =>
            new(this) { UserAuthenticated = false };

        public UIPageContext WithPasswordLogin() =>
            new(this) { PasswordLoginEnabled = true };


        public bool PasswordLoginEnabled
        {
            get => GetValue<bool>(nameof(PasswordLoginEnabled));
            set => SetValue(nameof(PasswordLoginEnabled), value);
        }

        public bool UserAuthenticated
        {
            get => GetValue<bool>(nameof(UserAuthenticated));
            set => SetValue(nameof(UserAuthenticated), value);
        }
    }
}
