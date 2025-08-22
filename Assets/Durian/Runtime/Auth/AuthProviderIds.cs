namespace Aloha.Durian
{
    public enum AuthProvider
    {
        Google,
        Apple
    }
    
    public static class AuthProviderIds
    {
        public static string GetId(this AuthProvider authProvider)
        {
            return authProvider switch
            {
                AuthProvider.Google => "google.com",
                AuthProvider.Apple => "apple.com",
                _ => null
            };
        }
    }
}