public class CreateDpImageRequest
{
    public int DpProductId { get; set; }
    public string DpImageTitle { get; set; } = null!;
    public IFormFile File { get; set; } = null!;
}
