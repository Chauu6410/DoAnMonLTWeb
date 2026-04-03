using System;
using System.ComponentModel.DataAnnotations;

namespace DoAnMonLTWeb.Models
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao.")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá.")]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
