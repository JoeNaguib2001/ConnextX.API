using ConnextX.API.Data.Models.DbContext;
using ConnextX.API.Data.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ConnextX.API.Data.Models.DbContext
{
    public class ApplicationDbContext : IdentityDbContext<ApiUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options ) : base(options)
        {

        }


        public DbSet<Language> Languages { get; set; }

        public DbSet<ArchivedSubscriptions> ArchivedSubscriptions { get; set; }

        public DbSet<CurrentSubscription> CurrentSubscriptions { get; set; }

        public DbSet<Plan> Plans { get; set; }


        protected override void OnModelCreating(ModelBuilder modelbuilder)
        {
            base.OnModelCreating(modelbuilder);
            modelbuilder.ApplyConfiguration(new RoleConfiguration());
            SeedData(modelbuilder);
        }

        private void SeedData(ModelBuilder builder)
        {
            List<Plan> plans = new List<Plan>()
            { new Plan { PlanId = 1, Name = "Free Plan", Description = "Basic Free Plan", Duration = 10000, Quota = 0, Price = 0 },
              new Plan { PlanId = 2, Name = "Premium Plan", Description = "Premium Plan", Duration = 30, Quota = 100, Price = 10 },
              new Plan { PlanId = 3, Name = "Enterprise Plan", Description = "Enterprise Plan", Duration = 30, Quota = 1000, Price = 100 }
            };
            builder.Entity<Plan>().HasData(plans);

           List <ApiUser> usersList = new List<ApiUser>()
            {

             new ApiUser
            {
                Id = "b74ddd14-6340-4840-95c2-db12554843e5",
                UserName = "FCIJUNIOR00",
                Email = "mohsenyoussef233@gmail.com",
                LockoutEnabled = false,
                PhoneNumber = "01001311691",
                FirstName = "Youssef",
                LastName = "Mohsen",
            },
            new ApiUser
            {
                Id = "b74ddd14-6340-4840-95c2-db12554843e6",
                UserName = "FCIJUNIOR01",
                Email = "mohsenyoussef234@gmail.com",
                LockoutEnabled = false,
                PhoneNumber = "01200297647",
                FirstName = "Youssef",
                LastName = "Mohsen",
            }

        };

            PasswordHasher<ApiUser> passwordHasher = new PasswordHasher<ApiUser>();
            usersList[0].PasswordHash = passwordHasher.HashPassword(usersList[0], "2324488Yy####");
            usersList[1].PasswordHash = passwordHasher.HashPassword(usersList[1], "2324488Yy####");
            builder.Entity<ApiUser>().HasData(usersList);

            List<IdentityUserRole<string>> rolesList = new List<IdentityUserRole<string>>()
            {
                new IdentityUserRole<string>
                {
                    RoleId = "AdminRoleId",
                    UserId = "b74ddd14-6340-4840-95c2-db12554843e5"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "AdminRoleId",
                    UserId = "b74ddd14-6340-4840-95c2-db12554843e6"
                }
            };
            builder.Entity<IdentityUserRole<string>>().HasData(rolesList);

            builder.Entity<CurrentSubscription>().HasData(
                new CurrentSubscription { Id = 1, PlanId = plans[0].PlanId, UserId = "b74ddd14-6340-4840-95c2-db12554843e5",StartDate = DateTime.Now,EndDate = DateTime.Now.AddDays(plans[0].Duration),Quota = plans[0].Quota},
                new CurrentSubscription { Id = 2, PlanId = plans[1].PlanId, UserId = "b74ddd14-6340-4840-95c2-db12554843e6", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(plans[1].Duration), Quota = plans[1].Quota }
                );

            builder.Entity<ArchivedSubscriptions>().HasData(
               new CurrentSubscription { Id = 1, PlanId = plans[2].PlanId, UserId = "b74ddd14-6340-4840-95c2-db12554843e5", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(plans[2].Duration), Quota = plans[2].Quota },
               new CurrentSubscription { Id = 2, PlanId = plans[0].PlanId, UserId = "b74ddd14-6340-4840-95c2-db12554843e6", StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(plans[0].Duration), Quota = plans[0].Quota }
               );


            List<Language> languages = new List<Language>()
            {
                new Language()
                {
                    Id = 1,
                    Name = "Arabic"
                },
                  new Language()
                {
                    Id = 2,
                    Name = "English"
                },
                  new Language()
                {
                    Id = 3,
                    Name = "German"
                },
            };
            builder.Entity<Language>().HasData(languages);
        }

    }
}
