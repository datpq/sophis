<?xml version="1.0" encoding="utf-8"?>
<!--<?xml-stylesheet type="text/xsl" href="decimalformat.xsl"?>-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:x="urn:schemas-microsoft-com:office:excel" 
  xmlns:reporting="http://www.sophis.net/reporting" 
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:ms="urn:schemas-microsoft-com:xslt"
    xmlns:o="urn:schemas-microsoft-com:office:office"    
    xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
    xmlns:html="http://www.w3.org/TR/REC-html40">
  <xsl:output method="xml" encoding="UTF-8" indent="yes"/>
  <xsl:decimal-format name="coerce" NaN="0" infinity="0" decimal-separator='.' grouping-separator=',' />
  <!--<xsl:decimal-format name="test" decimal-separator="." grouping-separator=","/>-->

 <!-- <xsl:template name="Values" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
    <xsl:param name="FundName"/>
    <xsl:variable name="FundName"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio/@reporting:name"/>
    
    <xsl:variable name="ucitsCommitmentCurrFund"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name='$FundName']/reporting:ucitsCommitmentCurr.Fund,'#.########','coerce')"/>
  </xsl:template>-->

<!--  <xsl:template name="Generate" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
    <xsl:variable name="Fund"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio"/>
    
      <xsl:for-each select="$Fund">
      <xsl:call-template name="Funds" xmlns="urn:schemas-microsoft-com:office:spreadsheet"/>    
    </xsl:for-each>
  
  </xsl:template> -->
  
  <xsl:template name="Funds" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
	<!--<xsl:param name="cbttCode"/>-->
   <!-- <xsl:if test="starts-with(name(), '1')"></xsl:if> -->

   <xsl:variable name="FundName"  select="./@reporting:name"/>
   <xsl:variable name="Type"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio/@reporting:name"/>   
   <xsl:variable name="commitmentStep1Gross" select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentStep1Gross,'###,###.00','coerce')"/>
   <xsl:variable name="commitmentStep2Net"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentStep2Net,'###,###.00','coerce')"/>
   <xsl:variable name="commitmentStep3NetHedge"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentStep3NetHedge,'###,###.00','coerce')"/>
   <xsl:variable name="commitmentStep3NetHedgeOfNav"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentStep3NetHedgeOfNav,'#.##','coerce')"/>     
   <xsl:variable name="commitmentUnderlyings"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentUnderlyings,'###,###.00','coerce')"/>
   <xsl:variable name="commitmentFxForwardsNetHedge"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentFxForwardsNetHedge,'###,###.00','coerce')"/>
   <!--<xsl:variable name="commitmentAllAssetsGrossWeight"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentAllAssetsGrossWeight,'#.########','coerce')"/>
   <xsl:variable name="commitmentAllAssetsNet"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentAllAssetsNet,'#.########','coerce')"/>
   <xsl:variable name="commitmentAllAssetsNetWeight"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentAllAssetsNetWeight,'#.########','coerce')"/>-->
   <xsl:variable name="rbcFundNav"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:rbcFundNav,'###,###.00','coerce')"/>
   <!--<xsl:variable name="allotment"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:allotment"/>-->
   <xsl:variable name="marketValue"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:marketValue,'###,###.00','coerce')"/>
   <xsl:variable name="marketValueCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:marketValueCurr.Global,'###,###.00','coerce')"/>
    <!--<xsl:variable name="nominal1stCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:nominal1stCcy,'#.########','coerce')"/>
   <xsl:variable name="nominal2ndCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:nominal2ndCcy,'#.########','coerce')"/>
   <xsl:variable name="currency"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:currency"/>-->
   <xsl:variable name="DV01"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:globalDv01,'###,###.00','coerce')"/>
   <xsl:variable name="DV01CurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:dv01Curr.Global,'###,###.00','coerce')"/>
   <xsl:variable name="EffectiveDuration"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:effectiveDuration,'###,###.00','coerce')"/>
   <xsl:variable name="MACDuration"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:mac.Duration,'###,###.00','coerce')"/>
   <xsl:variable name="DurationModified"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:durationModified,'###,###.00','coerce')"/>
   <Row>
    <!--<Cell>
      <Data ss:Type="Number">
      <xsl:value-of select="format-number(226825.8, '#,##0.00', 'test')"/>
      </Data>
    </Cell>-->
    <Cell></Cell>
    <Cell ss:StyleID="s69"><Data ss:Type="String"><xsl:value-of select="$FundName"/></Data></Cell>
    <Cell ss:StyleID="s69"></Cell>
        
     <!--<Cell><Data ss:Type="String"><xsl:value-of select="$Type"/></Data></Cell>-->
     
     <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$commitmentStep1Gross"/></Data></Cell>
     <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$commitmentStep2Net"/></Data></Cell>
     <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$commitmentStep3NetHedge"/></Data></Cell>     
     <Cell ss:StyleID="s69"><Data ss:Type="String"><xsl:value-of select="$commitmentStep3NetHedgeOfNav"/>%</Data></Cell>     
     <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$commitmentUnderlyings"/></Data></Cell>     
     <!--<Cell ss:StyleID="s69"><Data ss:Type="String"><xsl:value-of select="$commitmentFxForwardsNetHedge"/></Data></Cell>-->
     <!--<Cell><Data ss:Type="Number"><xsl:value-of select="$commitmentAllAssetsNet"/></Data></Cell>     
     <Cell><Data ss:Type="Number"><xsl:value-of select="$commitmentAllAssetsNetWeight"/></Data></Cell>   
     <Cell><Data ss:Type="Number"><xsl:value-of select="$ucitsCommitmentCurrFund"/></Data></Cell>-->
     <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$rbcFundNav"/></Data></Cell>          
<!--     <Cell><Data ss:Type="String"><xsl:value-of select="$allotment"/></Data></Cell>-->
     <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$marketValue"/></Data></Cell>     
     <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$marketValueCurrGlobal"/></Data></Cell>     
     <!--<Cell><Data ss:Type="Number"><xsl:value-of select="$nominal1stCcy"/></Data></Cell>     
     <Cell><Data ss:Type="Number"><xsl:value-of select="$nominal2ndCcy"/></Data></Cell>     
     <Cell><Data ss:Type="String"><xsl:value-of select="$currency"/></Data></Cell>     -->
     <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$DV01"/></Data></Cell>     
     <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$DV01CurrGlobal"/></Data></Cell>     
     <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$EffectiveDuration"/></Data></Cell>     
	 <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$MACDuration"/></Data></Cell>     
	 <Cell ss:StyleID="s69"><Data ss:Type="Number"><xsl:value-of select="$DurationModified"/></Data></Cell>     
</Row>

    <xsl:for-each select="$Type">
      <xsl:call-template name="Types" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
        <xsl:with-param name="FundName" select="$FundName"/>
        <xsl:with-param name="TypeName" select="$Type"/>
      </xsl:call-template>
    </xsl:for-each>
    

    
    
    
  </xsl:template>


  <xsl:template name="FundsPositions" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
    <!--<xsl:param name="cbttCode"/>-->
    <!-- <xsl:if test="starts-with(name(), '1')"></xsl:if> -->

    <xsl:variable name="FundName"  select="./@reporting:name"/>
    <xsl:variable name="Type"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio/@reporting:name"/>
    <xsl:variable name="commitmentStep1Gross"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentStep1Gross,'###,###.00','coerce')"/>
    <xsl:variable name="commitmentStep2Net"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentStep2Net,'###,###.00','coerce')"/>
    <xsl:variable name="commitmentStep3NetHedge"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentStep3NetHedge,'###,###.00','coerce')"/>
    <xsl:variable name="commitmentStep3NetHedgeOfNav"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentStep3NetHedgeOfNav,'#.##','coerce')"/>
    <xsl:variable name="commitmentUnderlyings"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentUnderlyings,'###,###.00','coerce')"/>
    <xsl:variable name="commitmentFxForwardsNetHedge"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentFxForwardsNetHedge,'###,###.00','coerce')"/>
    <!--<xsl:variable name="commitmentAllAssetsGrossWeight"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentAllAssetsGrossWeight,'#.########','coerce')"/>
   <xsl:variable name="commitmentAllAssetsNet"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentAllAssetsNet,'#.########','coerce')"/>
   <xsl:variable name="commitmentAllAssetsNetWeight"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentAllAssetsNetWeight,'#.########','coerce')"/>-->
    <xsl:variable name="rbcFundNav"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:rbcFundNav,'###,###.00','coerce')"/>
    <!--<xsl:variable name="allotment"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:allotment"/>-->
    <xsl:variable name="marketValue"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:marketValue,'###,###.00','coerce')"/>
    <xsl:variable name="marketValueCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:marketValueCurr.Global,'###,###.00','coerce')"/>
    <!--<xsl:variable name="nominal1stCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:nominal1stCcy,'#.########','coerce')"/>
   <xsl:variable name="nominal2ndCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:nominal2ndCcy,'#.########','coerce')"/>
   <xsl:variable name="currency"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:currency"/>-->
   <xsl:variable name="DV01"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:globalDv01,'###,###.00','coerce')"/>
   <xsl:variable name="DV01CurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:dv01Curr.Global,'###,###.00','coerce')"/>
   <xsl:variable name="EffectiveDuration"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:effectiveDuration,'###,###.00','coerce')"/>
   <xsl:variable name="MACDuration"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:mac.Duration,'###,###.00','coerce')"/>
   <xsl:variable name="DurationModified"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:durationModified,'###,###.00','coerce')"/>
   
      

    <xsl:for-each select="$Type">
      <xsl:call-template name="TypesPositions" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
        <xsl:with-param name="FundName" select="$FundName"/>
        <xsl:with-param name="TypeName" select="$Type"/>
      </xsl:call-template>
    </xsl:for-each>





  </xsl:template>
  
  <xsl:template name="Types" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
	  <xsl:param name="FundName"/>
    <xsl:param name="TypeName"/>
   <!-- <xsl:if test="starts-with(name(), '1')"></xsl:if> -->

   
   <xsl:variable name="Type"  select="."/>
   <!--<xsl:variable name="TypeName"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/@reporting:name"/>-->
   <!--<xsl:variable name="ucitsCommitmentCurrFund"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:ucitsCommitmentCurr.Fund,'#.########','coerce')"/>-->
   <xsl:variable name="commitmentStep1Gross"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentStep1Gross,'###,###.00','coerce')"/>
   <xsl:variable name="commitmentStep2Net"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentStep2Net,'###,###.00','coerce')"/>
   <xsl:variable name="commitmentStep3NetHedge"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentStep3NetHedge,'###,###.00','coerce')"/>
   <xsl:variable name="commitmentStep3NetHedgeOfNav"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentStep3NetHedgeOfNav,'#.##','coerce')"/>     
   <xsl:variable name="commitmentUnderlyings"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentUnderlyings,'###,###.00','coerce')"/>
   <xsl:variable name="commitmentFxForwardsNetHedge"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentFxForwardsNetHedge,'###,###.00','coerce')"/>
   <!--<xsl:variable name="commitmentAllAssetsNet"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentAllAssetsNet,'#.########','coerce')"/>
   <xsl:variable name="commitmentAllAssetsNetWeight"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentAllAssetsNetWeight,'#.########','coerce')"/>-->
   <xsl:variable name="rbcFundNav"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:rbcFundNav,'###,###.00','coerce')"/>
   <!--<xsl:variable name="allotment"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:line/reporting:allotment"/>-->
   <xsl:variable name="marketValue"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:marketValue,'###,###.00','coerce')"/>
   <xsl:variable name="marketValueCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:marketValueCurr.Global,'###,###.00','coerce')"/>
   <!--<xsl:variable name="nominal1stCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:nominal1stCcy,'#.########','coerce')"/>
   <xsl:variable name="nominal2ndCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:nominal2ndCcy,'#.########','coerce')"/>
   <xsl:variable name="currency"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:currency"/>-->
   <xsl:variable name="DV01"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:globalDv01,'###,###.00','coerce')"/>
   <xsl:variable name="DV01CurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:dv01Curr.Global,'###,###.00','coerce')"/>
   <xsl:variable name="EffectiveDuration"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:effectiveDuration,'###,###.00','coerce')"/>
   <xsl:variable name="MACDuration"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:mac.Duration,'###,###.00','coerce')"/>
   <xsl:variable name="DurationModified"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:durationModified,'###,###.00','coerce')"/>
    
   <Row>
    <Cell></Cell>
    <Cell></Cell>
    <Cell><Data ss:Type="String"><xsl:value-of select="$Type"/></Data></Cell>
    <!--<Cell><Data ss:Type="Number"><xsl:value-of select="$ucitsCommitmentCurrFund"/></Data></Cell>-->     
     <Cell><Data ss:Type="Number"><xsl:value-of select="$commitmentStep1Gross"/></Data></Cell>     
     <Cell><Data ss:Type="Number"><xsl:value-of select="$commitmentStep2Net"/></Data></Cell>
     <xsl:choose>
       <xsl:when test="$Type='FX FORWARD'">
         <Cell>           
           <Data ss:Type="Number">
             <xsl:value-of select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:commitmentFxForwardsNetHedge,'###,###.00','coerce')"/>
           </Data>
         </Cell>
       </xsl:when>
       <xsl:otherwise>
         <Cell>
           <Data ss:Type="Number">
             <xsl:value-of select="$commitmentStep3NetHedge"/>
           </Data>
         </Cell>
       </xsl:otherwise>
     </xsl:choose>
     
     <!--<Cell><Data ss:Type="Number"><xsl:value-of select="$commitmentStep3NetHedge"/></Data></Cell>-->     
     <Cell><Data ss:Type="Number"><xsl:value-of select="$commitmentStep3NetHedgeOfNav"/></Data></Cell>     
     <Cell><Data ss:Type="Number"><xsl:value-of select="$commitmentUnderlyings"/></Data></Cell>     
     
     <!--<Cell><Data ss:Type="String"><xsl:value-of select="$commitmentFxForwardsNetHedge"/></Data></Cell>-->
     <!--<Cell><Data ss:Type="Number"><xsl:value-of select="$commitmentAllAssetsNet"/></Data></Cell>     
     <Cell><Data ss:Type="Number"><xsl:value-of select="$commitmentAllAssetsNetWeight"/></Data></Cell> 
     <Cell></Cell>-->
     <Cell><Data ss:Type="Number"><xsl:value-of select="$rbcFundNav"/></Data></Cell>          
     <!--<Cell><Data ss:Type="String"><xsl:value-of select="$allotment"/></Data></Cell>-->
     <Cell><Data ss:Type="Number"><xsl:value-of select="$marketValue"/></Data></Cell>     
     <Cell><Data ss:Type="Number"><xsl:value-of select="$marketValueCurrGlobal"/></Data></Cell>     
     <!--<Cell><Data ss:Type="Number"><xsl:value-of select="$nominal1stCcy"/></Data></Cell>     
     <Cell><Data ss:Type="Number"><xsl:value-of select="$nominal2ndCcy"/></Data></Cell>     
     <Cell><Data ss:Type="String"><xsl:value-of select="$currency"/></Data></Cell>-->
     <Cell><Data ss:Type="Number"><xsl:value-of select="$DV01"/></Data></Cell>  
     <Cell><Data ss:Type="Number"><xsl:value-of select="$DV01CurrGlobal"/></Data></Cell>  
     <Cell><Data ss:Type="Number"><xsl:value-of select="$EffectiveDuration"/></Data></Cell>  
	 <Cell><Data ss:Type="Number"><xsl:value-of select="$MACDuration"/></Data></Cell> 
	 <Cell><Data ss:Type="Number"><xsl:value-of select="$DurationModified"/></Data></Cell> 
</Row>
      
      
  </xsl:template>

  <xsl:template name="TypesPositions" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
    <xsl:param name="FundName"/>
    <xsl:param name="TypeName"/>
    <!-- <xsl:if test="starts-with(name(), '1')"></xsl:if> -->


    <xsl:variable name="Type"  select="."/>
    <xsl:variable name="Position"  select="../reporting:line"/>
    <!--<xsl:variable name="TypeName"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/@reporting:name"/>-->
    <!--<xsl:variable name="ucitsCommitmentCurrFund"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:ucitsCommitmentCurr.Fund,'#.########','coerce')"/>-->
    <xsl:variable name="commitmentStep1Gross"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentStep1Gross,'###,###.00','coerce')"/>
    <xsl:variable name="commitmentStep2Net"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentStep2Net,'###,###.00','coerce')"/>
    <xsl:variable name="commitmentStep3NetHedge"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentStep3NetHedge,'###,###.00','coerce')"/>
    <xsl:variable name="commitmentStep3NetHedgeOfNav"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentStep3NetHedgeOfNav,'#.##','coerce')"/>
    <xsl:variable name="commitmentUnderlyings"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentUnderlyings,'###,###.00','coerce')"/>
    <xsl:variable name="commitmentFxForwardsNetHedge"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/commitmentFxForwardsNetHedge,'###,###.00','coerce')"/>
    <!--<xsl:variable name="commitmentAllAssetsNet"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentAllAssetsNet,'#.########','coerce')"/>
   <xsl:variable name="commitmentAllAssetsNetWeight"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentAllAssetsNetWeight,'#.########','coerce')"/>-->
    <xsl:variable name="rbcFundNav"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:rbcFundNav,'###,###.00','coerce')"/>
    <!--<xsl:variable name="allotment"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:line/reporting:allotment"/>-->
    <xsl:variable name="marketValue"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:marketValue,'###,###.00','coerce')"/>
    <xsl:variable name="marketValueCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:marketValueCurr.Global,'###,###.00','coerce')"/>
    <!--<xsl:variable name="nominal1stCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:nominal1stCcy,'#.########','coerce')"/>
   <xsl:variable name="nominal2ndCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:nominal2ndCcy,'#.########','coerce')"/>
   <xsl:variable name="currency"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:currency"/>-->
   <xsl:variable name="DV01"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:globalDv01,'###,###.00','coerce')"/>
   <xsl:variable name="DV01CurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:dv01Curr.Global,'###,###.00','coerce')"/>
   <xsl:variable name="EffectiveDuration"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:effectiveDuration,'###,###.00','coerce')"/>
   <xsl:variable name="MACDuration"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:MACDuration,'###,###.00','coerce')"/>
   <xsl:variable name="DurationModified"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:durationModified,'###,###.00','coerce')"/>

 

    <xsl:for-each select="$Position">
      <xsl:call-template name="Positions" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
        <xsl:with-param name="FundName" select="$FundName"/>
        <xsl:with-param name="TypeName" select="$Type"/>
        <xsl:with-param name="Position" select="."/>
      </xsl:call-template>
    </xsl:for-each>

  </xsl:template>

  <xsl:template name="Positions" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
    <xsl:param name="FundName"/>
    <xsl:param name="TypeName"/>
    <xsl:param name="Position"/>

    <xsl:variable name="PositionName"  select="$Position/@reporting:name"/>
    <xsl:variable name="commitmentStep1Gross"  select="format-number($Position/reporting:commitmentStep1Gross,'###,###.00','coerce')"/>
    <xsl:variable name="commitmentUnderlyings"  select="format-number($Position/reporting:commitmentUnderlyings,'###,###.00','coerce')"/>
    <xsl:variable name="NrOfSecurities"  select="format-number($Position/reporting:numberOfSecurities,'###,###.00','coerce')"/>
    <xsl:variable name="stCCY"  select="$Position/reporting:stCcy"/>
    <xsl:variable name="Nominal1stCCY"  select="format-number($Position/reporting:nominal1stCcy,'###,###.00','coerce')"/>
    <xsl:variable name="Currency"  select="$Position/reporting:currency/text()"/>
    <xsl:variable name="Nominal2ndCCY"  select="format-number($Position/reporting:nominal2ndCcy,'###,###.00','coerce')"/>
    <!--<xsl:variable name="commitmentAllAssetsNet"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentAllAssetsNet,'#.########','coerce')"/>
   <xsl:variable name="commitmentAllAssetsNetWeight"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentAllAssetsNetWeight,'#.########','coerce')"/>-->
    <xsl:variable name="rbcFundNav"  select="format-number($Position/reporting:rbcFundNav,'###,###.00','coerce')"/>
    <!--<xsl:variable name="allotment"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:line/reporting:allotment"/>-->
    <!-- <xsl:variable name="marketvalue"  select="format-number($position/reporting:line[@reporting:name=$positionname]/reporting:marketvalue,'###,###.00','coerce')"/> -->
	<xsl:variable name="marketValue"  select="format-number($Position/reporting:marketValue,'###,###.00','coerce')"/>
    <xsl:variable name="marketValueCurrGlobal"  select="format-number($Position/reporting:marketValueCurr.Global,'###,###.00','coerce')"/>
    <xsl:variable name="instrumentReference"  select="$Position/reporting:instrumentReference"/>
    <xsl:variable name="DV01"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$TypeName]/reporting:line[@reporting:name=$PositionName]/reporting:globalDv01,'###,###.00','coerce')"/>
    <xsl:variable name="DV01CurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$TypeName]/reporting:line[@reporting:name=$PositionName]/reporting:dv01Curr.Global,'###,###.00','coerce')"/>
    <xsl:variable name="EffectiveDuration"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$TypeName]/reporting:line[@reporting:name=$PositionName]/reporting:effectiveDuration,'###,###.00','coerce')"/>    
	<xsl:variable name="MACDuration"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$TypeName]/reporting:line[@reporting:name=$PositionName]/reporting:mac.Duration,'###,###.00','coerce')"/>    
	<xsl:variable name="DurationModified"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$TypeName]/reporting:line[@reporting:name=$PositionName]/reporting:durationModified,'###,###.00','coerce')"/>    
    <Row>
      <Cell/>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$FundName"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$TypeName"/>
        </Data>
      </Cell>            
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$Position/@reporting:name"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$instrumentReference"/>
        </Data>
      </Cell>
      <!--<Cell><Data ss:Type="Number"><xsl:value-of select="$ucitsCommitmentCurrFund"/></Data></Cell>-->
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$commitmentStep1Gross"/>
        </Data>
      </Cell>      
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$commitmentUnderlyings"/>
        </Data>
      </Cell>
      
      <!--<Cell><Data ss:Type="Number"><xsl:value-of select="$commitmentAllAssetsNet"/></Data></Cell>     
     <Cell><Data ss:Type="Number"><xsl:value-of select="$commitmentAllAssetsNetWeight"/></Data></Cell> 
     <Cell></Cell>-->
      
      <!--<Cell><Data ss:Type="String"><xsl:value-of select="$allotment"/></Data></Cell>-->
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$marketValue"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$marketValueCurrGlobal"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$NrOfSecurities"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$stCCY"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$Nominal1stCCY"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$Currency"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$Nominal2ndCCY"/>
        </Data>
      </Cell>
      <!--<Cell><Data ss:Type="Number"><xsl:value-of select="$nominal1stCcy"/></Data></Cell>     
     <Cell><Data ss:Type="Number"><xsl:value-of select="$nominal2ndCcy"/></Data></Cell>     
     <Cell><Data ss:Type="String"><xsl:value-of select="$currency"/></Data></Cell>-->
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$DV01"/>
        </Data>
      </Cell>
	  <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$DV01CurrGlobal"/>
        </Data>
      </Cell>
	  <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$EffectiveDuration"/>
        </Data>
      </Cell>
	  <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$MACDuration"/>
        </Data>
      </Cell>
	  <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$DurationModified"/>
        </Data>
      </Cell>
</Row>

  </xsl:template>

 <xsl:template match="/">
   <xsl:processing-instruction name="mso-application">progid="Excel.Sheet"</xsl:processing-instruction>

<Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet"
         xmlns:o="urn:schemas-microsoft-com:office:office"
         xmlns:x="urn:schemas-microsoft-com:office:excel"
         xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
         xmlns:html="http://www.w3.org/TR/REC-html40">
  <Styles>   
    <Style ss:ID="s67">
   <Borders>
     <Border ss:Position="Top" ss:LineStyle="Continuous" ss:Weight="2"/>
     <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="2"/>
     <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="2"/>
     <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="2"/>
   </Borders>
   <Interior ss:Color="#CCFFFF" ss:Pattern="Solid"/>
   <Protection/>
  </Style>
    <Style ss:ID="s68">
   <Borders>
     <Border ss:Position="Top" ss:LineStyle="Continuous" ss:Weight="2"/>
     <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="2"/>
     <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="2"/>
     <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="2"/>
   </Borders>
   <Interior  ss:Color="#BBCCCC" ss:Pattern="Solid"/>
   <Protection/>
  </Style>
    <Style ss:ID="s69">
      <Borders>
        <Border ss:Position="Top" ss:LineStyle="Continuous" ss:Weight="2"/>
        <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="2"/>
        <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="2"/>
        <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="2"/>
      </Borders>
      <Font ss:Bold="1"/>
      <Interior  ss:Color="#BBFCCF" ss:Pattern="Solid"/>
      <Protection/>
    </Style>
  
  </Styles>
    
  <Worksheet ss:Name="Funds">
    
    <Table ss:ExpandedColumnCount="20" x:FullColumns="1"
               x:FullRows="1" ss:DefaultRowHeight="15">
      <Column ss:AutoFitWidth="0" ss:Width="80"/>
      <Column ss:AutoFitWidth="1" ss:Width="100"/>
      <Column ss:AutoFitWidth="1" ss:Width="100"/>
      <Column ss:AutoFitWidth="0" ss:Width="180"/>
      <Column ss:AutoFitWidth="0" ss:Width="165"/>
      <Column ss:AutoFitWidth="1" ss:Width="180"/>
      <Column ss:AutoFitWidth="0" ss:Width="190"/>
      <Column ss:AutoFitWidth="0" ss:Width="165"/>
      <Column ss:AutoFitWidth="0" ss:Width="190"/>
      <Column ss:AutoFitWidth="0" ss:Width="180"/>
      <Column ss:AutoFitWidth="0" ss:Width="180"/>
      <Column ss:AutoFitWidth="0" ss:Width="180"/>
      <Column ss:AutoFitWidth="0" ss:Width="100"/>
      <Column ss:AutoFitWidth="0" ss:Width="100"/>
      <Column ss:AutoFitWidth="0" ss:Width="150"/>
      <Column ss:AutoFitWidth="0" ss:Width="100"/>
      <Column ss:AutoFitWidth="0" ss:Width="100"/>
      <Column ss:AutoFitWidth="0" ss:Width="100"/>


      <Row>
        <Cell>
          <Data ss:Type="String"><xsl:value-of select="/reporting:root/reporting:default0/reporting:header/reporting:resultOn"/></Data>
        </Cell>
      </Row>
      <Row>
        <Cell>
          <Data ss:Type="String"><xsl:value-of select="/reporting:root/reporting:default0/reporting:header/reporting:currency"/></Data>
        </Cell>
      </Row>
      <Row ss:Height="30">
        <Cell></Cell>
        <Cell ss:StyleID="s68"><Data ss:Type="String">Funds</Data></Cell>
        <Cell ss:StyleID="s68"><Data ss:Type="String">Type</Data></Cell>        
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Commitment - Step 1 (gross)</Data></Cell>
        <Cell ss:StyleID="s67"><Data ss:Type="String">Commitment - Step 2 (net)</Data></Cell>        
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Commitment - Step 3 (net/hedge)</Data></Cell>
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Commitment - Step 3 (net/hedge) % of NAV</Data></Cell>
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Commitment - Underlyings</Data></Cell>
        <!--<Cell ss:StyleID="s67" ><Data ss:Type="String">Commitment – FX Forwards (net/hedge) </Data></Cell>-->
        <!--<Cell ss:StyleID="s67" ><Data ss:Type="String">Commitment - All assets (net)</Data></Cell>
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Commitment - All assets (net weight)</Data></Cell>
        <Cell ss:StyleID="s67"><Data ss:Type="String">UCITS Commitment curr. fund</Data></Cell>-->
        <Cell ss:StyleID="s67" ><Data ss:Type="String">RBC Fund NAV </Data></Cell>
        <!--<Cell ss:StyleID="s67" ><Data ss:Type="String">Allotment</Data></Cell>-->
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Market Value</Data></Cell>
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Market Value curr. global</Data></Cell>
        <!--<Cell ss:StyleID="s67" ><Data ss:Type="String">Nominal 1st CCY</Data></Cell>
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Nominal 2nd CCY</Data></Cell>
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Currency</Data></Cell>      -->
        <Cell ss:StyleID="s67" ><Data ss:Type="String">DV01</Data></Cell>
	<Cell ss:StyleID="s67" ><Data ss:Type="String">DV01 curr. Global</Data></Cell>
	<Cell ss:StyleID="s67" ><Data ss:Type="String">Effective duration</Data></Cell>
	<Cell ss:StyleID="s67" ><Data ss:Type="String">Mac. duration</Data></Cell>
	<Cell ss:StyleID="s67" ><Data ss:Type="String">Duration (modified)</Data></Cell>
      </Row>
      
      <xsl:variable name="Fund"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio"/>
        
      <xsl:for-each select="$Fund">
        <xsl:sort order="descending" select="number(./reporting:commitmentStep3NetHedgeOfNav)" data-type="number"/>
        <xsl:call-template name="Funds" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
        </xsl:call-template>
      </xsl:for-each>
        
      
      </Table>
          
 
      </Worksheet>

  <Worksheet ss:Name="Positions">

    <Table ss:ExpandedColumnCount="20" x:FullColumns="1"
               x:FullRows="1" ss:DefaultRowHeight="15">
      <Column ss:AutoFitWidth="0" ss:Width="80"/>
      <Column ss:AutoFitWidth="1" ss:Width="100"/>
      <Column ss:AutoFitWidth="1" ss:Width="100"/>
      <Column ss:AutoFitWidth="0" ss:Width="180"/>
      <Column ss:AutoFitWidth="0" ss:Width="165"/>
      <Column ss:AutoFitWidth="1" ss:Width="180"/>
      <Column ss:AutoFitWidth="0" ss:Width="190"/>
      <Column ss:AutoFitWidth="0" ss:Width="165"/>
      <Column ss:AutoFitWidth="0" ss:Width="190"/>
      <Column ss:AutoFitWidth="0" ss:Width="180"/>
      <Column ss:AutoFitWidth="0" ss:Width="180"/>
      <Column ss:AutoFitWidth="0" ss:Width="180"/>
      <Column ss:AutoFitWidth="0" ss:Width="180"/>
      <Column ss:AutoFitWidth="0" ss:Width="100"/>
      <Column ss:AutoFitWidth="0" ss:Width="150"/>
      <Column ss:AutoFitWidth="0" ss:Width="100"/>
      <Column ss:AutoFitWidth="0" ss:Width="100"/>
      <Column ss:AutoFitWidth="0" ss:Width="100"/>
      <Column ss:AutoFitWidth="0" ss:Width="100"/>


      <Row></Row>
      <Row></Row>
      <Row ss:Height="30">
        <Cell></Cell>
        <Cell ss:StyleID="s68">
          <Data ss:Type="String">Funds</Data>
        </Cell>
        <Cell ss:StyleID="s68">
          <Data ss:Type="String">Allotment</Data>
        </Cell>
        <Cell ss:StyleID="s68">
          <Data ss:Type="String">Position</Data>
        </Cell>
        <Cell ss:StyleID="s68">
          <Data ss:Type="String">Instrument reference</Data>
        </Cell>
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">Commitment - Step 1 (gross)</Data>
        </Cell>        
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">Commitment - Underlyings</Data>
        </Cell>
        
        <!--<Cell ss:StyleID="s67" >
          <Data ss:Type="String">Commitment – FX Forwards (net/hedge) </Data>
        </Cell>-->
        <!--<Cell ss:StyleID="s67" ><Data ss:Type="String">Commitment - All assets (net)</Data></Cell>
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Commitment - All assets (net weight)</Data></Cell>
        <Cell ss:StyleID="s67"><Data ss:Type="String">UCITS Commitment curr. fund</Data></Cell>-->
        <!--<Cell ss:StyleID="s67" >
          <Data ss:Type="String">RBC Fund NAV </Data>
        </Cell>-->
        <!--<Cell ss:StyleID="s67" ><Data ss:Type="String">Allotment</Data></Cell>-->
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">Market Value</Data>
        </Cell>
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">Market Value curr. global</Data>
        </Cell>
        <Cell ss:StyleID="s67">
          <Data ss:Type="String">Number of securities</Data>
        </Cell>
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">1st CCY</Data>
        </Cell>
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">Nominal 1st CCY</Data>
        </Cell>
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">Currency</Data>
        </Cell>
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">Nominal 2nd CCY</Data>
        </Cell>
        <!--<Cell ss:StyleID="s67" ><Data ss:Type="String">Nominal 1st CCY</Data></Cell>
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Nominal 2nd CCY</Data></Cell>
        <Cell ss:StyleID="s67" ><Data ss:Type="String">Currency</Data></Cell>      -->
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">DV01</Data>
        </Cell>
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">DV01 curr. Global</Data>
        </Cell>
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">Effective duration</Data>
        </Cell>
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">Mac. duration</Data>
        </Cell>
        <Cell ss:StyleID="s67" >
          <Data ss:Type="String">Duration (modified)</Data>
        </Cell>
      </Row>

      <xsl:variable name="Fund"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio"/>
      <xsl:for-each select="$Fund">        
        <xsl:call-template name="FundsPositions" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
        </xsl:call-template>
      </xsl:for-each>


    </Table>


  </Worksheet>
</Workbook>
   </xsl:template>
        

</xsl:stylesheet>