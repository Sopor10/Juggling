namespace Siteswaps.Mcp.Server.Tools;

public record ToolError(string Message, string? Detail = null, string? Parameter = null);

public record ToolResult<T>(T? Data, ToolError? Error = null)
{
    public bool IsSuccess => Error is null;

    public static ToolResult<T> Ok(T data) => new(data, null);

    public static ToolResult<T> Fail(
        string message,
        string? detail = null,
        string? parameter = null
    ) => new(default, new ToolError(message, detail, parameter));
}

public static class ToolResult
{
    public static ToolResult<T> From<T>(Func<T> action)
    {
        try
        {
            return ToolResult<T>.Ok(action());
        }
        catch (ArgumentException ex)
        {
            return ToolResult<T>.Fail(ex.Message, ex.InnerException?.Message, ex.ParamName);
        }
        catch (Exception ex)
        {
            return ToolResult<T>.Fail($"Unexpected error: {ex.Message}", ex.ToString());
        }
    }

    public static async Task<ToolResult<T>> FromAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var result = await action();
            return ToolResult<T>.Ok(result);
        }
        catch (ArgumentException ex)
        {
            return ToolResult<T>.Fail(ex.Message, ex.InnerException?.Message, ex.ParamName);
        }
        catch (Exception ex)
        {
            return ToolResult<T>.Fail($"Unexpected error: {ex.Message}", ex.ToString());
        }
    }
}
