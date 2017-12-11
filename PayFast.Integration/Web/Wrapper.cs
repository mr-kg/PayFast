using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PayFast.Integration.Model;
using System.Web.UI;
using System.Web;
using System.Collections.Specialized;
using System.Net;

namespace PayFast.Integration.Web
{
    /// <summary>
    /// Code adapted from: PayFast website
    /// 
    /// This is a wrapper for the PayFast payment gateway. The PayFast process is as follows:
    /// <para>1 - A page in your site calls CreateTrans(...)</para>
    /// <para>2 - User redirected to PayFast to perform payment</para>
    /// <para>3 - PayFast then POSTs to your PFMeta.URLNotify to validate and perform actions based on the outcome</para>
    /// <para>4 - The PFMeta.URLNotify page must run the Validate(...) method in order to properly validate the transaction</para>
    /// <para>4 - PayFast will then send the user back to PFMeta.URLReturn</para>
    /// </summary>
    public class Wrapper
    {
        string orderId = "";
        string processorOrderId = "";
        string strPostedVariables = "";

        List<string> _validSites;

        public Wrapper()
        {
            _validSites = new List<string>() { "www.payfast.co.za", "sandbox.payfast.co.za", "w1w.payfast.co.za", "w2w.payfast.co.za" };
        }

        /// <summary>
        /// Create the actual transaction to send to PF.
        /// Usually triggered by a button or action ie.. "Pay Now"
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="page"></param>
        /// <param name="ctx"></param>
        /// <param name="trans"></param>
        /// <param name="exceptionAction"></param>
        public void CreateTrans(Settings settings, Page page, HttpContext ctx, Transaction trans, Action<Exception> exceptionAction)
        {
            try
            {
                if (!page.IsValid) return;

                string site = settings.IsLive ? "https://www.payfast.co.za/eng/process?" : "https://sandbox.payfast.co.za/eng/process?";
                string merchant_id = settings.IsLive ? settings.MerchantID : "10001056";
                string merchant_key = settings.IsLive ? settings.MerchantKey : "28buhfgf99inw";

                // Build the query string for payment site

                StringBuilder str = new StringBuilder();
                str.AppendFormat("merchant_id={0}", HttpUtility.UrlEncode(merchant_id));
                str.AppendFormat("&merchant_key={0}", HttpUtility.UrlEncode(merchant_key));
                str.AppendFormat("&return_url={0}", HttpUtility.UrlEncode(settings.URLReturn)); // Just thank the user and tell them you are processing their order (should already be done or take a few more seconds with ITN)
                str.AppendFormat("&cancel_url={0}", HttpUtility.UrlEncode(settings.URLCancel)); // Just thank the user and tell them that they cancelled the order (encourage them to email you if they have problems paying
                str.AppendFormat("&notify_url={0}", HttpUtility.UrlEncode(settings.URLNotify)); // Called once by the payment processor to validate

                str.AppendFormat("&m_payment_id={0}", HttpUtility.UrlEncode(trans.OrderId));
                str.AppendFormat("&amount={0}", HttpUtility.UrlEncode(trans.Amount.ToString()));
                str.AppendFormat("&item_name={0}", HttpUtility.UrlEncode(trans.Name));
                str.AppendFormat("&item_description={0}", HttpUtility.UrlEncode(trans.Description));

                // Redirect to PayFast
                ctx.Response.Redirect(site + str.ToString(), false);
                return;
            }
            catch (Exception ex)
            {
                exceptionAction.Invoke(ex);
            }
        }

        /// <summary>
        /// PF will POST to this page once a customer pays. It will be validated accordingly BEFORE redirecting to PFMeta.URLNotify.
        /// This page need not have any output HTML, as it is never seen by an end-user
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="page"></param>
        /// <param name="ctx"></param>
        /// <param name="success">Code to run on Success</param>
        /// <param name="failed">Code to run on Failure</param>
        /// <param name="investigate">Code to run on Investigation Enquiry</param>
        /// <param name="exception">Code to run in the event of an Exception</param>
        public void Validate(Settings settings, Page page, HttpContext ctx, Action success, Action failed, Action<string> investigate, Action<Exception> exception)
        {
            // Can't have a postback on this page, it is called
            // once by the payment processor.
            if (page.IsPostBack) return;

            try
            {
                // Are we testing or making live payments
                string site = settings.IsLive ? "https://www.payfast.co.za/eng/query/validate" : "https://sandbox.payfast.co.za/eng/query/validate";
                string merchant_id = settings.IsLive ? settings.MerchantID : "10001056";

                // Get the posted variables. Exclude the signature (it must be excluded when we hash and also when we validate).
                NameValueCollection arrPostedVariables = new NameValueCollection(); // We will use later to post data
                NameValueCollection req = ctx.Request.Form;
                string key = "";
                string value = "";

                for (int i = 0; i < req.Count; i++)
                {
                    key = req.Keys[i];
                    value = req[i];

                    if (key != "signature")
                    {
                        strPostedVariables += key + "=" + value + "&";
                        arrPostedVariables.Add(key, value);
                    }
                }

                // Remove the last &
                strPostedVariables = strPostedVariables.TrimEnd(new char[] { '&' });

                // Also get the Ids early. They are used to log errors to the orders table.
                orderId = page.Request.Form["m_payment_id"];
                processorOrderId = page.Request.Form["pf_payment_id"];

                // Get the posted signature from the form.
                if (string.IsNullOrEmpty(page.Request.Form["signature"]))
                    throw new Exception("Signature parameter cannot be null");

                // Check if this is a legitimate request from the payment processor
                PerformSecurityChecks(arrPostedVariables, merchant_id);

                // The request is legitimate. Post back to payment processor to validate the data received
                ValidateData(site, arrPostedVariables);

                // All is valid, process the order
                if (arrPostedVariables["payment_status"] == "COMPLETE")
                    success.Invoke();
                else if (arrPostedVariables["payment_status"] == "FAILED")
                    failed.Invoke();
                else
                    investigate.Invoke(arrPostedVariables["payment_status"]);
            }
            catch (Exception ex)
            {
                exception.Invoke(ex);
            }
        }

        private void PerformSecurityChecks(NameValueCollection arrPostedVariables, string merchant_id)
        {
            // Verify that we are the intended merchant
            string receivedMerchant = arrPostedVariables["merchant_id"];

            if (receivedMerchant != merchant_id)
                throw new Exception("Mechant ID mismatch");

            // Get the requesting ip address
            string requestIp = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            if (string.IsNullOrEmpty(requestIp))
                throw new Exception("IP address cannot be null");

            // Is address in list of websites
            if (!IsIpAddressInList(requestIp))
                throw new Exception("IP address invalid");
        }

        private bool IsIpAddressInList(string ipAddress)
        {
            IPAddress ip = IPAddress.Parse(ipAddress);
            foreach (string site in _validSites)
                if (System.Net.Dns.GetHostAddresses(site).Contains(ip))
                    return true;
            return false;
        }

        private void ValidateData(string site, NameValueCollection arrPostedVariables)
        {
            WebClient webClient = null;

            try
            {
                webClient = new WebClient();
                byte[] responseArray = webClient.UploadValues(site, arrPostedVariables);

                // Get the response and replace the line breaks with spaces
                string result = Encoding.ASCII.GetString(responseArray);
                result = result.Replace("\r\n", " ").Replace("\r", "").Replace("\n", " ");

                // Was the data valid?
                if (result == null || !result.StartsWith("VALID"))
                    throw new Exception("Data validation failed");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (webClient != null)
                    webClient.Dispose();
            }
        }

    }
}
