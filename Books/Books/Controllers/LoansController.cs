using Books.Data;
using Books.Dtos;
using Books.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Books.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoansController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LoansController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public ActionResult<Loan> CreateLoan(CreateLoanDto dto)
        {
            var book = _context.Books.Find(dto.BookId);
            if (book == null || !book.IsAvailable)
            {
                return BadRequest("The book is not available for loan.");
            }

            var user = _context.Users.Find(dto.UserId);
            if (user == null)
            {
                return BadRequest("User not found.");
            }

            var loan = new Loan
            {
                BookId = dto.BookId,
                UserId = dto.UserId,
                LoanDate = DateTime.UtcNow,
                ReturnDate = null
            };

            _context.Loans.Add(loan);
            book.IsAvailable = false; // Uppdatera status
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetLoansForBook), new { bookId = dto.BookId }, loan);
        }

        [HttpPut("{id}/return")]
        public ActionResult ReturnLoan(int id)
        {
            var loan = _context.Loans.Include(l => l.Book).FirstOrDefault(l => l.Id == id);
            if (loan == null || loan.ReturnDate != null)
            {
                return BadRequest("Loan not found or already returned.");
            }

            // Uppdatera lånet som avslutat
            loan.ReturnDate = DateTime.UtcNow;

            // Markera boken som tillgänglig igen
            loan.Book.IsAvailable = true;
            _context.SaveChanges();

            return NoContent();
        }
        [HttpGet("user/{userId}/active")]
        public ActionResult<IEnumerable<object>> GetActiveLoansForUser(int userId)
        {
            var loans = _context.Loans
                .Include(l => l.Book)
                .Where(l => l.UserId == userId && l.ReturnDate == null)
                .Select(l => new
                {
                    l.Id,
                    l.LoanDate,
                    Book = new { l.Book.Id, l.Book.Title, l.Book.Author }
                })
                .ToList();

            if (!loans.Any())
            {
                return NotFound("No active loans found for this user.");
            }

            return Ok(loans);
        }
        [HttpGet("book/{bookId}")]
        public ActionResult<IEnumerable<Loan>> GetLoansForBook(int bookId)
        {
            var loans = _context.Loans
                .Include(l => l.User)
                .Where(l => l.BookId == bookId)
                .ToList();

            if (!loans.Any())
            {
                return NotFound("No loans found for this book.");
            }

            return Ok(loans);
        }
    }
}
