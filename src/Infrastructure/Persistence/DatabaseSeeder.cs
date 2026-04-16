using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedAdminAsync();
        await SeedCategoriesAsync();
    }

    private async Task SeedAdminAsync()
    {
        var email = _configuration["AdminSeed:Email"];
        var password = _configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("AdminSeed config is missing. Skipping admin seed.");
            return;
        }

        var adminExists = await _context.Users.AnyAsync(u => u.Role == UserRole.ADMIN);
        if (adminExists)
            return;

        var admin = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(password),
            FullName = "Administrator",
            Role = UserRole.ADMIN,
        };

        _context.Users.Add(admin);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin account seeded: {Email}", email);
    }

    private async Task SeedCategoriesAsync()
    {
        if (await _context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new Category
            {
                Name = "Điện tử - Công nghệ",
                Slug = "dien-tu-cong-nghe",
                Children = new List<Category>
                {
                    new Category { Name = "Điện thoại di động", Slug = "dien-thoai-di-dong" },
                    new Category { Name = "Laptop - Máy tính", Slug = "laptop-may-tinh" },
                    new Category { Name = "Máy tính bảng", Slug = "may-tinh-bang" },
                    new Category { Name = "Phụ kiện điện tử", Slug = "phu-kien-dien-tu" },
                }
            },
            new Category
            {
                Name = "Thời trang",
                Slug = "thoi-trang",
                Children = new List<Category>
                {
                    new Category { Name = "Thời trang nam", Slug = "thoi-trang-nam" },
                    new Category { Name = "Thời trang nữ", Slug = "thoi-trang-nu" },
                    new Category { Name = "Giày dép", Slug = "giay-dep" },
                    new Category { Name = "Túi xách - Phụ kiện", Slug = "tui-xach-phu-kien" },
                }
            },
            new Category
            {
                Name = "Nhà cửa - Đời sống",
                Slug = "nha-cua-doi-song",
                Children = new List<Category>
                {
                    new Category { Name = "Nội thất", Slug = "noi-that" },
                    new Category { Name = "Đồ dùng nhà bếp", Slug = "do-dung-nha-bep" },
                    new Category { Name = "Trang trí nhà cửa", Slug = "trang-tri-nha-cua" },
                }
            },
            new Category
            {
                Name = "Sức khỏe - Làm đẹp",
                Slug = "suc-khoe-lam-dep",
                Children = new List<Category>
                {
                    new Category { Name = "Mỹ phẩm", Slug = "my-pham" },
                    new Category { Name = "Chăm sóc sức khỏe", Slug = "cham-soc-suc-khoe" },
                    new Category { Name = "Thực phẩm chức năng", Slug = "thuc-pham-chuc-nang" },
                }
            },
            new Category
            {
                Name = "Thể thao - Dã ngoại",
                Slug = "the-thao-da-ngoai",
                Children = new List<Category>
                {
                    new Category { Name = "Dụng cụ thể thao", Slug = "dung-cu-the-thao" },
                    new Category { Name = "Quần áo thể thao", Slug = "quan-ao-the-thao" },
                }
            },
            new Category
            {
                Name = "Sách - Văn phòng phẩm",
                Slug = "sach-van-phong-pham",
                Children = new List<Category>
                {
                    new Category { Name = "Sách", Slug = "sach" },
                    new Category { Name = "Văn phòng phẩm", Slug = "van-phong-pham" },
                }
            },
        };

        _context.Categories.AddRange(categories);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Categories seeded.");
    }
}
