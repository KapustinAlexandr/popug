using LinqToDB;

namespace Popug.Tasks.Api.Data;

public partial class TasksDb 
{
    partial void InitDataContext()
    {
        (this as IDataContext).CloseAfterUse = true;
    }
}