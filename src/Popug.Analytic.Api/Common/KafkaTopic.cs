namespace Popug.Analytic.Api.Common;

public static class KafkaTopic
{
    public static class Auth
    {
        public const string AdminEvents = "auth-admin-events";
        public const string UserStreaming = "auth-events";
    }

    public static class TaskTracker
    {
        public const string TaskStreaming = "TaskTracker.Task.Streaming";
        public const string TaskAssigned = "TaskTracker.Task.Assigned";
        public const string TaskCompleted = "TaskTracker.Task.Completed";
    }

    public static class Billing
    {
        public const string OperationLogged = "Billing.Operation.Logged";
    }
}