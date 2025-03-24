public class ReservationViewModel
{
    public int CarId { get; set; }
    public decimal PricePerDay { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
}
