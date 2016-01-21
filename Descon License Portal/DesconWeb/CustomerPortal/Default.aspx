<%@ Page Title="Welcome" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="DesconWeb._Default" %>
<%--<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">--%>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent" >
    <section class="featured">
        <div id="banner" class="content-wrapper" style="background: white; height: 100%; width:100%; margin:0 !important;position: relative">
             <h2 class="noSize anchor" id="section1"></h2>
            <div style="width: 20%; position: absolute; right: 3%; bottom: 5%;">
                <a id="diffClick" class="btn2bH2 btn-2 btn-2a" style="background: white; padding: 1.5% 0 !important; width: 90% ; margin-right: 5% !important; margin-bottom: 5% !important"><span>See How We're Different</span></a>
                <a runat="server" class="btn2bH btn-2 btn-2a smallLineHeight" href="~/Pricing.aspx" style="background:#ED5424;padding: 3% 0 !important;  margin-bottom: 5% !important"><span>Get Descon Open Today<br><font class="smallButtonText">NO PAYMENT REQUIRED</font></span></a>
                <a runat="server" class="btn2bH btn-2 btn-2a" href="~/Product.aspx" style="background:#3850A1; padding: 1.5% 0 !important; width: 90% ;margin-right: 5% !important"><span>Compare Software Levels</span></a>   
            </div>
            <img src="../Images/Homepage_NewBanner1117.png"  class="featured centertext" style="height: 100%; width: 100%; margin-top: -6% !important;"/>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <section>
        <h2 class="noSize anchor" id="section2"></h2>
        <div style="text-align: center; position: relative; margin-top: 50px; margin-bottom: 50px;">
        <div style="display: inline-block;">
           <img src="../Images/Homepage_Icon_2DView.png" class="featureImgHm" style=""> 
            <ul style="color:#ED5424; font-size: 22px; margin-left: -20px;  ">2D and 3D views</ul>
            <ul style="color:#1C1C2D; font-size: 14px; margin-left: -20px; ">Customize all four dynamically modified 2D views and<br/>visualize complex connections with a 360°​ 3D view.​ </ul>
        </div>
        <div style="display: inline-block">
            <img src="../Images/Homepage_Icon_Speed.png" class="featureImgHm" style="">
            <ul style="color:#ED5424; font-size: 22px; margin-left: -20px;">Efficiency & Speed</ul>
            <ul style="color:#1C1C2D; font-size: 14px; margin-left: -20px; ">Create working designs with your<br/>default preferences in just a few clicks. </ul>
        </div>
        <div style="display: inline-block">
            <img src="../Images/Homepage_Icon_ResponsiveReport.png" class="featureImgHm" style="">
            <ul style="color:#ED5424; font-size: 22px; margin-left: -20px; ">Responsive Results</ul>
            <ul style="color:#1C1C2D; font-size: 14px; margin-left: -20px; ">Graphics and report views update instantly with data<br/>inputs, allowing you to quickly optimize designs. </ul>
        </div>
    </div>
    </section>
    
    <section id="blueInfo">
        <h2 class="noSize anchor" id="section3"></h2>
        <a id="arrow" class="arrowdivider hidden">
            <img src="../Images/DesconArrowDkBl.png" style="position: relative; margin: auto; float: left; margin-left: 48%; ">
        </a>
        <div id="bluediv" class="darkBlueDiv divShadow" style="margin: 0px; padding-top: 0px; height: 1850px;" >
            <img src="../Images/DesconLogoBlReplacement.png" style="display: block;  margin:auto; position: relative;left: 0;right: 0; margin-top: 50px !important; margin-bottom: -225px; padding-bottom: 0; max-width: 100%; max-height: 100%"/>
            <ul style="font-size: 34px; text-align: center; padding-top: 250px; padding-bottom: 80px;">How We're Different</ul>
            <div style="margin: 0 auto; position: relative; padding-bottom: 175px; background: url(/Images/DesconHomeBlFeature.png) no-repeat; height: 814px; width: 1000px; background-size: 1000px 814px; max-height: 100%; max-width: 100%;">
            </div>
<%--            <section style="display: inline-block">
                <img src="../Images/DesconDotsB7.png" style="display: block;  margin:auto; position: absolute;left: 0;right: 0; "/>

                <div class="homeFeatureSectionMain">
                    <ul class="leftSideText largeHomeFeature">What you need</ul>
                    <ul class="leftSideText smallHomeFeature">Whether you work with structural connections constantly or just once in a while, Descon is a comprehensive tool that is used with each stage of the connection design process. 30 years of user input has directly guided our development team. That’s not going to change.  View our features page to see everything Descon has to offer.</ul>       
                </div>
    
                <div class="homeFeatureSection">
                    <ul class="rightSideText largeHomeFeature">When you need it</ul>
                    <ul class="rightSideText smallHomeFeature">Descon is available in a variety of packages, allowing your schedule to call the shots.  We have month-to-month plans called "Flex", that give you the ability to adjust users and software levels by month. We also offer Annual Maintenance Plans (AMP) that give you access year after year while keeping your software current.  Visit our pricing page for more details.</ul>
                    </div>
                <div class="homeFeatureSection">
                    <ul class="leftSideText largeHomeFeature">Allowing time</ul>
                    <ul class="leftSideText smallHomeFeature">Descon creates a report while you work, updating as you input or change any data, so you can focus on the design. With Descon, a working clip angle shear connection to a column flange is ready in as few as 2 clicks and 3 keyboard inputs.  That’s basically just under a minute. Save time; stay accurate: use Descon.</ul>
                    </div>
                <div class="homeFeatureSection" style="margin-bottom: 250px">
                    <ul class="rightSideText largeHomeFeature">To do more</ul>
                    <ul class="rightSideText smallHomeFeature">Beyond powerful software, Descon is a community. Users from around the world and our developers work together in support of industry progress. Our monthly email offers a way to share your distinct perspective and learn from others who work with structural steel connections.</ul>
                </div>
            </section>--%>
        </div>
    </section>
    <script type="text/javascript">
        function debounce(func, wait, immediate) {
            var timeout;
            return function () {
                var context = this, args = arguments;
                var later = function () {
                    timeout = null;
                    if (!immediate) func.apply(context, args);
                };
                var callNow = immediate && !timeout;
                clearTimeout(timeout);
                timeout = setTimeout(later, wait);
                if (callNow) func.apply(context, args);
            };
        };
    </script>
    <script type="text/javascript">
        var scrolled = 0;
        $('#diffClick').click(function ()
        {
            scrolled = scrolled + 900;

            $(window).animate({
                scrollTop: scrolled
            });
        });
        //min window scroll starts at about 650px. Hector quote moves to orange section at about 830px. Every 30 px away from this add 30 px to height of blue div.
        $(document).ready(function () {
            //alert(5);
            var width = $(window).width();
            if (width < 830)
            {
                var intervals = (830 - width) / 30;
                var addHeight = intervals * 20;
                $('#bluediv').height($('#bluediv').height() + addHeight);
                ($('#quoteSectionDiv').css("margin-top", -350 - addHeight));
                //alert($('#quoteSectionDiv').css("margin-top"));
            }
        });
        //$(window).bind('resize', function (e) {
        //    window.resizeEvt;
        //    $(window).resize(function () {
        //        clearTimeout(window.resizeEvt);
        //        window.resizeEvt = setTimeout(function ()
        //        {
        //            var width = $(window).width();
        //            if (width < 830) {
        //                var intervals = (830 - width) / 30;
        //                var addHeight = intervals * 20;
        //                $('#bluediv').height($('#bluediv').height() + addHeight);
        //                ($('#quoteSectionDiv').css("margin-top", -350 - addHeight));
        //                //alert($('#quoteSectionDiv').css("margin-top"));
        //            }
        //            else if (width >= 830) {
        //                $('#bluediv').css("height", 1850);
        //                ($('#quoteSectionDiv').css("margin-top", -350));
        //            }
        //            //code to do after window is resized
        //        }, 250);
        //    });
        //});
        //$(window).resize(function () {
        //    var width = $(window).width();
        //    if (width < 830)
        //    {
        //        var intervals = (830 - width) / 30;
        //        var addHeight = intervals * 30;
        //        $('#bluediv').height($('#bluediv').height() + addHeight);
        //        ($('#quoteSectionDiv').css("margin-top", -350 - addHeight));
        //        //alert($('#quoteSectionDiv').css("margin-top"));
        //    }
        //    else if (width > 830 && $('#quoteSectionDiv').css("margin-top") < -350)
        //    {
        //        $('#bluediv').height(1850);
        //        ($('#quoteSectionDiv').css("margin-top", -350));
        //    }
        //});
    </script>
</asp:Content>

