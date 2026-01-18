using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.Categories.Dtos
{
	public class CreateUpdateCategoryDto
	{
		[Required(ErrorMessage = "Tên danh mục là bắt buộc")]
		[MaxLength(100)]
		public string Name { get; set; }
	}
}
