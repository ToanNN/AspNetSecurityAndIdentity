using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace MappingUserClaims;

public class CustomClaimsTransformation : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var identity = new ClaimsIdentity();
        var claimType = "newClaim";
        if (principal.HasClaim(claim => claim.Type == claimType))
        {
            return principal;
        }

        identity.AddClaim(new Claim(claimType, "myClaimValue"));

        principal.AddIdentity(identity);
        return principal;
    }
}