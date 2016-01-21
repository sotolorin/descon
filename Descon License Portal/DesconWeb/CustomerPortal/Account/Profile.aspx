<%@ Page Title="Profile" Language="C#" MasterPageFile="~/Portal.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="DesconWeb.Account.Profile" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="PortalMainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
        <h2>Fill the form below to update Portal Profile</h2>
    </hgroup>
    <div>
        <p class="message-info">
                        Message Text Here.
                    </p>
                    <p class="validation-summary-errors">
                        <asp:Literal runat="server" ID="Error" />
                    </p>

                    <fieldset>
                        <legend>Portal Profile</legend>
                        <ol>
                            <li>
                                <asp:Label runat="server" AssociatedControlID="CompanyName">Company Name</asp:Label>
                                <asp:TextBox runat="server" ID="CompanyName" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="CompanyName"
                                    CssClass="field-validation-error" Error="The company name field is required." />
                            </li>
                            <li>
                                <asp:Label runat="server" AssociatedControlID="Address1">Address 1</asp:Label>
                                <asp:TextBox runat="server" ID="Address1" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="Address1"
                                    CssClass="field-validation-error" Error="The address field is required." />
                            </li>
                            <li>
                                <asp:Label runat="server" AssociatedControlID="Address2">Address 2</asp:Label>
                                <asp:TextBox runat="server" ID="Address2" />
                            </li>
                           <li>
                                <asp:Label runat="server" AssociatedControlID="City">City</asp:Label>
                                <asp:TextBox runat="server" ID="City" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="City"
                                    CssClass="field-validation-error" Error="The city field is required." />
                            </li>
                            <li>
                                <asp:Label runat="server" AssociatedControlID="State">State</asp:Label>
                                <asp:DropDownList ID="State" runat="server" Height="40" Font-Names="Arial" CssClass="ddl">
	                                <asp:ListItem Value="AL">Alabama</asp:ListItem>
	                                <asp:ListItem Value="AK">Alaska</asp:ListItem>
	                                <asp:ListItem Value="AZ">Arizona</asp:ListItem>
	                                <asp:ListItem Value="AR">Arkansas</asp:ListItem>
	                                <asp:ListItem Value="CA">California</asp:ListItem>
	                                <asp:ListItem Value="CO">Colorado</asp:ListItem>
	                                <asp:ListItem Value="CT">Connecticut</asp:ListItem>
	                                <asp:ListItem Value="DC">District of Columbia</asp:ListItem>
	                                <asp:ListItem Value="DE">Delaware</asp:ListItem>
	                                <asp:ListItem Value="FL">Florida</asp:ListItem>
	                                <asp:ListItem Value="GA">Georgia</asp:ListItem>
	                                <asp:ListItem Value="HI">Hawaii</asp:ListItem>
	                                <asp:ListItem Value="ID">Idaho</asp:ListItem>
	                                <asp:ListItem Value="IL">Illinois</asp:ListItem>
	                                <asp:ListItem Value="IN">Indiana</asp:ListItem>
	                                <asp:ListItem Value="IA">Iowa</asp:ListItem>
	                                <asp:ListItem Value="KS">Kansas</asp:ListItem>
	                                <asp:ListItem Value="KY">Kentucky</asp:ListItem>
	                                <asp:ListItem Value="LA">Louisiana</asp:ListItem>
	                                <asp:ListItem Value="ME">Maine</asp:ListItem>
	                                <asp:ListItem Value="MD">Maryland</asp:ListItem>
	                                <asp:ListItem Value="MA">Massachusetts</asp:ListItem>
	                                <asp:ListItem Value="MI">Michigan</asp:ListItem>
	                                <asp:ListItem Value="MN">Minnesota</asp:ListItem>
	                                <asp:ListItem Value="MS">Mississippi</asp:ListItem>
	                                <asp:ListItem Value="MO">Missouri</asp:ListItem>
	                                <asp:ListItem Value="MT">Montana</asp:ListItem>
	                                <asp:ListItem Value="NE">Nebraska</asp:ListItem>
	                                <asp:ListItem Value="NV">Nevada</asp:ListItem>
	                                <asp:ListItem Value="NH">New Hampshire</asp:ListItem>
	                                <asp:ListItem Value="NJ">New Jersey</asp:ListItem>
	                                <asp:ListItem Value="NM">New Mexico</asp:ListItem>
	                                <asp:ListItem Value="NY">New York</asp:ListItem>
	                                <asp:ListItem Value="NC">North Carolina</asp:ListItem>
	                                <asp:ListItem Value="ND">North Dakota</asp:ListItem>
	                                <asp:ListItem Value="OH">Ohio</asp:ListItem>
	                                <asp:ListItem Value="OK">Oklahoma</asp:ListItem>
	                                <asp:ListItem Value="OR">Oregon</asp:ListItem>
	                                <asp:ListItem Value="PA">Pennsylvania</asp:ListItem>
	                                <asp:ListItem Value="RI">Rhode Island</asp:ListItem>
	                                <asp:ListItem Value="SC">South Carolina</asp:ListItem>
	                                <asp:ListItem Value="SD">South Dakota</asp:ListItem>
	                                <asp:ListItem Value="TN">Tennessee</asp:ListItem>
	                                <asp:ListItem Value="TX">Texas</asp:ListItem>
	                                <asp:ListItem Value="UT">Utah</asp:ListItem>
	                                <asp:ListItem Value="VT">Vermont</asp:ListItem>
	                                <asp:ListItem Value="VA">Virginia</asp:ListItem>
	                                <asp:ListItem Value="WA">Washington</asp:ListItem>
	                                <asp:ListItem Value="WV">West Virginia</asp:ListItem>
	                                <asp:ListItem Value="WI">Wisconsin</asp:ListItem>
	                                <asp:ListItem Value="WY">Wyoming</asp:ListItem>
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="State"
                                    CssClass="field-validation-error" Error="The state field is required." />
                            </li>
                            <li>
                                <asp:Label runat="server" AssociatedControlID="Zip">Zip Code</asp:Label>
                                <asp:TextBox runat="server" ID="Zip" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="Zip"
                                    CssClass="field-validation-error" Error="The zip code field is required." />
                            </li>
                            <li>
                                <asp:Label runat="server" AssociatedControlID="UserCount">Number of Users</asp:Label>
                                <asp:TextBox runat="server" ID="UserCount" Width="50"/>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="UserCount"
                                    CssClass="field-validation-error" Error="The user count field is required." />
                                <asp:RegularExpressionValidator runat="server" ControlToValidate="UserCount" 
                                      CssClass="field-validation-error" Error="Enter only numeric characters." ValidationExpression="^[1-9]\d*$" />
                            </li>
                            <li>
                                <asp:Label runat="server" AssociatedControlID="Email">Email address</asp:Label>
                                <asp:TextBox runat="server" ID="Email" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="Email"
                                    CssClass="field-validation-error" Error="The email address field is required." />
                            </li>
                        </ol>
                        <asp:Button runat="server" CommandName="MoveNext" Text="Save Changes" OnClick="SaveAccount"/>
                    </fieldset>
    </div>
</asp:Content>