using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FamilyBudget.Data;
using FamilyBudget.Models;

namespace FamilyBudget.Controllers
{
    public class FinOperationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FinOperationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FinOperations
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.FinOperations.Include(f => f.Category).Include(f => f.Project).Include(f => f.ProjectMember);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: FinOperations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finOperation = await _context.FinOperations
                .Include(f => f.Category)
                .Include(f => f.Project)
                .Include(f => f.ProjectMember)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (finOperation == null)
            {
                return NotFound();
            }

            return View(finOperation);
        }

        // GET: FinOperations/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name");
            ViewData["ProjectMemberId"] = new SelectList(_context.ProjectMembers, "Id", "NameInProject");
            return View();
        }

        // POST: FinOperations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FinType,ForAll,Value,CategoryId,ProjectMemberId,ProjectId,CreateTime,UpdateTime")] FinOperation finOperation)
        {
            if (ModelState.IsValid)
            {
                finOperation.CreateTime = DateTime.Now;
                finOperation.UpdateTime = DateTime.Now;
                _context.Add(finOperation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", finOperation.CategoryId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", finOperation.ProjectId);
            ViewData["ProjectMemberId"] = new SelectList(_context.ProjectMembers, "Id", "NameInProject", finOperation.ProjectMemberId);
            return View(finOperation);
        }

        // GET: FinOperations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finOperation = await _context.FinOperations.FindAsync(id);
            if (finOperation == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", finOperation.CategoryId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", finOperation.ProjectId);
            ViewData["ProjectMemberId"] = new SelectList(_context.ProjectMembers, "Id", "NameInProject", finOperation.ProjectMemberId);
            return View(finOperation);
        }

        // POST: FinOperations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FinType,ForAll,Value,CategoryId,ProjectMemberId,ProjectId,UpdateTime")] FinOperation finOperation)
        {
            if (id != finOperation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    finOperation.UpdateTime = DateTime.Now;
                    _context.Update(finOperation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FinOperationExists(finOperation.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", finOperation.CategoryId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", finOperation.ProjectId);
            ViewData["ProjectMemberId"] = new SelectList(_context.ProjectMembers, "Id", "NameInProject", finOperation.ProjectMemberId);
            return View(finOperation);
        }

        // GET: FinOperations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finOperation = await _context.FinOperations
                .Include(f => f.Category)
                .Include(f => f.Project)
                .Include(f => f.ProjectMember)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (finOperation == null)
            {
                return NotFound();
            }

            return View(finOperation);
        }

        // POST: FinOperations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var finOperation = await _context.FinOperations.FindAsync(id);
            _context.FinOperations.Remove(finOperation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FinOperationExists(int id)
        {
            return _context.FinOperations.Any(e => e.Id == id);
        }
    }
}
