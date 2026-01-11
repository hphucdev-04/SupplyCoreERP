using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SupplyCoreERP.Categories.Dtos
{
	public class CreateUpdateCategoryDto
	{
		[Required(ErrorMessage = "Mã nhóm không được để trống")]
		[StringLength(50, ErrorMessage = "Mã nhóm tối đa 50 ký tự")]
		public string Code { get; set; }

		[Required(ErrorMessage = "Tên nhóm không được để trống")]
		[StringLength(255, ErrorMessage = "Tên nhóm tối đa 255 ký tự")]
		public string Name { get; set; }

		public string Description { get; set; }
	}
}
