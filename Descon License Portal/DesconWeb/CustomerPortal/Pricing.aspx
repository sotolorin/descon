<%@ Page Title="Pricing" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Pricing.aspx.cs" Inherits="DesconWeb.Pricing" %>


<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent" >
    <section class="featured">
        <div id="banner" class="content-wrapper divShadow" style="height: 100%; width:100%; margin:0 !important; min-width: 1000px !important; padding: 0 !important; background-image: url(../Images/PricingBanner.png);background-position: 0 0; background-size: 100% 100%; margin-top: -7%!important; padding-bottom:7% !important">
             <h2 class="noSize anchor" id="section1"></h2>
             <img src="../Images/PricingBanner.png" style="visibility: hidden; background-position: 0 0;height: 100%; width: 100%;margin-top: -7%; margin-bottom: 0%;" />
        </div>
    </section>
</asp:Content>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">   
    <section>
        <h2 class="noSize anchor" id="section2"></h2>
        <div id="featuresMatrix" style="text-align: center; position: relative; padding: 50px 0; margin: 0 !important; min-width: 1000px">
            <div style="background: transparent; ">
                <p style="font-size: 18px; color:#1C1C2D; text-align: center;">Test our powerful structural steel connection design software and find out what Descon can do for you.</p>   
                <a runat="server" class="btn2b btn-2 btn-2a" href="http://web.rexww.com/cn/ademz/desconfreetrial" data-rel="external" target="_blank" style="padding: 15px; display: block; margin: 50px auto; "><span>Request Free Trial</span></a>               
                <div style="z-index: 1; position: relative; background: url(/Images/DesconPricingFeature2.png) no-repeat; height: 860px; width: 900px; max-height: 100%; max-width: 100%; background-size: 900px 860px; padding: 0 !important; margin: 0 auto !important; ">
                    <p class="manidbold" style="font-size: 11px; color:black; text-align: center; position: absolute; top: 640px; left: 695px; z-index: 2"><a href="http://store.desconplus.com" data-rel="external" target="_blank" style="color:#ED5424">CLICK HERE</a> TO VIEW<br/>BRACING IN VERSION 7.</p>
                </div>
                <p style="font-size: 14px; color:#1C1C2D; text-align: center;">You can also choose a one-time purchase, a Set Plan, which means no new features and no support.
                    <br/>This option is not eligible for upgrade pricing, This option does not have the option to add multiple Users that share 1 seat.</p>               
                <a runat="server" class="btn2b btn-2 btn-2a" href="http://store.desconplus.com" data-rel="external" target="_blank" style="padding: 15px; display: block; margin: 50px auto;color:#2B338A !important "><span>GO TO STORE</span></a>   
            </div> 
        </div>
    </section>
    
    <section id="blueInfo">
        <h2 class="noSize anchor" id="section3"></h2>
        <a id="arrow" class="arrowdivider hidden">
            <img src="../Images/DesconArrowDkBl.png" style="position: relative; margin: auto; float: left; margin-left: 48%; ">
        </a>
        <div  id="bluePdiv" class="pricingBlueDiv divShadow" style="margin: 0px; padding-top: 0px; height:1700px; width: 100% !important;min-width: 1000px !important; position: relative; overflow-x: hidden" >
        <ul style="font-size: 32px; text-align: center; padding-top: 80px; padding-bottom: 30px;">Here's how it works</ul>
        <div style="margin: 0 auto; position: relative; padding-bottom: 175px; background: url(/Images/DesconPricingNbrList2.png) no-repeat; height: 1041px; width: 1000px; background-size: 1000px 1041px; max-height: 100%; max-width: 100%;">
        </div>
    </div>
    </section>

    <script type="text/javascript">
        $(document).ready(function () {
            $('#quoteSection').addClass('divExtended');
            $('#orangeSection').addClass('divExtended');
            $('#whiteSection').addClass('divExtended');
            $('#o2wDots').addClass('divExtended');
            $('#footer').addClass('divExtended');
        });
        $(document).ready(function () {
            var width = $(window).width();
            if (width < 830) {
                var intervals = (830 - width) / 30;
                var addHeight = intervals * 20;
                $('#bluediv').height($('#bluediv').height() + addHeight);
                ($('#quoteSectionDiv').css("margin-top", -350 - addHeight));
                //alert($('#quoteSectionDiv').css("margin-top"));
            }
        });
    </script>

</asp:Content>