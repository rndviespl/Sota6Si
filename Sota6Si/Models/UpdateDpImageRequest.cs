public class UpdateDpImageRequest
{
    public int DpProductId { get; set; }
    public string DpImageTitle { get; set; } = null!;
    public IFormFile? File { get; set; }
}