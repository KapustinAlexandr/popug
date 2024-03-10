using LinqToDB;

namespace Popug.Billing.Api.Data;

public partial class BillingDb
{
    partial void InitDataContext()
    {
        (this as IDataContext).CloseAfterUse = true;
    }
}