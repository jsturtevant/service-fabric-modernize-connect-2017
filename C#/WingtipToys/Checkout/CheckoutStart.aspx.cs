using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WingtipToys.Checkout
{
  public partial class CheckoutStart : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      RegisterAsyncTask(new PageAsyncTask(CheckoutAsync));
    }

      private async Task CheckoutAsync()
      {
          NVPAPICaller payPalCaller = new NVPAPICaller();

          if (Session["payment_amt"] != null)
          {
              string amt = Session["payment_amt"].ToString();

              var result = await payPalCaller.ShortcutExpressCheckout(amt);
              if (result.result)
              {
                  Session["token"] = result.token;
                  Response.Redirect(result.msg);
              }
              else
              {
                  Response.Redirect("CheckoutError.aspx?" + result.msg);
              }
          }
          else
          {
              Response.Redirect("CheckoutError.aspx?ErrorCode=AmtMissing");
          }
        }
  }
}