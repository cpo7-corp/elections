namespace Elections.Api.Core;

public class ConfigSvc(IConfiguration configuration)
{
    public T Get<T>(string key)
    {
        var val = configuration.GetSection(key).Get<T>();
        if (val == null && typeof(T) == typeof(string[]))
        {
            return (T)(object)Array.Empty<string>();
        }
        return val ?? default!;
    }
}
