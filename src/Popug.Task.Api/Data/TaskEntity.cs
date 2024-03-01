// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB;
using LinqToDB.Mapping;

#pragma warning disable 1573, 1591

namespace Popug.Tasks.Api.Data
{
	[Table("tasks")]
	public partial class TaskEntity
	{
		[Column("task_id"    , DataType  = DataType.Int32  , IsPrimaryKey = true         , IsIdentity = true, SkipOnInsert = true, SkipOnUpdate = true)] public int    TaskId      { get; set; } // integer
		[Column("description", CanBeNull = false           , DataType     = DataType.Text                                                             )] public string Description { get; set; } // text
		[Column("created_by" , CanBeNull = false           , DataType     = DataType.Text                                                             )] public string CreatedBy   { get; set; } // text
		[Column("assign_to"  , CanBeNull = false           , DataType     = DataType.Text                                                             )] public string AssignTo    { get; set; } // text
		[Column("is_done"    , DataType  = DataType.Boolean                                                                                           )] public bool   IsDone      { get; set; } // boolean
	}
}