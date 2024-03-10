// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB;
using LinqToDB.Mapping;
using System;

#pragma warning disable 1573, 1591

namespace Popug.Billing.Api.Data
{
	[Table("operations_log")]
	public partial class OperationsLogEntity
	{
		[Column("operation_log_id", DataType  = DataType.Int32         , IsPrimaryKey = true         , IsIdentity = true, SkipOnInsert = true, SkipOnUpdate = true)] public int            OperationLogId { get; set; } // integer
		[Column("operation_date"  , DataType  = DataType.DateTimeOffset                                                                                           )] public DateTimeOffset OperationDate  { get; set; } // timestamp (6) with time zone
		[Column("account_id"      , DataType  = DataType.Int32                                                                                                    )] public int            AccountId      { get; set; } // integer
		[Column("billing_cycle_id", DataType  = DataType.Int32                                                                                                    )] public int            BillingCycleId { get; set; } // integer
		[Column("reason_type"     , CanBeNull = false                  , DataType     = DataType.Text                                                             )] public string         ReasonType     { get; set; } // text
		[Column("reason_id"       , CanBeNull = false                  , DataType     = DataType.Text                                                             )] public string         ReasonId       { get; set; } // text
		[Column("description"     , CanBeNull = false                  , DataType     = DataType.Text                                                             )] public string         Description    { get; set; } // text
		[Column("debt"            , DataType  = DataType.Decimal                                                                                                  )] public decimal        Debt           { get; set; } // numeric(14,4)
		[Column("credit"          , DataType  = DataType.Decimal                                                                                                  )] public decimal        Credit         { get; set; } // numeric(14,4)
	}
}
