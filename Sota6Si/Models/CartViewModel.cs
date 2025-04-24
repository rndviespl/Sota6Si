using Sota6Si.Models;

namespace Sota6Si.Models
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public List<DpProduct> Products { get; set; } // Список продуктов
    }
}
