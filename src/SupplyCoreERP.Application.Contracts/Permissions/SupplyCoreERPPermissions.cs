namespace SupplyCoreERP.Permissions;

public static class SupplyCoreERPPermissions
{
	public const string GroupName = "SupplyCoreERP";

	public static class Catalog
	{
		public const string GroupNameCatalog = "Catalog";

		public static class Category
		{
			public const string Default = GroupNameCatalog + ".Category";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
		}

		public static class Medicine
		{
			public const string Default = GroupNameCatalog + ".Medicine";
			public const string Create = Default + ".Create";
			public const string Update = Default + ".Update";
			public const string Delete = Default + ".Delete";
		}


		//Add your own permission names. Example:
		//public const string MyPermission1 = GroupName + ".MyPermission1";
	}
}
