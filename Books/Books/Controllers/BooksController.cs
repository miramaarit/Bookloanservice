using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Books.Models;
using Books.Data;



namespace Books.Controllers
{
        [ApiController]
        [Route("[controller]")]
        public class BooksController : ControllerBase
        {
        private readonly AppDbContext _context;

        public BooksController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public ActionResult<IEnumerable<Book>> GetAvailableBooks()
        {
            var availableBooks = _context.Books.Where(b => b.IsAvailable).ToList();
            return Ok(availableBooks);
        }
        //Get book by id
        [HttpGet("{id}")]
        public ActionResult<Book> GetBook(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }
        //Create book
        [HttpPost]
        public ActionResult<Book> CreateBook(Book book)
        {
            _context.Books.Add(book);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
        }
        //Update book
        [HttpPut("{id}")]
        public ActionResult UpdateBook(int id, Book updatedBook)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }
            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.Genre = updatedBook.Genre;
            book.IsAvailable = updatedBook.IsAvailable;
            _context.SaveChanges();
            return NoContent();
        }
        [HttpDelete("{id}")]
        public ActionResult DeleteBook(int id)
        {
            var book = _context.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }

            // Kontrollera om boken är kopplad till aktiva lån
            var relatedLoans = _context.Loans.Any(loan => loan.BookId == id);
            if (relatedLoans)
            {
                return BadRequest("The book cannot be deleted because it is associated with active loans.");
            }

            // Ta bort boken
            _context.Books.Remove(book);
            _context.SaveChanges();
            return NoContent();
        }
    }
}




