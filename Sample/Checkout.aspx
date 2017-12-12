<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Checkout.aspx.cs" Inherits="Sample.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <h1>Checkout</h1>
        <div>
            <asp:Button ID="btnPayNow" Text="Pay Now" OnClick="btnPayNow_Click"/>
        </div>
    </form>
</body>
</html>
