// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB;
using LinqToDB.Data;

#pragma warning disable 1573, 1591

namespace Popug.Tasks.Api.Data
{
    public partial class TasksDb : DataConnection
	{
		public TasksDb()
		{
			InitDataContext();
		}

		public TasksDb(string configuration)
			: base(configuration)
		{
			InitDataContext();
		}

		public TasksDb(DataOptions<TasksDb> options)
			: base(options.Options)
		{
			InitDataContext();
		}

		partial void InitDataContext();

		public ITable<PopugEntity> Popugs => this.GetTable<PopugEntity>();
		public ITable<TaskEntity>  Tasks  => this.GetTable<TaskEntity>();
	}
}