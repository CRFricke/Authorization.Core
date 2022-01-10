using CRFricke.Authorization.Core.UI.Data;
using System;

namespace Authorization.Core.UI.Tests.Integration.Models
{
    public class RoleModel
    {
        public string Id { get; private set; }

        public string Name { get; set; }

        public string Description { get; set; }

        internal AuthUiRole ToRole()
        {
            return new AuthUiRole
            {
                Id = Id ?? Guid.NewGuid().ToString(),
                Name = Name,
                Description = Description
            };
        }

        internal static RoleModel CreateFrom(AuthUiRole role)
        {
            return new RoleModel
            {
                Id = role.Id,
                Description = role.Description,
                Name = role.Name
            };
        }
    }
}
