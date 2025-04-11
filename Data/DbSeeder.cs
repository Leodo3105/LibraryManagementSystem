using System;
using System.Collections.Generic;
using System.Linq;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LibraryManagementSystem.Data
{
    public static class DbSeeder
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var passwordService = scope.ServiceProvider.GetRequiredService<IPasswordService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                context.Database.Migrate();

                // Seed data only if DB is empty
                if (!context.Users.Any())
                {
                    logger.LogInformation("Starting database seeding...");

                    // 1. Seed Users
                    SeedUsers(context, passwordService);

                    // 2. Seed Categories
                    SeedCategories(context);

                    // 3. Seed Books
                    SeedBooks(context);

                    logger.LogInformation("Database seeding completed.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }

        private static void SeedUsers(ApplicationDbContext context, IPasswordService passwordService)
        {
            var users = new List<User>
            {
                new User
                {
                    Username = "admin",
                    Email = "admin@library.com",
                    PasswordHash = passwordService.HashPassword("Admin@123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Username = "user1",
                    Email = "user1@example.com",
                    PasswordHash = passwordService.HashPassword("User@123"),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();
        }

        private static void SeedCategories(ApplicationDbContext context)
        {
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Khoa học viễn tưởng",
                    Description = "Sách về các chủ đề khoa học viễn tưởng, tương lai và công nghệ",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Tiểu thuyết",
                    Description = "Các tác phẩm văn học tiểu thuyết",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Kỹ năng sống",
                    Description = "Sách về phát triển bản thân và kỹ năng sống",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Công nghệ thông tin",
                    Description = "Sách về lập trình, phát triển phần mềm và công nghệ thông tin",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Name = "Lịch sử",
                    Description = "Sách về các sự kiện lịch sử, nhân vật và văn minh",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();
        }

        private static void SeedBooks(ApplicationDbContext context)
        {
            var scifiCategoryId = context.Categories.FirstOrDefault(c => c.Name == "Khoa học viễn tưởng")?.Id;
            var novelCategoryId = context.Categories.FirstOrDefault(c => c.Name == "Tiểu thuyết")?.Id;
            var selfHelpCategoryId = context.Categories.FirstOrDefault(c => c.Name == "Kỹ năng sống")?.Id;
            var techCategoryId = context.Categories.FirstOrDefault(c => c.Name == "Công nghệ thông tin")?.Id;
            var historyCategoryId = context.Categories.FirstOrDefault(c => c.Name == "Lịch sử")?.Id;

            var books = new List<Book>
            {
                // Science fiction books
                new Book
                {
                    Title = "Dune",
                    Author = "Frank Herbert",
                    ISBN = "9780441172719",
                    PublicationYear = 1965,
                    Publisher = "Chilton Books",
                    TotalCopies = 5,
                    AvailableCopies = 5,
                    CategoryId = scifiCategoryId.Value,
                    CoverImageUrl = "/uploads/covers/dune.jpg",
                    Description = "Dune là một tiểu thuyết khoa học viễn tưởng của Frank Herbert, xuất bản lần đầu vào năm 1965. Nó đã thắng giải Nebula và giải Hugo, và được coi là một trong những tác phẩm khoa học viễn tưởng hay nhất mọi thời đại.",
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "Neuromancer",
                    Author = "William Gibson",
                    ISBN = "9780441569595",
                    PublicationYear = 1984,
                    Publisher = "Ace",
                    TotalCopies = 3,
                    AvailableCopies = 3,
                    CategoryId = scifiCategoryId.Value,
                    CoverImageUrl = "/uploads/covers/neuromancer.jpg",
                    Description = "Neuromancer là tiểu thuyết đầu tay của tác giả William Gibson, xuất bản năm 1984. Cuốn sách đã giành được ba giải thưởng lớn của thể loại khoa học viễn tưởng: giải Nebula, giải Philip K. Dick và giải Hugo.",
                    CreatedAt = DateTime.UtcNow
                },
                
                // Novels
                new Book
                {
                    Title = "Trăm năm cô đơn",
                    Author = "Gabriel García Márquez",
                    ISBN = "9786048894313",
                    PublicationYear = 1967,
                    Publisher = "Nhà xuất bản Văn học",
                    TotalCopies = 4,
                    AvailableCopies = 4,
                    CategoryId = novelCategoryId.Value,
                    CoverImageUrl = "/uploads/covers/tram-nam-co-don.jpg",
                    Description = "Trăm năm cô đơn là tiểu thuyết được xuất bản vào năm 1967 của nhà văn Gabriel García Márquez, người Colombia, đoạt giải Nobel Văn học năm 1982.",
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "Đắc nhân tâm",
                    Author = "Dale Carnegie",
                    ISBN = "9786045552834",
                    PublicationYear = 1936,
                    Publisher = "Nhà xuất bản Tổng hợp TP.HCM",
                    TotalCopies = 7,
                    AvailableCopies = 7,
                    CategoryId = selfHelpCategoryId.Value,
                    CoverImageUrl = "/uploads/covers/dac-nhan-tam.jpg",
                    Description = "Đắc nhân tâm là tên gọi trong tiếng Việt của quyển sách được xuất bản năm 1936, tên tiếng Anh là How to Win Friends and Influence People của Dale Carnegie.",
                    CreatedAt = DateTime.UtcNow
                },
                
                // IT books
                new Book
                {
                    Title = "Clean Code: A Handbook of Agile Software Craftsmanship",
                    Author = "Robert C. Martin",
                    ISBN = "9780132350884",
                    PublicationYear = 2008,
                    Publisher = "Prentice Hall",
                    TotalCopies = 3,
                    AvailableCopies = 3,
                    CategoryId = techCategoryId.Value,
                    CoverImageUrl = "/uploads/covers/clean-code.jpg",
                    Description = "Clean Code là một cuốn sách về lập trình của Robert Cecil Martin, xuất bản vào năm 2008. Sách này nêu ra các nguyên tắc, mô hình và cách thực hành viết mã nguồn sạch.",
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "Design Patterns: Elements of Reusable Object-Oriented Software",
                    Author = "Erich Gamma, Richard Helm, Ralph Johnson, John Vlissides",
                    ISBN = "9780201633610",
                    PublicationYear = 1994,
                    Publisher = "Addison-Wesley Professional",
                    TotalCopies = 2,
                    AvailableCopies = 2,
                    CategoryId = techCategoryId.Value,
                    CoverImageUrl = "/uploads/covers/design-patterns.jpg",
                    Description = "Design Patterns: Elements of Reusable Object-Oriented Software là một cuốn sách về kỹ thuật phần mềm mô tả mẫu thiết kế phần mềm.",
                    CreatedAt = DateTime.UtcNow
                },
                
                // History books
                new Book
                {
                    Title = "Sapiens: Lược sử loài người",
                    Author = "Yuval Noah Harari",
                    ISBN = "9786048932542",
                    PublicationYear = 2011,
                    Publisher = "Nhà xuất bản Thế giới",
                    TotalCopies = 5,
                    AvailableCopies = 5,
                    CategoryId = historyCategoryId.Value,
                    CoverImageUrl = "/uploads/covers/sapiens.jpg",
                    Description = "Sapiens: Lược sử loài người là tác phẩm nổi tiếng của Yuval Noah Harari, xuất bản lần đầu bằng tiếng Hebrew ở Israel năm 2011, và bằng tiếng Anh năm 2014.",
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Books.AddRange(books);
            context.SaveChanges();
        }
    }
}