namespace Popug.Tasks.Api.Common;

public static class CUDExtensions
{
    public static CUDEvent<T> CUDCreated<T>(this T source) where T : class
    {
        return new CUDEvent<T> { Data = source, Type = "created" };
    }

    public static CUDEvent<T> CUDUpdated<T>(this T source) where T : class
    {
        return new CUDEvent<T> { Data = source, Type = "updated" };
    }

    public static CUDEvent<T> CUDDeleted<T>(this T source) where T : class
    {
        return new CUDEvent<T> { Data = source, Type = "deleted" };
    }
}