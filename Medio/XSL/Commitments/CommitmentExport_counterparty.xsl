<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:x="urn:schemas-microsoft-com:office:excel"
  xmlns:reporting="http://www.sophis.net/reporting"
  xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:ms="urn:schemas-microsoft-com:xslt"
    xmlns:o="urn:schemas-microsoft-com:office:office"
    xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
    xmlns:html="http://www.w3.org/TR/REC-html40">
  <xsl:output method="xml" encoding="UTF-8" indent="yes"/>
  <xsl:decimal-format name="coerce" NaN="0" infinity="0"  decimal-separator='.' grouping-separator=',' />
  <xsl:decimal-format name="coerce1" NaN="0" infinity="0"/>

 

  <xsl:template name="Funds" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">

    <xsl:variable name="FundName"  select="./@reporting:name"/>
    <xsl:variable name="Counterparty"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio/reporting:cptyRiskOfNav[number(text())>0]/../@reporting:name"/>                  
    <xsl:variable name="CptyRiskOfNAV"  select="format-number(sum(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio/reporting:cptyRiskOfNav[number(text())>0]), '#0.##','coerce1')"/>
    <xsl:variable name="RBCFundNAV"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:rbcFundNav,'#.########','coerce')"/>
    <xsl:variable name="ResultCurrGlobal"  select="format-number(sum(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio/reporting:cptyRiskOfNav[number(text())>0]/../reporting:resultCurr.Global),'#.########','coerce')"/>
    <xsl:variable name="UnrealizedCurrGlobal"  select="format-number(sum(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio/reporting:cptyRiskOfNav[number(text())>0]/../reporting:unrealizedCurr.Global),'#.########','coerce')"/>
  
    <Row>
      <Cell></Cell>
      <Cell ss:StyleID="s69">
        <Data ss:Type="String">
          <xsl:value-of select="$FundName"/>
        </Data>
      </Cell>
      <Cell ss:StyleID="s69"></Cell>

      <!--<Cell><Data ss:Type="String"><xsl:value-of select="$Type"/></Data></Cell>-->

      <Cell ss:StyleID="s69">
        <Data ss:Type="String">
          <xsl:value-of select="$CptyRiskOfNAV"/>%
        </Data>
      </Cell>
      <Cell ss:StyleID="s69">
        <Data ss:Type="Number">
          <xsl:value-of select="$RBCFundNAV"/>
        </Data>
      </Cell>
      <!--<Cell ss:StyleID="s69">
        <Data ss:Type="Number">
          <xsl:value-of select="$ResultCurrGlobal"/>
        </Data>
      </Cell>-->
      <Cell ss:StyleID="s69">
        <Data ss:Type="Number">
          <xsl:value-of select="$UnrealizedCurrGlobal"/>
        </Data>
      </Cell>     
    </Row>

    <xsl:for-each select="$Counterparty">
      <xsl:sort order="descending" select="number(../reporting:cptyRiskOfNav[number(text())>0])" data-type="number"/>
      <xsl:call-template name="Counterparty" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
        <xsl:with-param name="FundName" select="$FundName"/>
        <xsl:with-param name="CounterpartyName" select="$Counterparty"/>
      </xsl:call-template>
    </xsl:for-each>





  </xsl:template>


  <xsl:template name="FundsPositions" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
    

    <xsl:variable name="FundName"  select="./@reporting:name"/>
    <xsl:variable name="CounterpartyGroup"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio/reporting:cptyRiskOfNav[number(text())>0]/../@reporting:name"/>
    <xsl:variable name="Counterparty"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio/reporting:cptyRiskOfNav[number(text())>0]/../reporting:folio/@reporting:name"/>
    <xsl:variable name="CptyRiskOfNAV"  select="format-number(sum(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio/reporting:cptyRiskOfNav[number(text())>0]),'#0.##','coerce1')"/>
    <xsl:variable name="RBCFundNAV"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:rbcFundNav,'#.########','coerce')"/>
    <xsl:variable name="ResultCurrGlobal"  select="format-number(sum(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio/reporting:cptyRiskOfNav[number(text())>0]/../reporting:resultCurr.Global),'###,###.00','coerce')"/>
    <xsl:variable name="UnrealizedCurrGlobal"  select="format-number(sum(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio/reporting:cptyRiskOfNav[number(text())>0]/../unrealizedCurr.Global),'###,###.00','coerce')"/>
 

    <xsl:for-each select="$CounterpartyGroup">     
      <!--<xsl:sort order="descending" select="../reporting:cptyRiskOfNav[number(text())>0]"/>-->
      <xsl:call-template name="CounterpartyGroupPositions" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
        <xsl:with-param name="FundName" select="$FundName"/>
        <xsl:with-param name="CounterpartyGroup" select="$CounterpartyGroup"/>
      </xsl:call-template>
    </xsl:for-each>





  </xsl:template>


  <xsl:template name="CounterpartyGroupPositions" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
    <xsl:param name="FundName"/>
    <xsl:param name="CounterpartyGroup"/>
    <!-- <xsl:if test="starts-with(name(), '1')"></xsl:if> -->


    <xsl:variable name="Counterparty"  select="$CounterpartyGroup/../reporting:folio"/>
    <xsl:variable name="CounterpartyName"  select="$CounterpartyGroup/../reporting:folio/@reporting:name"/>
    
    <xsl:variable name="Position"  select="$Counterparty/reporting:folio/reporting:line"/>
    <!--<xsl:variable name="TypeName"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/@reporting:name"/>-->
    <!--<xsl:variable name="ucitsCommitmentCurrFund"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:ucitsCommitmentCurr.Fund,'#.########','coerce')"/>-->
    <xsl:variable name="CptyRiskOfNAV"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio[@reporting:name=$CounterpartyName]/reporting:cptyRiskOfNav,'#0.##','coerce1')"/>
    <!--<xsl:variable name="RBCFundNAV"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio/reporting:folio[@reporting:name=$Counterparty]/reporting:cptyRiskOfNav,'#.########','coerce')"/>-->
    <xsl:variable name="ResultCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio[@reporting:name=$CounterpartyName]/reporting:resultCurr.Global,'###,###.00','coerce')"/>
    <xsl:variable name="UnrealizedCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio[@reporting:name=$CounterpartyName]/reporting:unrealizedCurr.Global,'###,###.00','coerce')"/>
    <!--<xsl:variable name="commitmentUnderlyings"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Counterparty]/reporting:commitmentUnderlyings,'#.########','coerce')"/>
    <xsl:variable name="commitmentFxForwardsNetHedge"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Counterparty]/commitmentFxForwardsNetHedge,'#.########','coerce')"/>-->
    <!--<xsl:variable name="commitmentAllAssetsNet"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentAllAssetsNet,'#.########','coerce')"/>
   <xsl:variable name="commitmentAllAssetsNetWeight"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentAllAssetsNetWeight,'#.########','coerce')"/>-->
    <!--<xsl:variable name="rbcFundNav"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Counterparty]/reporting:rbcFundNav,'#.########','coerce')"/>-->
    <!--<xsl:variable name="allotment"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:line/reporting:allotment"/>-->
    <!--<xsl:variable name="marketValue"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Counterparty]/reporting:marketValue,'#.########','coerce')"/>
    <xsl:variable name="marketValueCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Counterparty]/reporting:marketValueCurr.Global,'#.########','coerce')"/>-->
    <!--<xsl:variable name="nominal1stCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:nominal1stCcy,'#.########','coerce')"/>
   <xsl:variable name="nominal2ndCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:nominal2ndCcy,'#.########','coerce')"/>
   <xsl:variable name="currency"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:currency"/>-->

   

    <xsl:for-each select="$Counterparty">    
      <xsl:call-template name="CounterpartyPositions" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
        <xsl:with-param name="FundName" select="$FundName"/>
        <xsl:with-param name="CounterpartyGroup" select="$CounterpartyGroup"/>
        <xsl:with-param name="CounterpartyName" select="$CounterpartyName"/>
      </xsl:call-template>
    </xsl:for-each>



  </xsl:template>
  

  <xsl:template name="Counterparty" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
    <xsl:param name="FundName"/>
    <xsl:param name="CounterpartyName"/>
    <!-- <xsl:if test="starts-with(name(), '1')"></xsl:if> -->

    

    <xsl:variable name="Counterparty"  select="."/>    
    <!--<xsl:variable name="TypeName"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/@reporting:name"/>-->
    <!--<xsl:variable name="ucitsCommitmentCurrFund"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:ucitsCommitmentCurr.Fund,'#.########','coerce')"/>-->
    <xsl:variable name="CptyRiskOfNAV"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio[@reporting:name=$Counterparty]/reporting:cptyRiskOfNav,'#0.##','coerce1')"/>
    <!--<xsl:variable name="RBCFundNAV"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio/reporting:folio[@reporting:name=$Counterparty]/reporting:cptyRiskOfNav,'#.########','coerce')"/>-->
    <xsl:variable name="ResultCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio[@reporting:name=$Counterparty]/reporting:resultCurr.Global,'###,###.00','coerce')"/>
    <xsl:variable name="UnrealizedCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio[@reporting:name=$Counterparty]/reporting:unrealizedCurr.Global,'###,###.00','coerce')"/>
    <!--<xsl:variable name="commitmentUnderlyings"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Counterparty]/reporting:commitmentUnderlyings,'#.########','coerce')"/>
    <xsl:variable name="commitmentFxForwardsNetHedge"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Counterparty]/commitmentFxForwardsNetHedge,'#.########','coerce')"/>-->
    <!--<xsl:variable name="commitmentAllAssetsNet"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentAllAssetsNet,'#.########','coerce')"/>
   <xsl:variable name="commitmentAllAssetsNetWeight"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:commitmentAllAssetsNetWeight,'#.########','coerce')"/>-->
    <!--<xsl:variable name="rbcFundNav"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Counterparty]/reporting:rbcFundNav,'#.########','coerce')"/>-->
    <!--<xsl:variable name="allotment"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:line/reporting:allotment"/>-->
    <!--<xsl:variable name="marketValue"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Counterparty]/reporting:marketValue,'#.########','coerce')"/>
    <xsl:variable name="marketValueCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Counterparty]/reporting:marketValueCurr.Global,'#.########','coerce')"/>-->
    <!--<xsl:variable name="nominal1stCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:nominal1stCcy,'#.########','coerce')"/>
   <xsl:variable name="nominal2ndCcy"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:nominal2ndCcy,'#.########','coerce')"/>
   <xsl:variable name="currency"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:currency"/>-->


    <Row>
      <Cell></Cell>
      <Cell></Cell>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$Counterparty"/>
        </Data>
      </Cell>
      <!--<Cell><Data ss:Type="Number"><xsl:value-of select="$ucitsCommitmentCurrFund"/></Data></Cell>-->
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$CptyRiskOfNAV"/>%
        </Data>
      </Cell>
      <Cell></Cell>
      <!--<Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$ResultCurrGlobal"/>
        </Data>
      </Cell>-->
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$UnrealizedCurrGlobal"/>
        </Data>
      </Cell>     
    </Row>


  </xsl:template>

  <xsl:template name="CounterpartyPositions" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
    <xsl:param name="FundName"/>
    <xsl:param name="CounterpartyGroup"/>
    <xsl:param name="CounterpartyName"/>
    <!-- <xsl:if test="starts-with(name(), '1')"></xsl:if> -->



    <!--<xsl:variable name="CounterpartyGroup"  select="."/>-->
    
    <xsl:variable name="Counterparty"  select="./reporting:folio"/>
    <!--<xsl:variable name="Position"  select="$Counterparty/reporting:folio/reporting:line"/>-->

    <xsl:variable name="Position"  select="$Counterparty/reporting:line"/>
    
    <!--<xsl:variable name="TypeName"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/@reporting:name"/>-->
    <!--<xsl:variable name="ucitsCommitmentCurrFund"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:ucitsCommitmentCurr.Fund,'#.########','coerce')"/>-->
    <xsl:variable name="CptyRiskOfNAV"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio[@reporting:name=$CounterpartyName]/reporting:cptyRiskOfNav,'#0.##','coerce1')"/>
    <!--<xsl:variable name="RBCFundNAV"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio/reporting:folio[@reporting:name=$Counterparty]/reporting:cptyRiskOfNav,'#.########','coerce')"/>-->
    <xsl:variable name="ResultCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio[@reporting:name=$CounterpartyName]/reporting:resultCurr.Global,'###,###.00','coerce')"/>
    <xsl:variable name="UnrealizedCurrGlobal"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio[@reporting:name=$CounterpartyName]/reporting:unrealizedCurr.Global,'###,###.00','coerce')"/>


    <xsl:for-each select="$Position">      
      <xsl:call-template name="Positions" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
        <xsl:with-param name="FundName" select="$FundName"/>
        <xsl:with-param name="CounterpartyGroup" select="$CounterpartyGroup"/>
        <xsl:with-param name="Position" select="."/>
      </xsl:call-template>
    </xsl:for-each>

  </xsl:template>


  <xsl:template name="Positions" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet">
    <xsl:param name="FundName"/>
    <xsl:param name="CounterpartyGroup"/>
    <xsl:param name="Position"/>

    <xsl:variable name="PositionName"  select="$Position/@reporting:name"/>
    <xsl:variable name="Counterparty"  select="../../@reporting:name"/>
    <xsl:variable name="CounterpartyGr"  select="$Position/../../../@reporting:name"/>
    <!--<xsl:variable name="TypeName"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/@reporting:name"/>-->
    <!--<xsl:variable name="ucitsCommitmentCurrFund"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio[@reporting:name=$Type]/reporting:ucitsCommitmentCurr.Fund,'#.########','coerce')"/>-->
    <xsl:variable name="CptyRiskOfNAV"  select="format-number($Position/reporting:cptyRiskOfNav,'#0.##','coerce1')"/>
    <!--<xsl:variable name="RBCFundNAV"  select="format-number(/reporting:root/reporting:default0/reporting:window/reporting:folio[@reporting:name=$FundName]/reporting:folio/reporting:folio[@reporting:name=$Counterparty]/reporting:cptyRiskOfNav,'#.########','coerce')"/>-->
    <xsl:variable name="Result"  select="format-number($Position/reporting:result,'###,###.00','coerce')"/>
    <xsl:variable name="ResultCurrGlobal"  select="format-number($Position/reporting:resultCurr.Global,'###,###.00','coerce')"/>
    <xsl:variable name="Unrealized"  select="format-number($Position/reporting:unrealized,'#.########','coerce')"/>
    <xsl:variable name="Allotment"  select="$Position/reporting:allotment"/>
    <xsl:variable name="UnrealizedCurrGlobal"  select="format-number($Position/reporting:unrealizedCurr.Global,'###,###.00','coerce')"/>   
    <xsl:variable name="instrumentReference"  select="$Position/reporting:instrumentReference"/>
    <xsl:variable name="NrOfSecurities"  select="format-number($Position/reporting:numberOfSecurities,'###,###.00','coerce')"/>

    <Row>
      <Cell/>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$FundName"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$CounterpartyGr"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$Counterparty"/>
        </Data>
      </Cell>
      <Cell>
      <Data ss:Type="String">
        <xsl:value-of select="$Allotment"/>
      </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$PositionName"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$instrumentReference"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="String">
          <xsl:value-of select="$CptyRiskOfNAV"/>%
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$Result"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$ResultCurrGlobal"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$Unrealized"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$UnrealizedCurrGlobal"/>
        </Data>
      </Cell>
      <Cell>
        <Data ss:Type="Number">
          <xsl:value-of select="$NrOfSecurities"/>
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

        <Table ss:ExpandedColumnCount="18" x:FullColumns="1"
                   x:FullRows="1" ss:DefaultRowHeight="15">
          <Column ss:AutoFitWidth="0" ss:Width="80"/>
          <Column ss:AutoFitWidth="1" ss:Width="150"/>
          <Column ss:AutoFitWidth="1" ss:Width="150"/>
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
            <Cell ss:StyleID="s68">
              <Data ss:Type="String">Funds</Data>
            </Cell>
            <Cell ss:StyleID="s68">
              <Data ss:Type="String">Counterparty (group)</Data>
            </Cell>
            <Cell ss:StyleID="s67" >
              <Data ss:Type="String">Cpty Risk % of NAV</Data>
            </Cell>
            <Cell ss:StyleID="s67">
              <Data ss:Type="String">RBC Fund NAV</Data>
            </Cell>
            <!--<Cell ss:StyleID="s67" >
              <Data ss:Type="String">Result curr. Global</Data>
            </Cell>-->
            <Cell ss:StyleID="s67" >
              <Data ss:Type="String">Unrealized curr. Global</Data>
            </Cell>          
          </Row>

          <xsl:variable name="Fund"  select="/reporting:root/reporting:default0/reporting:window/reporting:folio"/>

          <xsl:for-each select="$Fund">
            <xsl:sort order="descending" select="number(sum(./reporting:folio[@reporting:name='Counterparty Brokers group']/reporting:folio/reporting:cptyRiskOfNav[number(text())>0]))" data-type="number"/>
            <xsl:call-template name="Funds" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
            </xsl:call-template>
          </xsl:for-each>


        </Table>


      </Worksheet>

      <Worksheet ss:Name="Positions">

        <Table ss:ExpandedColumnCount="18" x:FullColumns="1"
                   x:FullRows="1" ss:DefaultRowHeight="15">
          <Column ss:AutoFitWidth="0" ss:Width="80"/>
          <Column ss:AutoFitWidth="1" ss:Width="150"/>
          <Column ss:AutoFitWidth="1" ss:Width="150"/>
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


          <Row></Row>
          <Row></Row>
          <Row ss:Height="30">
            <Cell></Cell>
            <Cell ss:StyleID="s67">
              <Data ss:Type="String">Funds</Data>
            </Cell>
            <Cell ss:StyleID="s67">
              <Data ss:Type="String">Counterparty (group)</Data>
            </Cell>
            <Cell ss:StyleID="s67">
              <Data ss:Type="String">Counterparty</Data>
            </Cell>
            <Cell ss:StyleID="s67">
              <Data ss:Type="String">Allotment</Data>
            </Cell>
            <Cell ss:StyleID="s67">
              <Data ss:Type="String">Position name</Data>
            </Cell>
            <Cell ss:StyleID="s67">
              <Data ss:Type="String">Instrument reference</Data>
            </Cell>
            <Cell ss:StyleID="s67" >
              <Data ss:Type="String">Cpty Risk % of NAV</Data>
            </Cell>
            <Cell ss:StyleID="s67">
              <Data ss:Type="String">Result</Data>
            </Cell>
            <Cell ss:StyleID="s67" >
              <Data ss:Type="String">Result curr. Global</Data>
            </Cell>
            <Cell ss:StyleID="s67" >
              <Data ss:Type="String">Unrealized</Data>
            </Cell>
            <Cell ss:StyleID="s67" >
              <Data ss:Type="String">Unrealized curr. Global</Data>
            </Cell>
            <Cell ss:StyleID="s67" >
              <Data ss:Type="String">Number of securities</Data>
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