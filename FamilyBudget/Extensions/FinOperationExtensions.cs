using FamilyBudget.Data;
using FamilyBudget.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FamilyBudget.Extensions
{
    public static class FinOperationExtensions
    {
        public static bool CanView(this IdentityUser user, FinOperation finOperation, ApplicationDbContext context)
        {
            var project = context.Projects
                .Include(x => x.ProjectMembers)
                .FirstOrDefault(x => x.Id == finOperation.ProjectId);

            return project.OwnerId == user.Id || project.ProjectMembers.Any(x => x.UserId == user.Id);
        }

        public static bool CanEdit(this IdentityUser user, FinOperation fin_operation, ApplicationDbContext context)
        {
            return CanView(user, fin_operation, context);
        }

        public static bool CanDelete(this IdentityUser user, FinOperation fin_operation, ApplicationDbContext context)
        {
            return CanView(user, fin_operation, context);
        }
    }
}

