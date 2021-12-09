using FamilyBudget.Data;
using FamilyBudget.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FamilyBudget.Extensions
{
    public static class CategoryExtensions
    {
        public static bool CanView(this IdentityUser user, Category category, ApplicationDbContext context)
        {
            var project = context.Projects
                .Include(x => x.ProjectMembers)
                .FirstOrDefault(x => x.Id == category.ProjectId);

            return project.OwnerId == user.Id || project.ProjectMembers.Any(x => x.UserId == user.Id);
        }

        public static bool CanEdit(this IdentityUser user, Category category, ApplicationDbContext context)
        {
            return CanView(user, category, context);
        }

        public static bool CanDelete(this IdentityUser user, Category category, ApplicationDbContext context)
        {
            return CanView(user, category, context);
        }
    }
}

