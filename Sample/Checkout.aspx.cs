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
                MerchantID = "X",
                MerchantKey = "X",
                URLCancel = "https://google.com",
                URLNotify = "https://google.com",
                URLReturn = "https://google.com"
            };
            var trans = new Transaction()
            {
                Amount = 100M,
                Description = "TestItem1",
                Name = "Test",
                OrderId = "12256",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@email.com",
                CellNumber = "0821234567"
            };

            // Store wherever convenient
            Session["PFSettings"] = settings;
            Session["PFWrapper"] = wrapper;

            // Kick off a PF transaction
            wrapper.CreateTrans(settings, this.Page, this.Context, trans, (ex) => { /* Would probably log the exception or deal with it accordingly */ });
        }
    }
}