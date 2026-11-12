using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace Warehouse_25DH110549.Models
{
    public class ProductMetadata
    {
        // Lớp này chứa các quy tắc kiểm tra dữ liệu (Validation)
        // Nó liên kết với class Product thông qua Partial Class
        [MetadataType(typeof(ProductMetadata))]
        public partial class Product
        {
        }

        public class ProductMetadatas
        {
            [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
            [Display(Name = "Tên sản phẩm")]
            public string ProductName { get; set; }

            [Required(ErrorMessage = "Danh mục không được để trống")]
            [Display(Name = "Danh mục")]
            public string Category { get; set; }

            // ĐÂY LÀ DÒNG QUAN TRỌNG ĐỂ CHẶN GIÁ = 0
            [Required(ErrorMessage = "Đơn giá không được để trống")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn 0")]
            [Display(Name = "Đơn giá")]
            public decimal Price { get; set; }

            [Required(ErrorMessage = "Số lượng không được để trống")]
            [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải là số nguyên không âm")]
            [Display(Name = "Số lượng")]
            public int Quantity { get; set; }
        }
    }
}