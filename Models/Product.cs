using System.ComponentModel.DataAnnotations;

namespace DoAnMonLTWeb.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string? Image { get; set; }

        public string? Description { get; set; }

        public bool IsSale { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "So luong ton khong duoc am.")]
        public int Stock { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }
    }
}
