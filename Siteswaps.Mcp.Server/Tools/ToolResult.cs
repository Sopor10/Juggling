namespace Siteswaps.Mcp.Server.Tools;

public record ToolError(string Message, string? Detail = null, string? Parameter = null);

public record ToolResult<T>(T? Data, ToolError? Error = null)
{
    public bool IsSuccess => Error is null;
}

public static class ToolResult
{
    public static ToolResult<T> Ok<T>(T data) => new(data, null);

    public static ToolResult<T> Fail<T>(
        string message,
        string? detail = null,
        string? parameter = null
    ) => new(default, new ToolError(message, detail, parameter));

    public static ToolResult<T> From<T>(Func<T> action)
    {
        try
        {
            return Ok(action());
        }
        catch (ArgumentException ex)
        {
            return Fail<T>(ex.Message, ex.InnerException?.Message, ex.ParamName);
        }
        catch (Exception ex)
        {
            return Fail<T>($"Unexpected error: {ex.Message}", ex.ToString());
        }
    }

    public static async Task<ToolResult<T>> FromAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var result = await action();
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return Fail<T>(ex.Message, ex.InnerException?.Message, ex.ParamName);
        }
        catch (Exception ex)
        {
            return Fail<T>($"Unexpected error: {ex.Message}", ex.ToString());
        }
    }
}
