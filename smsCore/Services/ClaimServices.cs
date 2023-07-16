    using global::Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Options;
    using System.Security.Claims;

    namespace smsCore.Data.Services
    {
        public class CustomClaimsPrincipalFactory :
            UserClaimsPrincipalFactory<ApplicationUser>
        {
        private readonly LoggedInUserHelper loggedInUser;
            public CustomClaimsPrincipalFactory(
                UserManager<ApplicationUser> userManager,
                IOptions<IdentityOptions> optionsAccessor,LoggedInUserHelper _helper)
                    : base(userManager, optionsAccessor)
            {
            loggedInUser = _helper;
            }

            // This method is called only when login. It means that "the drawback   
            // of calling the database with each HTTP request" never happen.  
            public async override Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
            {
                var principal = await base.CreateAsync(user);

            var UserId = user.Id;

            var helperUser = loggedInUser.GetLoggedInUserHelper(UserId);

            if (principal.Identity != null)
            {
                ((ClaimsIdentity)principal.Identity).AddClaims(
                    new[] { new Claim("BasicUserType", helperUser.BasicUserType.ToString()) });

                var roles = await UserManager.GetRolesAsync(user);
                foreach(var role in roles)
                {
                    ((ClaimsIdentity)principal.Identity).AddClaims(
                    new[] { new Claim(ClaimTypes.Role, role) });
                }
                
                ((ClaimsIdentity)principal.Identity).AddClaims(
                    new[] { new Claim("CampusIds", string.Join(",", helperUser.CampusId)) });

                ((ClaimsIdentity)principal.Identity).AddClaims(
                    new[] { new Claim("ProfilePic", helperUser.ProfilePic.ToString()) });
                ((ClaimsIdentity)principal.Identity).AddClaims(
                    new[] { new Claim("FullName", helperUser.FullName.ToString()) });
                if(!string.IsNullOrWhiteSpace(helperUser.Email))
                ((ClaimsIdentity)principal.Identity).AddClaims(
                    new[] { new Claim("Email", helperUser.Email.ToString()) });

                ((ClaimsIdentity)principal.Identity).AddClaims(
                    new[] { new Claim("primaryId", helperUser.primaryId.ToString()) });
            }
            return principal;
            }
        }
    }

