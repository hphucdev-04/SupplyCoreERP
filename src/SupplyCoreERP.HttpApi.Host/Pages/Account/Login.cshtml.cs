using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using Volo.Abp.Account.Web;
using Volo.Abp.Identity;

namespace SupplyCoreERP.Pages.Account
{
    public class LoginModel : Volo.Abp.Account.Web.Pages.Account.LoginModel
    {
        public List<TenantDto> AvailableTenants { get; set; }

        public LoginModel(
            IAuthenticationSchemeProvider schemeProvider,
            IOptions<AbpAccountOptions> accountOptions,
            IOptions<IdentityOptions> identityOptions,
            IdentityDynamicClaimsPrincipalContributorCache claimsPrincipalContributorCache,
            IWebHostEnvironment webHostEnvironment)
            : base(schemeProvider, accountOptions, identityOptions,
                   claimsPrincipalContributorCache, webHostEnvironment)
        {
            AvailableTenants = new List<TenantDto>();
        }
    }

    public class TenantDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}

