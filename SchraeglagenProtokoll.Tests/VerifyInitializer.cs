using System.Runtime.CompilerServices;
using Argon;

namespace SchraeglagenProtokoll.Tests;

public static class VerifyInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifierSettings.AddExtraSettings(settings =>
        {
            settings.DefaultValueHandling = DefaultValueHandling.Include; // Otherwise first values from enums get lost
            settings.Converters.Add(new StringEnumConverter()); // Serialize enums as string
        });
    }
}
