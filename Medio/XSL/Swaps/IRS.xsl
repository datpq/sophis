<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:ms="urn:schemas-microsoft-com:xslt" 
                xmlns:ns0="http://www.sophis.net/bo_xml" xmlns:ns1="http://www.sophis.net/otc_message" xmlns:ns2="http://www.sophis.net/trade" xmlns:ns3="http://www.sophis.net/party" xmlns:ns4="http://www.sophis.net/folio" xmlns:ns5="http://www.sophis.net/Instrument" xmlns:ns6="http://www.sophis.net/SSI" xmlns:ns7="http://www.sophis.net/NostroAccountReference" xmlns:ns8="http://sophis.net/sophis/common" xmlns:api="urn:internal-api"
                exclude-result-prefixes="ns0 ns1 ns2 ns3 ns4 ns5 ns6 ns7 ns8 api"
>

  <xsl:output method="text" indent="no"/>
<!--
  <ms:script language="C#" implements-prefix="api">  

  <ms:assembly name="SophisDotNetToolkit"/>
    <ms:assembly name="Sophis.Core"/>
    <ms:assembly name="Sophis.Core.Data"/>
    <ms:assembly name="System.Data"/>
    <ms:assembly name="Oracle.DataAccess"/>

    <![CDATA[

      public string retrieveSQLData(String SQLQuery)
      {
        try
        {
          Oracle.DataAccess.Client.OracleCommand cmdOracle = new Oracle.DataAccess.Client.OracleCommand(SQLQuery, Sophis.DataAccess.DBContext.Connection);  
          Oracle.DataAccess.Client.OracleDataReader reader = cmdOracle.ExecuteReader();
          string SQLresult = "";
          if(reader.Read())
          {
            SQLresult = reader[0].ToString();
          }
          return SQLresult;
        }
        catch(Exception e)
        {
        return "error in sql";
        }
      }
    
      public string getPlaceOfSettlementID(string pset_id)
      {
        try
        {  
          String result;
          String SQLQuery = "select shortcode from tiersgeneral where code = to_number('"+pset_id+"')";       
          result = retrieveSQLData(SQLQuery);
          return result;        
        }
        catch(Exception e)
        {
            return e.Message;
        }
      }

      public string getPlaceOfSettlementBIC(string pset_id)
      {
        try
        {  
          String result;
          String SQLQuery = "select swift from tiersgeneral where code = to_number('"+pset_id+"')";       
          result = retrieveSQLData(SQLQuery);
          return result;        
        }
        catch(Exception e)
        {
            return e.Message;
        }
      }
    
   ]]>

</ms:script> 

-->

  <!-- Templates -->
  <xsl:template name="messageid">
  <xsl:variable name="reversalid" select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:linkreversalid[1]/text()"/>
      <xsl:choose>
      <xsl:when test="$reversalid = '0'">
        <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:ident"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:linkreversalid"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <!-- Basics -->
  <xsl:template name="tradeid">
      <xsl:value-of select="//ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:tradeId[@ns2:tradeIdScheme='http://www.sophis.net/trade/tradeId/id']"/>
  </xsl:template>

  <xsl:template name="nostro_id">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:nostroAccountReference/ns7:accountAtCustodian"/>
  </xsl:template>

  <xsl:variable name="OnMarket">
    <xsl:variable name="broker" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker/ns3:party/ns3:partyId[4]/text()"/>
    <xsl:variable name="cpty" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[4]/text()"/>
  <xsl:choose>
    <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[3]/text()='EXECUTION'">
      <xsl:value-of select="'TRUE'"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="'FALSE'"/>
    </xsl:otherwise>
  </xsl:choose>
  
  </xsl:variable>
  
  <xsl:variable name="cpty" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[4]/text()"/>
  
  <xsl:variable name="folio">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:extendedPartyTradeInformation/ns4:identifierAllocationRule/ns4:folio/ns4:portfolioName[@ns4:portfolioNameScheme='http://www.sophis.net/folio/portfolioName/name']"/>
  </xsl:variable>

  <!-- Instrument -->
  
  <xsl:variable name="deal_type">
      <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:pricing/ns5:family">
            <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:pricing/ns5:family"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>Swap</xsl:text>
        </xsl:otherwise>
      </xsl:choose>
  </xsl:variable>
  <xsl:variable name="direction">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:swapTransactionType"/>
  </xsl:variable>
  
  <xsl:template name="sec-des">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityFuture/ns5:name"/>
  </xsl:template>
  
  <xsl:template name="instrument_ref">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:instrument/ns5:reference[@ns5:name='Ticker']"/>
  </xsl:template>

  <xsl:template name="instrument_isin">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:instrument/ns5:reference[@ns5:name='Isin']"/>
  </xsl:template>

  <xsl:template name="instrument_market">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:share/ns5:market/ns5:sophis"/>
  </xsl:template>
 
 <xsl:variable name="OPT_TYP">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:optionType"/>
 </xsl:variable>
 
 <xsl:template name="EUR_US_FLG">
     <xsl:choose>
         <xsl:when test="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:exercise/ns5:europeanExercise">EUR</xsl:when>
         <xsl:otherwise>US</xsl:otherwise>
    </xsl:choose>
 </xsl:template>
 
 <xsl:variable name="currency">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:currency"/>
 </xsl:variable>
 
 <xsl:variable name="paying_currency">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:notionalSchedule/ns5:notionalStepSchedule/ns5:currency"/>
 </xsl:variable>
 <xsl:variable name="receiving_currency">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:notionalSchedule/ns5:notionalStepSchedule/ns5:currency"/>
 </xsl:variable>
 
 
 <xsl:variable name="UND_SEC_COD">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:underlyer/ns5:reference[@ns5:name='Bloomberg']"/>
 </xsl:variable>
  <!-- Accounts -->
  <xsl:variable name="MGP">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxSwap/ns2:fxLeg1/ns2:nostroSecurity/ns7:nostroAccountReference/ns8:accountAtCustodian"/>
  </xsl:variable>

  <!-- Economics -->
  <xsl:variable name="trade_qty">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:numberOfSecurities,'#')"/>
  </xsl:variable>
  <xsl:variable name="trade_gross">
     <xsl:value-of select="($trade_spot*$trade_qty)*$trade_contract_size"/>
  </xsl:variable>
  <xsl:variable name="trade_net">
  	<!-- $trade_gross + $trade_fees -->
    <xsl:value-of select="0+$trade_gross+$total_fees"/>
  </xsl:variable>
  <xsl:variable name="trade_spot">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:spot,'#.####')"/>
  </xsl:variable>
  <xsl:variable name="trade_contract_size">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:contractSize, '#.####')"/>
  </xsl:variable>

  <xsl:template name="trade_settlement_ccy">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:currency"/>
  </xsl:template>

  <xsl:template name="trade_forex">
    <xsl:if test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex">
       <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex,'0.######')"/>
    </xsl:if>
   
  </xsl:template>
  
  <xsl:variable name="upfront_amount">
    <xsl:variable name="fx_rate">
        <xsl:choose>
            <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex">
                <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex,'#.#####')"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="number('1')"/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:variable>
    
    <!-- <xsl:variable name="abs_amount" select="number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount) / $fx_rate"/> -->
    <xsl:variable name="abs_amount" select="number(/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:amount) div $fx_rate"/>
    
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount[@ns2:negative='true']">
            <xsl:value-of select="-format-number($abs_amount, '0.####')"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="format-number($abs_amount, '0.####')"/>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="str_pri">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:strike/ns5:strikeValue/ns5:strikePrice/ns5:amount"/>
  </xsl:variable>
  
  <xsl:variable name="notional">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:notional/ns5:amount"/>
  </xsl:variable>
  
  <xsl:variable name="instrument_count">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:numberOfSecurities"/>
  </xsl:variable>
  
  <xsl:variable name="nominal">
    <xsl:value-of select="format-number($notional * $instrument_count, '#.####')"/>
  </xsl:variable>
  
  <xsl:variable name="receiving_rate_type">
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:fixedRateSchedule">
          <xsl:value-of select="'Fixed Rate'"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:floatingRateCalculation/ns5:floatingRateIndex"/>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
    <xsl:variable name="paying_rate_type">
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:fixedRateSchedule">
          <xsl:value-of select="'Fixed Rate'"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:floatingRateCalculation/ns5:floatingRateIndex"/>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  
  <xsl:template name="rate_margin">
    <!-- <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:accruedCoupon/ns2:rate,  '#.####')"/> -->
    <!-- <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:floatingFlowsList/ns5:floatingFlow[1]/ns5:spread, '#.####')"/> -->
    
    <xsl:for-each select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:floatingFlowsList/ns5:floatingFlow">
      <xsl:if test="position() = 1">
          <!--<xsl:value-of select="format-number(/ns5:spread, '#.####')"/>-->
          <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:floatingFlowsList/ns5:floatingFlow/ns5:spread"/>
      </xsl:if>
    </xsl:for-each>
    
  </xsl:template>
  
  <xsl:variable name="receiving_rate_margin">
   <xsl:choose>
    <xsl:when test="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:floatingFlowsList">
      <xsl:for-each select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:floatingFlowsList/ns5:floatingFlow">
          <xsl:if test="position() = 1">
              <!--<xsl:value-of select="format-number(/ns5:spread, '#.####')"/>-->
              <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:floatingFlowsList/ns5:floatingFlow/ns5:spread"/>
          </xsl:if> 
     </xsl:for-each>
    </xsl:when>
    <xsl:otherwise>
       <xsl:for-each select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:fixedFlowsList/ns5:fixedFlow">
          <xsl:if test="position() = 1">
              <!--<xsl:value-of select="format-number(/ns5:spread, '#.####')"/>-->
              <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:fixedFlowsList/ns5:fixedFlow/ns5:fixedRate"/>
          </xsl:if>
       </xsl:for-each>
    </xsl:otherwise>
   </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="paying_rate_margin">
   <xsl:choose>
    <xsl:when test="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:floatingFlowsList">
      <xsl:for-each select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:floatingFlowsList/ns5:floatingFlow">
          <xsl:if test="position() = 1">
              <!--<xsl:value-of select="format-number(/ns5:spread, '#.####')"/>-->
              <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:floatingFlowsList/ns5:floatingFlow/ns5:spread"/>
          </xsl:if>
     </xsl:for-each>
    </xsl:when>
    <xsl:otherwise>
       <xsl:for-each select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:fixedFlowsList/ns5:fixedFlow">
          <xsl:if test="position() = 1">
              <!--<xsl:value-of select="format-number(/ns5:spread, '#.####')"/>-->
              <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:fixedFlowsList/ns5:fixedFlow/ns5:fixedRate"/>
          </xsl:if>
       </xsl:for-each>
    </xsl:otherwise>
   </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="paying_period_base">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodFrequency/ns5:periodEnum"/>
  </xsl:variable>
  <xsl:variable name="paying_period_multiplier">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodFrequency/ns5:periodMultiplier"/>
  </xsl:variable>
  
  <xsl:variable name="receiving_period_base">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodFrequency/ns5:periodEnum"/>
  </xsl:variable>
  <xsl:variable name="receiving_period_multiplier">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodFrequency/ns5:periodMultiplier"/>
  </xsl:variable>
  
  <xsl:variable name="receiving_day_convention_sophis">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodDatesAdjustments/ns5:businessDayConvention"/>
  </xsl:variable>
  <xsl:variable name="paying_day_convention_sophis">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodDatesAdjustments/ns5:businessDayConvention"/>
  </xsl:variable>
  <xsl:variable name="receiving_day_convention">
    <!-- <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodDatesAdjustments/ns5:businessDayConvention"/>-->
    <xsl:variable name="dc" select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodDatesAdjustments/ns5:businessDayConvention"/>
    <xsl:choose>
        <xsl:when test="starts-with($dc, 'MODFOLLOWING')">
            <xsl:value-of select="'Modified following'"/>
        </xsl:when>
        <xsl:when test="starts-with($dc,'MODNEAREST')">
            <xsl:value-of select="'Modified nearest'"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="$dc"/>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="paying_day_convention">
    <!-- <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodDatesAdjustments/ns5:businessDayConvention"/>-->
    <xsl:variable name="dc" select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodDatesAdjustments/ns5:businessDayConvention"/>
    <xsl:choose>
        <xsl:when test="starts-with($dc, 'MODFOLLOWING')">
            <xsl:value-of select="'Modified following'"/>
        </xsl:when>
        <xsl:when test="starts-with($dc,'MODNEAREST')">
            <xsl:value-of select="'Modified nearest'"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="$dc"/>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <!-- Fees -->
  <xsl:variable name="total_fees">
  	<xsl:value-of select="$market_fees+$broker_fees+$counterparty_fees"/>
  </xsl:variable>
  <xsl:variable name="market_fees">
  	<xsl:choose>

    <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/marketFees']/ns2:paymentAmount/ns5:amount">
      <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/marketFees']/ns2:paymentAmount/ns5:amount,'#.######')"/>
    </xsl:when>
    <xsl:otherwise>
    	<xsl:value-of select="0"/>
    	</xsl:otherwise>
</xsl:choose>
  </xsl:variable>

  <xsl:variable name="broker_fees">
  	<xsl:choose>

    <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/brokerFees']/ns2:paymentAmount/ns5:amount">
      <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/brokerFees']/ns2:paymentAmount/ns5:amount,'#.######')"/>
    </xsl:when>
    <xsl:otherwise>
    	<xsl:value-of select="0"/>
    	</xsl:otherwise>
</xsl:choose>
  </xsl:variable>

  <xsl:variable name="counterparty_fees">
  	<xsl:choose>

    <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/counterpartyFees']/ns2:paymentAmount/ns5:amount">
      <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/counterpartyFees']/ns2:paymentAmount/ns5:amount,'#.######')"/>
    </xsl:when>
    <xsl:otherwise>
    	<xsl:value-of select="0"/>
    	</xsl:otherwise>
</xsl:choose>
  </xsl:variable>

<xsl:variable name="receiving_base">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:dayCountFraction"/>
</xsl:variable>
<xsl:variable name="paying_base">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:dayCountFraction"/>
</xsl:variable>


  <xsl:template name="base_n_days">
    <xsl:param name = "raw_base" />
    <!-- <xsl:variable name="raw_base" select="/ns0:sph_otc_message/ns0:instrument/*/ns5:interestType/*/ns5:dayCountBasis"/> -->
    <xsl:choose>
        <xsl:when test="contains($raw_base, '365') and contains($raw_base, 'FIXED')">
            <xsl:text>AFI/365</xsl:text>
        </xsl:when>
        <xsl:when test="starts-with($raw_base, 'ACT/ACT') or starts-with($raw_base, 'ACT/365')">
            <xsl:text>ACT/365</xsl:text>
        </xsl:when>
        <xsl:when test="starts-with($raw_base, '30E/360') or contains($raw_base,'Eurobond')">
            <xsl:text>30E/360</xsl:text>
        </xsl:when>
        <xsl:when test="starts-with($raw_base, '30/360') or starts-with($raw_base,'360/360')">
            <xsl:text>360/360</xsl:text>
        </xsl:when>
        <xsl:when test="starts-with($raw_base, 'ACT/360')">
            <xsl:text>ACT/360</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="$raw_base"/>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
<xsl:variable name="receiving_calculation_method">
    <xsl:call-template name="base_n_days">
        <xsl:with-param name="raw_base" select = "$receiving_base" />
    </xsl:call-template>
</xsl:variable>

<xsl:variable name="paying_calculation_method">
    <xsl:call-template name="base_n_days">
        <xsl:with-param name="raw_base" select = "$paying_base" />
    </xsl:call-template>
</xsl:variable>

  <!-- Settlements -->
  <xsl:template name="set-cur">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityFuture/ns5:currency"/>
  </xsl:template>
  
  <xsl:template name="set-net-amt">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount"/>
  </xsl:template>
  
  <xsl:template name="nostro_pset_id">
    <xsl:variable name="pset_id" select="//ns3:advancedData/ns3:settlementInstruction/ns3:ssiPathId[text()=/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:ssiPathId]/../ns3:settlementPlace"/>
    <xsl:value-of select="api:getPlaceOfSettlementBIC($pset_id)"/>
  </xsl:template>

  <xsl:template name="lostro_custodian_bic">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:custodian"/>
  </xsl:template>

  <xsl:template name="lostro_custodian_acc">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:accountAtCustodian"/>
  </xsl:template>


  <xsl:template name="lostro_agent_bic">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:agentCode"/>
  </xsl:template>

  <xsl:template name="lostro_agent_acc">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:accountAtAgent"/>
  </xsl:template>

  <xsl:template name="futures_acc">
  	<xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:agentCode"/>
  </xsl:template>

  <xsl:template name="lostro_clearing_code">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:counterparty/ns3:party/ns3:description/ns3:advancedData/ns3:settlementInstruction/ns3:ssiPathId[text()=/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:ssiPathId]/../ns3:UserColumnsData/ns3:UserColumn[1]/ns3:Value"/>
  </xsl:template>

  <xsl:template name="lostro_clearing_acc">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:counterparty/ns3:party/ns3:description/ns3:advancedData/ns3:settlementInstruction/ns3:ssiPathId[text()=/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:ssiPathId]/../ns3:UserColumnsData/ns3:UserColumn[2]/ns3:Value"/>
  </xsl:template>


  <xsl:template name="nostro_custodian_bic">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:custodian"/>
  </xsl:template>

   <xsl:template name="nostro_custodian_acc">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:accountAtCustodian"/>
  </xsl:template>

  <xsl:template name="smdt">
    <xsl:variable name="sophis_smdt" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:SMDT"/>
    <xsl:choose>
      <xsl:when test="$sophis_smdt = 'RBC Custody / FOP'">
        <xsl:value-of select="'F'"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="'A'"/>
      </xsl:otherwise>
      </xsl:choose>
  </xsl:template>

  <xsl:variable name="UTI">
  <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:tradeId[@ns2:tradeIdScheme='http://www.sophis.net/trade/tradeId/USI_ident']/text()">
            <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:tradeId[@ns2:tradeIdScheme='http://www.sophis.net/trade/tradeId/USI_ident']/text()"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>UTI000</xsl:text>
        </xsl:otherwise>
    </xsl:choose>
    
  </xsl:variable>
  
  <xsl:variable name="USI">
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:tradeId[@ns2:tradeIdScheme='http://www.sophis.net/trade/tradeId/USI_namespace']">
            <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:tradeId[@ns2:tradeIdScheme='http://www.sophis.net/trade/tradeId/USI_namespace']/text()"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>USI000</xsl:text>
        </xsl:otherwise>
    </xsl:choose>
    
  </xsl:variable>
  
  <!-- Times and Dates -->
  <xsl:variable name="receiving_beginning_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:paymentDates/ns5:firstPaymentDate/ns8:adjustableDate/ns8:unadjustedDate, 'yyyyMMdd')"/>
  </xsl:variable>
  <xsl:variable name="paying_beginning_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:paymentDates/ns5:firstPaymentDate/ns8:adjustableDate/ns8:unadjustedDate, 'yyyyMMdd')"/>
  </xsl:variable>
  
  <xsl:variable name="value_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:valueDate/ns8:unadjustedDate,'yyyyMMdd')"/>
  </xsl:variable>
  <xsl:template name="trade_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:tradeDate,'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="settlement_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:settlementDate,'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="receiving_maturity_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:terminationDate/ns8:adjustableDate/ns8:unadjustedDate, 'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="paying_maturity_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:terminationDate/ns8:adjustableDate/ns8:unadjustedDate, 'yyyyMMdd')"/>
  </xsl:template>
  
  <xsl:variable name="confirmation_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:messageCreationTimestamp,'yyyMMdd')"/>
  </xsl:variable>
  <xsl:variable name="confirmation_time">
    <xsl:value-of select="ms:format-time(/ns0:sph_otc_message/ns0:messageCreationTimestamp,'hhmmss')"/>
  </xsl:variable>
  
  <xsl:template name="trade_time">
    <xsl:value-of select="ms:format-time(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:tradeTime,'hhmmss')"/>
  </xsl:template>
    <xsl:template name="message_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:messageCreationTimestamp,'yyyyMMdd')"/>
  </xsl:template>
    <xsl:template name="message_time">
    <xsl:value-of select="ms:format-time(/ns0:sph_otc_message/ns0:messageCreationTimestamp,'hhmmss')"/>
  </xsl:template>
  <xsl:template name="action">
    <xsl:variable name="reversalid" select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:linkreversalid[1]/text()"/>
      <xsl:choose>
      <xsl:when test="$reversalid = '0'">
        <xsl:text>CREATE</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>DELETE</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template name="first_coupon_date">
      <xsl:for-each select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:accruedCoupon">
      <xsl:if test="position() = 1">
          <xsl:value-of select="ms:format-date(ns2:date/ns8:unadjustedDate, 'yyyyMMdd')"/>
      </xsl:if>
    </xsl:for-each>
  </xsl:template>
  
  <xsl:variable name="receiving_first_coupon_date">
     <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:paymentDates/ns5:firstPaymentDate/ns8:adjustableDate/ns8:unadjustedDate,'yyyyMMdd')"/>
  </xsl:variable>
  <xsl:variable name="paying_first_coupon_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:paymentDates/ns5:firstPaymentDate/ns8:adjustableDate/ns8:unadjustedDate, 'yyyyMMdd')"/>
  </xsl:variable>
  
  <!-- If buyer = Entity, then its a BUY trade, otherwise SELL (SEL) -->
  <xsl:template name="BuySell">
    <xsl:variable name="buyer" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:buyerPartyReference/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/id']"/>

    <xsl:variable name="entity" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:entity/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/id']"/>

    <xsl:choose>
      <xsl:when test="$buyer = $entity">
        <xsl:text>BUY</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>SEL</xsl:text>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:template>

  <xsl:template name="FileName" match="*">
    <xsl:call-template name="action"/>
    <xsl:call-template name="tradeid"/>
    <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:ident"/>
  </xsl:template>

  <xsl:template name="BrokerID" match="*">
    <xsl:variable name="broker" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker/ns3:party/ns3:partyId[4]/text()"/>
    <xsl:variable name="cpty" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[4]/text()"/>
  <xsl:choose>
    <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[3]/text()='EXECUTION'">
      <xsl:value-of select="$broker"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="$cpty"/>
    </xsl:otherwise>
  </xsl:choose>
  
  </xsl:template>
  
    <xsl:template name="depositary_ext_ref">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:depositary/ns3:party/ns3:description/ns3:names/ns3:externalReference"/>
  </xsl:template>

   <xsl:template name="BrokerName" match="*">
    <xsl:variable name="broker" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']/text()"/>
    <xsl:variable name="cpty" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']/text()"/>
  <xsl:choose>
    <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']/text()='EXECUTION'">
      <xsl:value-of select="$broker"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="$cpty"/>
    </xsl:otherwise>
  </xsl:choose>
  
  </xsl:template>  
  
  <xsl:template name="tic_bbg">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:position/ns4:position/ns4:underlyer/ns5:reference[@ns5:name='Bloomberg']/text()"/>
  </xsl:template>

  <xsl:variable name="seperator" select="';'"/>
 <!--<xsl:variable name="newline" select=""/>-->


<xsl:template match="*">
  <xsl:text>FROM;TO;DATE;TIME;PATH;NAME;EXTNAME;ANSWER1;COUNT &#xa;</xsl:text>
  <!-- Document Header -->
  <xsl:value-of select="'MEDIO'"/>
  <xsl:value-of select="$seperator"/>
  <xsl:value-of select="'RBC'"/> 
  <xsl:value-of select="$seperator"/>
  <xsl:call-template name="message_date"/>
  <xsl:value-of select="$seperator"/>
  <xsl:call-template name="message_time"/>
  <xsl:value-of select="$seperator"/>
  <!-- File Path -->
  <xsl:value-of select="$seperator"/>
  <!-- File Name -->
  <xsl:call-template name="FileName"/>
  <xsl:value-of select="$seperator"/>
  <xsl:value-of select="'TXT'"/>
  <xsl:value-of select="$seperator"/>
  <xsl:value-of select="'FTP_CUS'"/>
  <xsl:value-of select="$seperator"/>
  <xsl:value-of select="'1'"/>
  <xsl:value-of select="$seperator"/>
  <xsl:text>&#xa;</xsl:text>

  <!-- Document Body -->
  <xsl:text>ACTION; EMETTEUR; EXT-REF; INTERNAL_ORIGID; INTERNAL_ID; INTERNAL_STATUS; EXTERNAL_ORIGID; EXTERNAL_ID; EXTERNAL_STATUS; DATE_OUT; TIME_OUT; ERROR_MESSAGE; Trade ref; Deal Type; Direction; Portfolio; Counterparty; Currency; Trade date; Beginning date; Maturity date; Nominal; Rate type; Rate margin; Base; Coupon term unit; Day convention; First coupon date; Termination/Upfront Amount; Fee Settlement date; Unique Transaction Identifier Prefix; Unique Transaction Identifier Value; Delivery type; Execution time; Confirmation date; Confirmation time &#xa;</xsl:text>
  <!-- Receiving Leg -->
  <!-- ACTION -->
  <xsl:call-template name="action"/>
  <xsl:value-of select="$seperator"/>

  <!-- EMETTEUR -->
  <xsl:value-of select="'MEDIO'"/>
  <xsl:value-of select="$seperator"/>

  <!-- EXT-REF -->
  <xsl:call-template name="messageid"/>
  <xsl:value-of select="$seperator"/>
  <!-- INTERNAL_ORIGID -->
  <xsl:value-of select="$seperator"/>
  <!-- INTERNAL_ID -->
  <xsl:value-of select="$seperator"/>
  <!-- INTERNAL_STATUS -->
  <xsl:value-of select="$seperator"/>
  <!-- EXTERNAL_ORIGID -->
  <xsl:value-of select="$seperator"/>
  <!-- EXTERNAL_ID -->
  <xsl:value-of select="$seperator"/>
  <!-- EXTERNAL_STATUS -->
  <xsl:value-of select="$seperator"/>
  <!-- DATE_OUT -->
  <xsl:call-template name="message_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- TIME_OUT -->
  <xsl:call-template name="message_time"/>
  <xsl:value-of select="$seperator"/>
  <!-- ERROR_MESSAGE -->
  <xsl:value-of select="$seperator"/>
  <!-- Trade ref -->
  <xsl:call-template name="tradeid"/>
  <xsl:value-of select="$seperator"/>
  <!-- Deal Type [TODO][CHECK]-->
  <xsl:value-of select="$deal_type"/>
  <xsl:value-of select="$seperator"/>
  <!-- Direction [TODO][CHECK] -->
  <xsl:value-of select="'Receivable Leg'"/>
  <xsl:value-of select="$seperator"/>
  <!-- Portfolio -->
  <xsl:value-of select="$folio"/>
  <xsl:value-of select="$seperator"/>
  <!-- Counterparty -->
  <xsl:value-of select="$cpty"/>
  <xsl:value-of select="$seperator"/>
  <!-- Currency -->
  <xsl:value-of select="$receiving_currency"/>
  <xsl:value-of select="$seperator"/>
  <!-- Trade date -->
  <xsl:call-template name="trade_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Beginning date -->
  <xsl:value-of select="$receiving_beginning_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Maturity date [TODO][CHECK]-->
  <xsl:call-template name="receiving_maturity_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Nominal [TODO][CHECK] -->
  <xsl:value-of select="$nominal"/>
  <xsl:value-of select="$seperator"/>
  <!-- Rate type [TODO][NOT FOUND] -->
  <xsl:value-of select="$receiving_rate_type"/>
  <xsl:value-of select="$seperator"/>
  <!-- Rate margin [TODO][CHECK]-->
  <xsl:value-of select="$receiving_rate_margin"/>
  <xsl:value-of select="$seperator"/>
  <!-- Base [TODO][CHECK] -->
  <xsl:value-of select="$receiving_calculation_method"/>
  <xsl:value-of select="$seperator"/>
  <!-- Coupon term unit [TODO][CHECK] -->
  <xsl:value-of select="concat($receiving_period_multiplier,' ', $receiving_period_base)" />
  <xsl:value-of select="$seperator"/>
  <!-- Day Convention [TODO][CHECK] -->
  <xsl:value-of select="$receiving_day_convention"/>
  <xsl:value-of select="$seperator"/>
  <!-- First Coupon Date [TODO][CHECK] -->
  <xsl:value-of select="$receiving_first_coupon_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Termination/Upfront Amount [TODO][NOT FOUND]-->
  <xsl:value-of select="$upfront_amount"/>
  <xsl:value-of select="$seperator"/>
  <!-- Fee Settlement Date [TODO][CHECK]-->
  <xsl:call-template name="settlement_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Unique Transaction Identifier Prefix (USI?) [TODO][NOT FOUND]-->
    <xsl:value-of select="$USI"/>
  <xsl:value-of select="$seperator"/>
  <!-- Unique Transaction Identifier Value (UTI) [TODO][CHECK]-->
  <xsl:value-of select="$UTI"/>
  <xsl:value-of select="$seperator"/>
  <!-- Delivery Type [TODO][NOTFOUND] -->
  <xsl:value-of select="'Cash'"/>
  <xsl:value-of select="$seperator"/>
  <!-- Execution time [TODO] -->
  <xsl:call-template name="trade_time"/>
  <xsl:value-of select="$seperator"/>
  <!-- Confirmation Date [TODO][CHECK]-->
  <xsl:value-of select="$confirmation_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Confirmation Time [TODO][CHECK]-->
  <xsl:value-of select="$confirmation_time"/>
  
  <xsl:text>&#xa;</xsl:text>
  
  <!-- Paying Leg -->
  <xsl:call-template name="action"/>
  <xsl:value-of select="$seperator"/>

  <!-- EMETTEUR -->
  <xsl:value-of select="'MEDIO'"/>
  <xsl:value-of select="$seperator"/>

  <!-- EXT-REF -->
  <xsl:call-template name="messageid"/>
  <xsl:value-of select="$seperator"/>
  <!-- INTERNAL_ORIGID -->
  <xsl:value-of select="$seperator"/>
  <!-- INTERNAL_ID -->
  <xsl:value-of select="$seperator"/>
  <!-- INTERNAL_STATUS -->
  <xsl:value-of select="$seperator"/>
  <!-- EXTERNAL_ORIGID -->
  <xsl:value-of select="$seperator"/>
  <!-- EXTERNAL_ID -->
  <xsl:value-of select="$seperator"/>
  <!-- EXTERNAL_STATUS -->
  <xsl:value-of select="$seperator"/>
  <!-- DATE_OUT -->
  <xsl:call-template name="message_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- TIME_OUT -->
  <xsl:call-template name="message_time"/>
  <xsl:value-of select="$seperator"/>
  <!-- ERROR_MESSAGE -->
  <xsl:value-of select="$seperator"/>
  <!-- Trade ref -->
  <xsl:call-template name="tradeid"/>
  <xsl:value-of select="$seperator"/>
  <!-- Deal Type [TODO][CHECK]-->
  <xsl:value-of select="$deal_type"/>
  <xsl:value-of select="$seperator"/>
  <!-- Direction [TODO][CHECK] -->
  <xsl:value-of select="'Payable Leg'"/>
  <xsl:value-of select="$seperator"/>
  <!-- Portfolio -->
  <xsl:value-of select="$folio"/>
  <xsl:value-of select="$seperator"/>
  <!-- Counterparty -->
  <xsl:value-of select="$cpty"/>
  <xsl:value-of select="$seperator"/>
  <!-- Currency -->
  <xsl:value-of select="$currency"/>
  <xsl:value-of select="$seperator"/>
  <!-- Trade date -->
  <xsl:call-template name="trade_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Beginning date -->
  <xsl:value-of select="$paying_beginning_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Maturity date [TODO][CHECK]-->
  <xsl:call-template name="paying_maturity_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Nominal [TODO][CHECK] -->
  <xsl:value-of select="$nominal"/>
  <xsl:value-of select="$seperator"/>
  <!-- Rate type [TODO][NOT FOUND] -->
  <xsl:value-of select="$paying_rate_type"/>
  <xsl:value-of select="$seperator"/>
  <!-- Rate margin [TODO][CHECK]-->
  <xsl:value-of select="$paying_rate_margin"/>
  <!--<xsl:call-template name="rate_margin"/>-->
  <xsl:value-of select="$seperator"/>
  <!-- Base [TODO][CHECK] -->
  <xsl:value-of select="$paying_calculation_method"/>
  <xsl:value-of select="$seperator"/>
  <!-- Coupon term unit [TODO][CHECK] -->
  <xsl:value-of select="concat($paying_period_multiplier,' ', $paying_period_base)" />
  <xsl:value-of select="$seperator"/>
  <!-- Day Convention [TODO][CHECK] -->
  <xsl:value-of select="$paying_day_convention"/>
  <xsl:value-of select="$seperator"/>
  <!-- First Coupon Date [TODO][CHECK] -->
  <xsl:value-of select="$paying_first_coupon_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Termination/Upfront Amount [TODO][NOT FOUND]-->
  <xsl:value-of select="$seperator"/>
  <!-- Fee Settlement Date [TODO][CHECK]-->
  <xsl:call-template name="settlement_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Unique Transaction Identifier Prefix (UTI) [TODO][NOT FOUND]-->
  <xsl:value-of select="$seperator"/>
  <!-- Unique Transaction Identifier Value (UTI) [TODO][NOT FOUND]-->
  <xsl:value-of select="$seperator"/>
  <!-- Delivery Type [TODO][NOTFOUND] -->
  <xsl:value-of select="$seperator"/>
  <!-- Execution time [TODO] -->
  <xsl:value-of select="$seperator"/>
  <!-- Confirmation Date [TODO][CHECK]-->
  <xsl:value-of select="$seperator"/>
  <!-- Confirmation Time [TODO][CHECK]-->

  <!-- - - - - - -->
  <!-- - - - - - -->
  
</xsl:template>

</xsl:stylesheet>