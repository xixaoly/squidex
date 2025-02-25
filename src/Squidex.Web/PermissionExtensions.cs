﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.Security;
using Squidex.Shared.Identity;

namespace Squidex.Web
{
    public static class PermissionExtensions
    {
        private sealed class PermissionFeature
        {
            public PermissionSet Permissions { get; }

            public PermissionFeature(PermissionSet permissions)
            {
                Permissions = permissions;
            }
        }

        public static PermissionSet Permissions(this HttpContext httpContext)
        {
            var feature = httpContext.Features.Get<PermissionFeature>();

            if (feature == null)
            {
                feature = new PermissionFeature(httpContext.User.Permissions());

                httpContext.Features.Set(feature);
            }

            return feature.Permissions;
        }

        public static bool HasPermission(this HttpContext httpContext, Permission permission, PermissionSet permissions = null)
        {
            return httpContext.Permissions().Includes(permission) || permissions?.Includes(permission) == true;
        }

        public static bool HasPermission(this HttpContext httpContext, string id, string app = "*", string schema = "*", PermissionSet permissions = null)
        {
            return httpContext.HasPermission(Shared.Permissions.ForApp(id, app, schema), permissions);
        }

        public static bool HasPermission(this ApiController controller, Permission permission, PermissionSet permissions = null)
        {
            return controller.HttpContext.HasPermission(permission, permissions);
        }

        public static bool HasPermission(this ApiController controller, string id, string app = "*", string schema = "*", PermissionSet permissions = null)
        {
            if (app == "*")
            {
                if (controller.RouteData.Values.TryGetValue("app", out var value) && value is string s)
                {
                    app = s;
                }
            }

            if (schema == "*")
            {
                if (controller.RouteData.Values.TryGetValue("name", out var value) && value is string s)
                {
                    schema = s;
                }
            }

            return controller.HasPermission(Shared.Permissions.ForApp(id, app, schema), permissions);
        }
    }
}
