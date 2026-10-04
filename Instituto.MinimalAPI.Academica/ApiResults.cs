namespace Instituto.MinimalAPI.Academica;

// Traduce el resultado de la BR al formato { isSuccess, message, data } que consume el front.
public static class ApiResults
{
    public static IResult Ok<T>(T data, string message = "Operación exitosa")
        => Results.Ok(new { isSuccess = true, message, data });

    public static IResult Created<T>(T data, string message = "Operación exitosa")
        => Results.Json(new { isSuccess = true, message, data }, statusCode: StatusCodes.Status201Created);

    public static IResult Fail(string message, int statusCode = StatusCodes.Status400BadRequest)
        => Results.Json(new { isSuccess = false, message }, statusCode: statusCode);
}
