using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using PayFast.Integration.Model;
using PayFast.Integration.Web;

namespace Sample
{
    /// <summary>
    /// I know this called Notify, but really, it is a validation page that is never seen by the user.
    /// Parameterized as 'notify' by PF, names as such for simplicity
    /// </summary>
    public partial class Notify : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // NOTE: May not have session state here, but it is ok with re-instantiation
            var settings = (Settings)Session["PFSettings"];
            var wrapper = (Wrapper)Session["PFWrapper"];

            wrapper.Validate(settings, this.Page, this.Context,
                () => { /* Invoked on success */ },
                () => { /* Invoked on failure Ie. insufficient funds */ },
                (someMessageFromPF) => { /* Invoked upon needing investigation */ }, 
                (ex) => { /* Log anddeal with exception accordingly */ });
        }
    }
}