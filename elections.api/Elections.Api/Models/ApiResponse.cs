namespace Elections.Api.Models;

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public object? Data { get; set; }

    public ApiResponse()
    {
        Success = true;
    }

    public static ApiResponse FromError(string error)
    {
        return new ApiResponse { Success = false, Error = error };
    }

    public static ApiResponse FromSuccess(object? data = null)
    {
        return new ApiResponse { Success = true, Data = data };
    }
}

public class ApiResponse<T> : ApiResponse
{
    public new T? Data { get; set; }
}
