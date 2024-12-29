namespace Books.Models
{
    public class Loan
    {
        public int Id { get; set; }

       public int BookId { get; set; }

        public Book Book { get; set; } // Navigering till Book

        public int UserId { get; set; }
        public User User { get; set; } // Navigering till User

        // Lånedatum och retur
        public DateTime LoanDate { get; set; }
        public DateTime? ReturnDate { get; set; } // Nullable för aktiva lån
    }
}
