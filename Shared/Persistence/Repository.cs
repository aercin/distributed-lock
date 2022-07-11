namespace SingleNodeDistributedLockSample.Persistence
{
    public class Repository
    {
        public Repository()
        {
            Books = new List<Book>
            {
                new Book { SeriNo= new Guid("3fa1f0fb-f5b3-4fa8-8f9d-58cf65806500"), Name ="Lion King", UnitPrice = 125.0M, IsRented=false,RentedBy =null}
            };
        }

        public List<Book> Books { get; set; }
    }

    public class Book
    {
        public Guid SeriNo { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsRented { get; set; }
        public string RentedBy { get; set; }
    }
}
