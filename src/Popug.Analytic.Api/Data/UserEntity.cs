// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB;
using LinqToDB.Mapping;

#pragma warning disable 1573, 1591

namespace Popug.Analytic.Api.Data
{
	[Table("users")]
	public partial class UserEntity
	{
		[Column("id"       , DataType  = DataType.Int32, IsPrimaryKey = true         , IsIdentity = true, SkipOnInsert = true, SkipOnUpdate = true)] public int    Id       { get; set; } // integer
		[Column("user_id"  , CanBeNull = false         , DataType     = DataType.Text                                                             )] public string UserId   { get; set; } // text
		[Column("user_name", DataType  = DataType.Text                                                                                            )] public string UserName { get; set; } // text
	}
}