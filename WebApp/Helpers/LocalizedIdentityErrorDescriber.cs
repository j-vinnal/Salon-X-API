using Microsoft.AspNetCore.Identity;

namespace WebApp.Helpers;

/// <summary>
/// Provides localized error descriptions for identity errors.
/// </summary>
public class LocalizedIdentityErrorDescriber : IdentityErrorDescriber
{
    /// <summary>
    /// Returns the default identity error.
    /// </summary>
    /// <returns>An <see cref="IdentityError"/> object.</returns>
    public override IdentityError DefaultError()
    {
        return new IdentityError
        {
            Code = nameof(DefaultError)
            // Description = Helpers.Base.App.Resources.Identity.DefaultError
        };
    }
}