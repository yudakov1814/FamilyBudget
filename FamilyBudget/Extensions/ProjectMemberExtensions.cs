using FamilyBudget.Data;
using FamilyBudget.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace FamilyBudget.Extensions
{
    public static class ProjectMemberExtensions
    {
        public static bool CanView(this IdentityUser user, ProjectMember projectMember, ApplicationDbContext context)
        {
            var project = context.Projects
                .Include(x => x.ProjectMembers)
                .FirstOrDefault(x => x.Id == projectMember.ProjectId);

            return project.OwnerId == user.Id || project.ProjectMembers.Any(x => x.UserId == user.Id);
        }

        public static bool CanEdit(this IdentityUser user, ProjectMember projectMember, ApplicationDbContext context)
        {
            return CanView(user, projectMember, context);
        }

        public static bool CanDelete(this IdentityUser user, ProjectMember projectMember, ApplicationDbContext context)
        {
            return CanView(user, projectMember, context);
        }
    }
}


