namespace FitTrackPro.Infrastructure.Persistence.Seed;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FitTrackPro.Domain.Entities;
using FitTrackPro.Domain.ValueObjects;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            // Check if already seeded
            if (await _context.Foods.AnyAsync())
            {
                _logger.LogInformation("Database already seeded");
                return;
            }

            _logger.LogInformation("Starting database seeding...");

            await SeedVietnameseFoodsAsync();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private async Task SeedVietnameseFoodsAsync()
    {
        var foods = new List<Food>
        {
            // Breakfast Items
            Food.Create(
                "Phở Bò",
                "Phở Bò",
                "Breakfast",
                500,
                "bowl",
                450,
                new MacroNutrients(25, 60, 12),
                3,
                5
            ),
            Food.Create(
                "Bánh Mì Thịt",
                "Bánh Mì Thịt",
                "Breakfast",
                200,
                "piece",
                400,
                new MacroNutrients(18, 45, 15),
                2,
                3
            ),
            Food.Create(
                "Xôi Gà",
                "Xôi Gà",
                "Breakfast",
                300,
                "plate",
                420,
                new MacroNutrients(20, 55, 12),
                2,
                2
            ),
            Food.Create(
                "Bánh Cuốn",
                "Bánh Cuốn",
                "Breakfast",
                250,
                "plate",
                320,
                new MacroNutrients(15, 50, 8),
                2,
                3
            ),
            Food.Create(
                "Hủ Tiếu",
                "Hủ Tiếu",
                "Breakfast",
                450,
                "bowl",
                400,
                new MacroNutrients(22, 58, 10),
                3,
                4
            ),

            // Lunch Items
            Food.Create(
                "Cơm Tấm Sườn",
                "Cơm Tấm Sườn",
                "Lunch",
                400,
                "plate",
                650,
                new MacroNutrients(30, 75, 20),
                3,
                5
            ),
            Food.Create(
                "Cơm Gà Xối Mỡ",
                "Cơm Gà Xối Mỡ",
                "Lunch",
                350,
                "plate",
                600,
                new MacroNutrients(35, 65, 18),
                2,
                3
            ),
            Food.Create(
                "Bún Bò Huế",
                "Bún Bò Huế",
                "Lunch",
                500,
                "bowl",
                550,
                new MacroNutrients(28, 68, 15),
                4,
                6
            ),
            Food.Create(
                "Mì Quảng",
                "Mì Quảng",
                "Lunch",
                400,
                "bowl",
                480,
                new MacroNutrients(25, 62, 12),
                3,
                4
            ),
            Food.Create(
                "Cơm Chiên Dương Châu",
                "Cơm Chiên Dương Châu",
                "Lunch",
                350,
                "plate",
                580,
                new MacroNutrients(22, 70, 16),
                2,
                4
            ),

            // Dinner Items
            Food.Create(
                "Lẩu Thái",
                "Lẩu Thái",
                "Dinner",
                600,
                "serving",
                450,
                new MacroNutrients(35, 30, 18),
                5,
                8
            ),
            Food.Create(
                "Bún Chả",
                "Bún Chả",
                "Dinner",
                400,
                "bowl",
                520,
                new MacroNutrients(28, 55, 16),
                3,
                6
            ),
            Food.Create(
                "Gỏi Cuốn",
                "Gỏi Cuốn",
                "Snack",
                100,
                "roll",
                80,
                new MacroNutrients(5, 12, 2),
                1,
                1
            ),
            Food.Create(
                "Nem Rán",
                "Nem Rán (Chả Giò)",
                "Snack",
                50,
                "piece",
                120,
                new MacroNutrients(6, 10, 7),
                1,
                1
            ),

            // Beverages
            Food.Create(
                "Cà Phê Sữa Đá",
                "Cà Phê Sữa Đá",
                "Beverage",
                250,
                "cup",
                180,
                new MacroNutrients(3, 25, 6),
                0,
                20
            ),
            Food.Create(
                "Trà Sữa Trân Châu",
                "Trà Sữa Trân Châu",
                "Beverage",
                500,
                "cup",
                350,
                new MacroNutrients(5, 60, 10),
                0,
                45
            ),
            Food.Create(
                "Nước Mía",
                "Nước Mía",
                "Beverage",
                300,
                "cup",
                150,
                new MacroNutrients(1, 38, 0),
                0,
                35
            ),
            Food.Create(
                "Sinh Tố Bơ",
                "Sinh Tố Bơ",
                "Beverage",
                400,
                "cup",
                320,
                new MacroNutrients(6, 40, 15),
                8,
                25
            ),

            // Snacks
            Food.Create(
                "Bánh Bao Nhân Thịt",
                "Bánh Bao Nhân Thịt",
                "Snack",
                100,
                "piece",
                220,
                new MacroNutrients(10, 30, 6),
                2,
                3
            ),
            Food.Create(
                "Bánh Flan",
                "Bánh Flan",
                "Snack",
                150,
                "piece",
                200,
                new MacroNutrients(6, 28, 6),
                0,
                22
            ),

            // Rice & Noodles (Base)
            Food.Create(
                "Cơm Trắng",
                "Cơm Trắng",
                "Base",
                200,
                "bowl",
                260,
                new MacroNutrients(5, 58, 0.5m),
                1,
                0
            ),
            Food.Create(
                "Bánh Phở",
                "Bánh Phở",
                "Base",
                200,
                "serving",
                220,
                new MacroNutrients(4, 48, 0.5m),
                1,
                0
            ),
            Food.Create(
                "Bún",
                "Bún",
                "Base",
                200,
                "bowl",
                200,
                new MacroNutrients(4, 44, 0.5m),
                1,
                0
            ),

            // Proteins
            Food.Create(
                "Thịt Heo Nướng",
                "Thịt Heo Nướng",
                "Protein",
                100,
                "gram",
                250,
                new MacroNutrients(26, 0, 18),
                0,
                0
            ),
            Food.Create(
                "Gà Luộc",
                "Gà Luộc",
                "Protein",
                100,
                "gram",
                165,
                new MacroNutrients(31, 0, 3.6m),
                0,
                0
            ),
            Food.Create(
                "Tôm Luộc",
                "Tôm Luộc",
                "Protein",
                100,
                "gram",
                99,
                new MacroNutrients(24, 0.2m, 0.3m),
                0,
                0
            ),
            Food.Create(
                "Cá Kho Tộ",
                "Cá Kho Tộ",
                "Protein",
                150,
                "piece",
                280,
                new MacroNutrients(28, 8, 14),
                0,
                5
            ),

            // Vegetables
            Food.Create(
                "Rau Muống Xào",
                "Rau Muống Xào",
                "Vegetable",
                150,
                "plate",
                45,
                new MacroNutrients(3, 7, 0.5m),
                3,
                1
            ),
            Food.Create(
                "Canh Chua",
                "Canh Chua",
                "Soup",
                300,
                "bowl",
                120,
                new MacroNutrients(12, 15, 3),
                2,
                8
            ),
            Food.Create(
                "Dưa Chua",
                "Dưa Chua",
                "Side",
                50,
                "serving",
                15,
                new MacroNutrients(1, 3, 0.1m),
                1,
                2
            ),

            // Desserts
            Food.Create(
                "Chè Ba Màu",
                "Chè Ba Màu",
                "Dessert",
                250,
                "bowl",
                280,
                new MacroNutrients(4, 55, 5),
                3,
                30
            ),
            Food.Create(
                "Chè Khúc Bạch",
                "Chè Khúc Bạch",
                "Dessert",
                200,
                "bowl",
                250,
                new MacroNutrients(5, 45, 6),
                2,
                35
            )
        };

        await _context.Foods.AddRangeAsync(foods);
        _logger.LogInformation("Seeded {Count} Vietnamese food items", foods.Count);
    }
}