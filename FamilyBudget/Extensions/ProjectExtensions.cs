using FamilyBudget.Data;
using FamilyBudget.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FamilyBudget.Extensions
{
    public static class ProjectExtensions
    {
        public static bool CanView(this IdentityUser user, Project project, ApplicationDbContext context)
        {
            return project.OwnerId == user.Id || context.ProjectMembers
                .Where(p => p.ProjectId == project.Id).Any(x => x.UserId == user.Id);
        }

        public static bool CanEdit(this IdentityUser user, Project project, ApplicationDbContext context)
        {
            return CanView(user, project, context);
        }

        public static bool CanDelete(this IdentityUser user, Project project, ApplicationDbContext context)
        {
            return CanView(user, project, context);
        }
    }
}


