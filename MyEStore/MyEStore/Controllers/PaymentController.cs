using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyEStore.Models;

namespace MyEStore.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly PaypalClient _paypalClient;
        public PaymentController(PaypalClient paypalClient)
        {
            _paypalClient = paypalClient;
        }

        #region Payment/Index
        public IActionResult Index()
        {
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(CartItems);
        }
        #endregion

        public static string CART_KEY = "CART";

        // cái này là copy từ bên CartController, rảnh thì cho nó thành lớp hay gì để dễ reuse
        public List<CartItem> CartItems
        {
            get
            {
                // Trong tài liệu là đoạn comment dưới:
                //var carts = HttpContext.Session.Get<List<CartItem>>("CART");
                //if(carts == null)
                //{
                //    carts = new List<CartItem>();
                //}
                //return carts;

                // Còn cái này là thầy làm cho gọn - hơi khó hiểu...
                return HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
            }

            // Set là làm thêm (ngoài tài liệu)
            set
            {
                HttpContext.Session.Set(CART_KEY, value);
            }
        }

        #region Payment/PaypalDemo
        public IActionResult PaypalDemo()
        {
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(CartItems);
        }
        #endregion

        #region Payment/PaypalOrder
        [HttpPost]
        public async Task<IActionResult> PaypalOrder(CancellationToken cancellationToken)
        {
            // Tạo đơn hàng (thông tin lấy từ Session???) ???????????????????????????????????????????????? SessionExtensions hử ta?
            var tongTien = CartItems.Sum(p => p.ThanhTien).ToString();
            var donViTienTe = "USD";

            //OrderId là mã tham chiếu duy nhất ???????????????????????????????????????????????????????????
            var orderIdref = "DH" + DateTime.Now.Ticks.ToString();

            try
            {
                // a.Create paypal order
                var response = await _paypalClient.CreateOder(tongTien, donViTienTe, orderIdref);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new
                {
                    ex.GetBaseException().Message
                };
                return BadRequest(error);
            }
        }
        #endregion Payment/PaypalOrder

        #region Payment/PaypalCapture
        public async Task<IActionResult> PaypalCapture(string orderId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderId);
                var reference = response.purchase_units[0].reference_id;

                // Put the logic to save the transaction here ???????????????????
                // Can use the "reference" variable as a transaction key ....?

                // Lưu đơn hàng vô DB

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new
                {
                    ex.GetBaseException().Message
                };
                return BadRequest(error);
            }

        }
        #endregion Payment/PaypalCapture

        public IActionResult Success()
        {
            return View();
        }

    }
}
