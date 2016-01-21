<%@ Page Title="Welcome" Language="C#" MasterPageFile="~/Portal.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="CustomerPortal._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="PortalFeaturedContent">
    <section class="featured">
        <div class="content-wrapper2">
            <hgroup class="title">
                <%--<h1><%: Title %>.</h1>--%>
                <h1>Welcome to Descon Plus Customer Portal. <br/></h1>
                <h2>Here you can manage your accounts, users, and Descon application</h2>
            </hgroup>
            <p>
                Existing users may login to access their accounts. 
                If you do not have an online account with us you 
                can register and start accessing the portal today 
                by clicking <a href="Account/Register.aspx" title="here">here</a>.
            </p>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="PortalMainContent">
    <h3>Select One:</h3>
    <div>
        <a runat="server" class="btn5 btn-5 btn-5b icon-cog" href="~/Account/Login.aspx" style="color:white; font-size: 22px"><span>LOGIN</span></a>
        <a runat="server" class="btn5 btn-5 btn-5b icon-register" href="~/Account/Register.aspx" style="color:white; font-size: 22px"><span>CREATE ACCOUNT</span></a>
        <a runat="server" class="btn5 btn-5 btn-5b icon-support" href="~/Pages/SupportLinks.aspx" style="color:white; font-size: 22px"><span>SUPPORT</span></a>
    </div>
</asp:Content>
