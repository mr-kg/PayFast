using PayFast.Integration.Model;
using PayFast.Integration.Web;
using System;

namespace Sample
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnPayNow_Click(object sender, EventArgs e)
        {
            var wrapper = new Wrapper();
            var settings = new Settings()
            {
                IsLive = false,
                MerchantID = "x",
                MerchantKey = "x",
                URLCancel = "https://someurl.com/cancel",
                URLNotify = "https://someurl.com/notify",
                URLReturn = "https://someurl.com/return",
            };
            var trans = new Transaction()
            {
                Amount = 100M,
                Description = "Test Item 1",
                Name = "Test",
                OrderId = "1"
            };

            // Store wherever convenient
            Session["PFSettings"] = settings;
            Session["PFWrapper"] = wrapper;

            // Kick off a PF transaction
            wrapper.CreateTrans(settings, this.Page, this.Context, trans, (ex) => { /* Would probably log the exception or deal with it accordingly */ });
        }
    }
}