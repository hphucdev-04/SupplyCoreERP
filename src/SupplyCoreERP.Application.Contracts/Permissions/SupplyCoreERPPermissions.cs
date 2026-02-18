namespace SupplyCoreERP.Permissions;

public static class SupplyCoreERPPermissions
{
	public const string GroupName = "SupplyCoreERP";

	//Group Catalog
	public static class Catalog
	{
		public const string GroupNameCatalog = "Catalog";

		//Category
		public static class Category
		{
			public const string Default = GroupNameCatalog + ".Category"; 
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
		}

		//Medicine
		public static class Medicine
		{
			public const string Default = GroupNameCatalog + ".Medicine";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
			public const string Approve = Default + ".Approve"; 
		}

		//BaseUnit
		public static class BaseUnit
		{
			public const string Default = GroupNameCatalog + ".BaseUnit";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
		}

		//DosageForm
		public static class DosageForm
		{
			public const string Default = GroupNameCatalog + ".DosageForm";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
		}

		//ActiveIngredient
		public static class ActiveIngredient
		{
			public const string Default = GroupNameCatalog + ".ActiveIngredient";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
		}

		//Manufacturer
		public static class Manufacturer
		{
			public const string Default = GroupNameCatalog + ".Manufacturer";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
		}
	}
}