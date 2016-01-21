<%@ Page Title="Product" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Product.aspx.cs" Inherits="DesconWeb.Product" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent" >
    <section class="featured">
        <div id="banner" class="content-wrapper divShadow" style="height: 100%; width:100%; margin:0 !important; min-width: 1000px !important; padding: 0 !important; background-image: url(../Images/Features_NewBanner1117.png);background-position: 0 0; background-size: 100% 100%; margin-top: -6%!important; padding-bottom:6% !important">
             <h2 class="noSize anchor" id="section1"></h2>
             <img src="../Images/Features_NewBanner1117.png" style="visibility: hidden; background-position: 0 0;height: 100%; width: 100%;margin-top: -6%; margin-bottom: 0%;" />

            
            <%--NO DISPLAY--%>
            <div class="featured centertext" style="display: none; background:url(/Images/HomepageBannerShrink.png); height: 100%; background-position: center; width:100%; margin-top: -10%; padding-top: 10%; padding-bottom: 10%;position: relative">
<%--                <div style="display: inline-block;">
                    <img src="../Images/DesconLogo7.png" style="float: left" />
                </div>--%>
                <img src="../Images/DesconLogo7.png" style="position: absolute; margin: 0 auto; margin-left: -25% !important" />
                  <hgroup style="text-align: center; position: absolute; left: 0; right: 0;">
                        <h1 style="font-size: 40px; margin-top: 170px; padding-bottom: 0 !important; margin-bottom: 0 !important">DESCON VERSION 8</h1>
                        <h2 style="">STRUCTURAL STEEL CONNECTION DESIGN SOFTWARE</h2>
                    </hgroup>

                <div style="position: fixed; top:100px; right: 75px;">
                   <a runat="server" href="#section1" style="display: block" class="navDots">
                       <img src="../Images/DesconDotWt.png">
                       <img src="../Images/DesconDotOra.png">
                   </a>
                   <a runat="server" href="#section2" style="display: block" class="navDots">
                       <img src="../Images/DesconDotWt.png">
                       <img src="../Images/DesconDotOra.png">
                   </a>
                   <a runat="server" href="#section3" style="display: block" class="navDots">
                       <img src="../Images/DesconDotWt.png">
                       <img src="../Images/DesconDotOra.png">
                   </a>
                   <a runat="server" href="#section4" style="display: block" class="navDots">
                       <img src="../Images/DesconDotWt.png">
                       <img src="../Images/DesconDotOra.png">
                   </a>
                   <a runat="server" href="#section5" style="display: block" class="navDots">
                       <img src="../Images/DesconDotWt.png">
                       <img src="../Images/DesconDotOra.png">
                   </a>
                </div>                
            </div>        

        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">   
    <section>
        <h2 class="noSize anchor" id="section2"></h2>
        <div id="featuresMatrix" style="text-align: center; position: relative; padding: 50px 0; margin: 0 !important;margin-left: -15% !important">
                <div style="background: transparent; ">                  
                    <div style="background: url(/Images/Descon8Diagram1117_Cropped.png) no-repeat; height: 605px; width: 866px; background-size: 866px 605px; background-position: center; display: inline-block; z-index: 1; padding: 0 !important; margin: 0 !important; ">
                    </div>
                    <div style="text-align: center; padding-bottom: 30px; margin-left: 235px">
                        <a runat="server" class="btn2bPricing" href="~/Pricing.aspx" style="color:#ed5424 !important;"><span>See Pricing</span></a>
                        <a runat="server" class="btn2bPricing" href="~/Pricing.aspx" style="color:#8DD9F0 !important;"><span>See Pricing</span></a>  
                    </div>
                    <p style="font-size: 12px; color:gray; text-align: center; margin-left: 235px">*Version 8 has a rolling release <br/>Open and Basic were released on November 12th, 2015 
                        <br/>Both Version 7 and Version 8 can be used during the rolling , if needed <br/><a style="color:#ED5424" runat="server" href="~/Pricing.aspx">Click Here</a> for bracing in Version 7</p>
                </div> 
        </div>
    </section>
    
    <section id="blueInfo">
        <h2 class="noSize anchor" id="section3"></h2>
        <a id="arrow" class="arrowdivider hidden">
            <img src="../Images/DesconArrowDkBl.png" style="position: relative; margin: auto; float: left; margin-left: 48%; ">
        </a>
        <div id="blueXdiv" class="featureBlueDiv divShadow" style="margin: 0px; padding-top: 0px; height:2050px !important; width: 100% !important;min-width: 1000px !important; position: relative; overflow-x: hidden" >
        <ul style="font-size: 32px; text-align: center; padding-top: 50px; padding-bottom: 80px;">A look at Descon software</ul>
        <div style="text-align: center; position: relative; padding-bottom: 175px;">
            <div style="display: inline-block;">
               <img id="Feat1" src="../Images/FeatureImg2_1.png" class="featureImgFeature" style=""> 
            </div>
            <div style="display: inline-block;">
               <img id="Feat2" src="../Images/FeatureImg2_2.png" class="featureImgFeature" style=""> 
            </div>
            <div style="display: inline-block;">
               <img id="Feat3" src="../Images/FeatureImg2_3.png" class="featureImgFeature" style=""> 
            </div>
            <div style="display: inline-block;">
               <img id="Feat4" src="../Images/FeatureImg2_4.png" class="featureImgFeature" style=""> 
            </div>
            <div style="display: inline-block;">
               <img id="Feat5" src="../Images/FeatureImg2_5.png" class="featureImgFeature" style=""> 
            </div>
            <div style="display: inline-block;">
               <img id="Feat6" src="../Images/FeatureImg2_6.png" class="featureImgFeature" style=""> 
            </div>

            <div style="display: block; padding: 20px 0; text-align: center">
                <div id="TextFeat1" class="hidden" style="float: left" >
                    <ul class="productMiniFeatureBig">2D Views</ul>
                    <ul class="productMiniFeatureSmall">Customize all four dynamically modified 2D views. </ul>
                </div> 
                <div id="TextFeat2" class="hidden" style="float: left" >
                    <ul class="productMiniFeatureBig">3D Views</ul>
                    <ul class="productMiniFeatureSmall">Visualize complex connections with a dynamic 3D view. </ul>
                </div> 
                <div id="TextFeat3" class="hidden" style="float: left" >
                    <ul class="productMiniFeatureBig">On the Fly</ul>
                    <ul class="productMiniFeatureSmall">Switch specifications whenever needed, even mid-design. It just takes the click of a mouse.</ul>
                </div>
                <div id="TextFeat4" class="hidden" style="float: left" >
                    <ul class="productMiniFeatureBig">Feedback</ul>
                    <ul class="productMiniFeatureSmall">See the governing limit state at a glance, as well as the strength ratios of all limit states. </ul>
                </div>
                <div id="TextFeat5" class="hidden" style="float: left" >
                    <ul class="productMiniFeatureBig">Speed & Efficiency</ul>
                    <ul class="productMiniFeatureSmall">Create working designs with your default preferences in just a few clicks. </ul>
                </div>
                <div id="TextFeat6" class="hidden" style="float: left" >
                    <ul class="productMiniFeatureBig">Double Check</ul>
                    <ul class="productMiniFeatureSmall">If any report doesn't pass Descon's checks, Descon will give a clear diagnosis that you can use to track down any problems. </ul>
                </div>
            </div>
        </div>
        <section style="">           
            <img src="../Images/Features_CenterLine3.png" style="display: block;  margin:auto; position: absolute;left: 0;right: 0; height: 1000px; margin-top: 4px;"/>            
            <div style="color:#DFE0E1 !important;position: relative; min-width: 1000px !important;">
                
                <div class="productFeatureSectionMain" style="">
                    <img src="../Images/Features_Screenshots-02.png" class="clearAll featureScreenCenter" style=""> 
                    <%--<ul class="productFeatureTextY featureTextYCenter clearAll" style="color:#493B99; margin-top: 145px !important;">Graphics</ul>--%>
                    <div class="clearAll">
                        <ul class="rightSideText largeProductFeature" style="color:#493B99;">VISUALIZE COMPLEX CONNECTIONS WITH<br/>2D & 3D VIEWS</ul>
                        <%--<ul class="rightSideText smallestProductFeature" style="height:auto; width: 30% !important">Not only does it help you save time and give you an incredible way to visualize your connections, but it's also fun to watch.</ul>  --%>
                        <ul class="rightSideText smallProductFeature" style="text-transform: uppercase; font-weight: bolder; height: auto;">GRAPHICS & DESIGN FEATURES</ul>   
                        <ul class="rightSideText smallestProductFeature productList1" style="margin-left:54.5%; height: auto">
                            <li><font color="#DFE0E1" size="2.5em" face="CleanLight" >Visualize complex connections with 2D and 3D views</font></li> 
                            <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Display any combination of 5 separate drawing views</font></li> 
                            <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Dynamically drag and stretch callouts and dimensions</font></li>
                            <li><font color="#DFE0E1" size="2.5em" face="CleanLight">Dynamically zoom, pan and rotate drawings</font></li> 
                            <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Customize preferences for material types and thicknesses</font></li> 
                            <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Click on connection elements to directly access inputs and forms</font></li>  
                        </ul>   
                    </div>            
                </div>
<%--                <div class="productFeatureSection" style="top:320px;">
                    <img src="../Images/Features_Screenshots-04.png" class="clearAll featureScreenCenter" style=""> 
                    <ul class="productFeatureTextY featureTextYCenter clearAll" style="color:#5551A1; margin-top: 150px !important;">Modeling</ul>
                    <div class="clearAll" style="">
                        <ul class="rightSideText largeProductFeature" style="color:#5551A1; max-width: 600px !important">LEFT/RIGHT OF CONNECTIONS <br/> TOP/BOTTOM OF BRACES MODELED TOGETHER</ul>
                        <ul class="rightSideText smallestProductFeature" style="height:auto; ">See each side of your connection together and make adjustments at the same time.</ul>
                        <ul class="rightSideText smallProductFeature" style="text-transform: uppercase; font-weight: bolder; height: auto;">Similar Modeling Features</ul>   
                        <ul class="rightSideText smallestProductFeature productList2" style="margin-left:54.5%; height: auto">
                            <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Instant Synchronization between Spreadsheets and Graphics</font></li> 
                            <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Support for Multiple Imperial and Metric Unit Systems</font></li> 
                            <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Automatic Beam Coping (Web and Flanges)</font></li> 
                            <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Left/Right of Connections Modeled Together (Beams/Braces to Column)</font></li> 
                            <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Top/Bottom of Braces Modeled Together (Braces to Beam)</font></li> 
                            <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">User Shape Input Capability</font></li> 
                            <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Generates Batch Cases with Excel</font></li> 
                        </ul> 
                    </div>
   
                </div>--%>
                <div class="productFeatureSection"  style="top:332px;">
                    <img src="../Images/Features_Screenshots-03.png" class="clearAll featureScreenCenter" style=""> 
                    <%--<ul class="productFeatureTextY featureTextYCenter clearAll" style="color:#5D93CB;  margin-top: 125px !important;">Results</ul>--%>
                    <ul class="rightSideText largeProductFeature" style="color:#5551A1">QUICKLY VIEW LIMIT STATES<br/>WITH RATIO GAUGES</ul>
                    <%--<ul class="rightSideText smallestProductFeature" style="height:auto; width: 30% !important">If any report doesn't pass Descon's checks, Descon will give a clear diagnosis that you can use to track down any problems.</ul>--%>
                    <ul class="rightSideText smallProductFeature" style="text-transform: uppercase; font-weight: bolder; height: auto;">Analysis & Results Features</ul>   
                    <ul class="rightSideText smallestProductFeature productList2" style="margin-left:54.5%; height: auto">
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Quickly view limit states with ratio gauges</font></li> 
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Easily access calculations by clicking on individual gauges</font></li> 
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">See overall connection capacity at a glance</font></li> 
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Automatically creates optimized designs </font></li> 
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Switch On the Fly between AISC 13th/14th and ASD/LRFD</font></li> 
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Drawings and reports update instantly with input changes</font></li>
                    </ul>    
                </div>
                <div class="productFeatureSection"  style="top:655px;">
                    <img src="../Images/Features_Screenshots-01.png" class="clearAll featureScreenCenter" style="">
                    <%--<ul class="productFeatureTextY featureTextYCenter clearAll" style="width:450px; margin-left: -248px !important; margin-top: 105px !important; color: #8DD9F0; margin-top: 173px !important;">Design & Analysis</ul> --%>
                    <ul class="rightSideText largeProductFeature" style="color: #5D93CB;">COMPLETE HAND-STYLE CALCULATIONS<br/>IN .PDF, .RTF, OR .HTML FORMAT</ul>
                    <%--<ul class="rightSideText smallestProductFeature">Descon software maintains the latest AISC codes and ordinances.</ul>--%>
<%--                    <ul class="rightSideText smallestProductFeature productList3" style="margin-left:54.5%; height: auto; padding-bottom: 16px;">
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">AISC 360-05 (13th Edition): ASD & LRFD</font></li> 
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">AISC 360-10 (14th Edition): ASD & LRFD</font></li> 
                    </ul> --%>
                    <ul class="rightSideText smallProductFeature" style="text-transform: uppercase; font-weight: bolder; height: auto;">Reporting & Output Features</ul>   
                    <ul class="rightSideText smallestProductFeature productList3" style="margin-left:54.5%; height: auto">
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Complete hand-style calculations in .pdf, .rtf, or .html format</font></li> 
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Search calculations by headings or keywords</font></li> 
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Customizable report header, including company logo</font></li> 
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Add highlighting, comments and bookmarks to reports</font></li> 
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Organize and separate drawings on report pages</font></li> 
                        <li><font color="#DFE0E1" size ="2.5em" face="CleanLight">Quickly cycle through no-goods (NG’s) and bookmarks</font></li> 
                    </ul>    
                </div>
            </div>
        </section>
    </div>
    </section>

    <script type="text/javascript">
        $(document).ready(function () {
            $('#quoteSection').addClass('divExtended');
            $('#orangeSection').addClass('divExtended');
            $('#whiteSection').addClass('divExtended');
            $('#o2wDots').addClass('divExtended');
            $('#footer').addClass('divExtended');
            //($('#quoteSectionDiv').css("margin-top", -300));

            var width = $(window).width();
            if (width < 925) {
                var addPixels = (925 - width) * .15;
                var margin = addPixels;
                $('#featuresMatrix').css("margin-left", margin);
            }

            function isInView(elem) {
                return $(elem).offset().top - $(window).scrollTop() < $(elem).height();
            }
            $(window).scroll(function () {
                if (isInView($('#TextFeat1'))) {
                    if ($('#TextFeat1').hasClass('hidden')) {
                        $('#TextFeat1').removeClass('hidden');
                        $('#TextFeat1').css('marginLeft', 0);
                        $('#TextFeat1').animate({ marginLeft: '+=' + (($(window).width() / 2) - ($('#TextFeat1').width() / 2)) }, "fast");
                    }
                }
            });

        });
        $('#Feat1').hover(function () {
            var offset = (($(window).width() / 2) - ($('#TextFeat1').width() / 2));
            if ($('#TextFeat1').hasClass('hidden')) {
                $('#TextFeat1').removeClass('hidden');
                $('#TextFeat1').css('marginLeft', 0);
                var currentMargin = $('#TextFeat1').css("marginLeft");
                if (parseFloat(currentMargin) < parseFloat(offset)) $('#TextFeat1').animate({ marginLeft: '+=' + offset }, "fast");
            }
            //else {
            //    $('#TextFeat1').addClass('hidden');               
            //}
        });
        $('#Feat2').hover(function () {
            var offset = (($(window).width() / 2) - ($('#TextFeat2').width() / 2));
            if (!$('#TextFeat1').hasClass('hidden')) { $('#TextFeat1').addClass('hidden'); }
            if ($('#TextFeat2').hasClass('hidden')) {
                $('#TextFeat2').removeClass('hidden');
                $('#TextFeat2').css('marginLeft', 0);
                var currentMargin = $('#TextFeat2').css("marginLeft");
                if (parseFloat(currentMargin) < parseFloat(offset)) $('#TextFeat2').animate({ marginLeft: '+=' + offset }, "fast");
            }
            else {
                $('#TextFeat2').addClass('hidden');
            }
        });
        $('#Feat3').hover(function () {
            var offset = (($(window).width() / 2) - ($('#TextFeat3').width() / 2));
            if (!$('#TextFeat1').hasClass('hidden')) { $('#TextFeat1').addClass('hidden'); }
            if ($('#TextFeat3').hasClass('hidden')) {
                $('#TextFeat3').removeClass('hidden');
                $('#TextFeat3').css('marginLeft', 0);
                var currentMargin = $('#TextFeat3').css("marginLeft");
                if (parseFloat(currentMargin) < parseFloat(offset)) $('#TextFeat3').animate({ marginLeft: '+=' + offset }, "fast");
            }
            else {
                $('#TextFeat3').addClass('hidden');
            }
        });
        $('#Feat4').hover(function () {
            var offset = (($(window).width() / 2) - ($('#TextFeat4').width() / 2));
            if (!$('#TextFeat1').hasClass('hidden')) { $('#TextFeat1').addClass('hidden'); }
            if ($('#TextFeat4').hasClass('hidden')) {
                $('#TextFeat4').removeClass('hidden');
                $('#TextFeat4').css('marginLeft', 0);
                var currentMargin = $('#TextFeat4').css("marginLeft");
                if (parseFloat(currentMargin) < parseFloat(offset)) $('#TextFeat4').animate({ marginLeft: '+=' + offset }, "fast");
            }
            else {
                $('#TextFeat4').addClass('hidden');
            }
        });
        $('#Feat5').hover(function () {
            var offset = (($(window).width() / 2) - ($('#TextFeat5').width() / 2));
            if (!$('#TextFeat1').hasClass('hidden')) { $('#TextFeat1').addClass('hidden'); }
            if ($('#TextFeat5').hasClass('hidden')) {
                $('#TextFeat5').removeClass('hidden');
                $('#TextFeat5').css('marginLeft', 0);
                var currentMargin = $('#TextFeat5').css("marginLeft");
                if (parseFloat(currentMargin) < parseFloat(offset)) $('#TextFeat5').animate({ marginLeft: '+=' + offset }, "fast");
            }
            else {
                $('#TextFeat5').addClass('hidden');
            }
        });
        $('#Feat6').hover(function () {
            var offset = (($(window).width() / 2) - ($('#TextFeat6').width() / 2));
            if (!$('#TextFeat1').hasClass('hidden')) { $('#TextFeat1').addClass('hidden'); }
            if ($('#TextFeat6').hasClass('hidden')) {
                $('#TextFeat6').removeClass('hidden');
                $('#TextFeat6').css('marginLeft', 0);
                var currentMargin = $('#TextFeat6').css("marginLeft");
                if (parseFloat(currentMargin) < parseFloat(offset)) $('#TextFeat6').animate({ marginLeft: '+=' + offset }, "fast");

            }
            else {
                $('#TextFeat6').addClass('hidden');
            }
        });
    </script>

</asp:Content>

