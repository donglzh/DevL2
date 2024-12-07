namespace DevL2.WebAPI.Common;

public class Response<T>
{
    /// <summary>
    /// Code
    /// </summary>
    public int Code { get; set; }
    /// <summary>
    /// Data
    /// </summary>
    public T Data { get; set; } = default!;
    /// <summary>
    /// Message
    /// </summary>
    public string? Message { get; set; }
    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public Response(T data, string? message = null)
    {
        Code = 200;
        Data = data;
        Message = message ?? "Success";
    }
    
    public Response(int code,string message)
    {
        Code = code;
        Message = message;
    }
}