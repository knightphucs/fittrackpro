namespace FitTrackPro.Infrastructure.MachineLearning;
 
/// <summary>
/// Map label (folder name / predict key) → tên hiển thị tiếng Việt
/// Phải khớp với thư mục trong training-data/
/// </summary>
public static class FoodLabelMapper
{
    // Key = label trong model (tên thư mục ảnh training)
    // Value = tên tiếng Việt hiển thị
    private static readonly Dictionary<string, string> LabelToDisplayName = new(StringComparer.OrdinalIgnoreCase)
    {
        // Noodle / Soup
        { "pho_bo",       "Phở Bò" },
        { "pho_ga",       "Phở Gà" },
        { "bun_bo_hue",   "Bún Bò Huế" },
        { "bun_cha",      "Bún Chả" },
        { "mi_quang",     "Mì Quảng" },
        { "hu_tieu",      "Hủ Tiếu" },
        { "banh_canh",    "Bánh Canh" },
 
        // Rice dishes
        { "com_tam",      "Cơm Tấm" },
        { "com_rang",     "Cơm Chiên Dương Châu" },
        { "com_ga",       "Cơm Gà" },
 
        // Bread & Rolls
        { "banh_mi",      "Bánh Mì" },
        { "goi_cuon",     "Gỏi Cuốn" },
        { "nem_ran",      "Nem Rán (Chả Giò)" },
        { "banh_xeo",     "Bánh Xèo" },
 
        // Hotpot
        { "lau_thai",     "Lẩu Thái" },
        { "lau_de",       "Lẩu Dê" },
 
        // Snacks / Desserts
        { "banh_bao",     "Bánh Bao Nhân Thịt" },
        { "banh_flan",    "Bánh Flan" },
        { "che_ba_mau",   "Chè Ba Màu" },
 
        // Beverages
        { "ca_phe_sua_da", "Cà Phê Sữa Đá" },
        { "tra_sua",       "Trà Sữa Trân Châu" },
    };
 
    public static string GetDisplayName(string label)
        => LabelToDisplayName.TryGetValue(label, out var name) ? name : label;
 
    public static IReadOnlyDictionary<string, string> All => LabelToDisplayName;
}