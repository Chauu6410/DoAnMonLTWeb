namespace DoAnMonLTWeb.Models
{
    public static class OrderStatusHelper
    {
        public const string ChoXuLy = "Cho xu ly";
        public const string DangXuLy = "Dang xu ly";
        public const string HoanThanh = "Hoan thanh";
        public const string DaHuy = "Da huy";

        public static readonly string[] All = { ChoXuLy, DangXuLy, HoanThanh, DaHuy };

        public static string Normalize(string? status)
        {
            return status switch
            {
                "Pending" => ChoXuLy,
                "Processing" => DangXuLy,
                "Completed" => HoanThanh,
                "Cancelled" => DaHuy,
                null or "" => ChoXuLy,
                _ => status
            };
        }
    }
}
