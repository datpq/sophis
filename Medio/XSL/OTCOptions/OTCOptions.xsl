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

  <!-- Instrument -->
  <xsl:variable name="INST_CODE">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:instrument/ns5:reference[@ns5:name='Bloomberg']"/>
  </xsl:variable>
  
  <xsl:template name="sec-des">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:name"/>
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
  
   <xsl:template name="SEC-COD">
    <xsl:value-of select="//ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:identifier/ns5:reference[@ns5:name='Bloomberg']"/>
  </xsl:template>
  
 <xsl:variable name="OPT_TYP">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/*/ns5:optionType"/>
 </xsl:variable>
 
 <xsl:template name="EUR_US_FLG">
     <xsl:choose>
         <xsl:when test="/ns0:sph_otc_message/ns0:instrument/*/ns5:exercise/ns5:europeanExercise">European</xsl:when>
         <xsl:otherwise>American</xsl:otherwise>
    </xsl:choose>
 </xsl:template>
 
 <xsl:variable name="UND_SEC_COD">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:underlyer/ns5:reference[@ns5:name='Bloomberg']"/>
 </xsl:variable>
  <!-- Accounts -->
  <xsl:variable name="MGP">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:accountAtCustodian"/>
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

  <xsl:variable name="str_pri">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:strike/ns5:strikeValue/ns5:strikePrice/ns5:amount"/>
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


  <!-- Settlements -->
  <xsl:template name="set-cur">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:currency"/>
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


  <!-- Times and Dates -->
  <xsl:template name="trade_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:tradeDate,'ddMMyyyy')"/>
  </xsl:template>
  <xsl:template name="termination_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/*/ns5:rateLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:terminationDate/ns8:adjustableDate/ns8:unadjustedDate,'ddMMyyyy')"/>
  </xsl:template>
  <xsl:template name="settlement_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:settlementDate,'ddMMyyyy')"/>
  </xsl:template>
  <xsl:template name="value_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:paymentDate,'ddMMyyyy')"/>
  </xsl:template>
  <xsl:template name="maturity_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/*/ns5:exercise/*/ns5:expirationDate/ns8:adjustableDate/ns8:unadjustedDate, 'ddMMyyyy')"/>
  </xsl:template>
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
  
  <xsl:variable name="business_event">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:businessevent/ns1:name"/>
  </xsl:variable>
  
    <xsl:variable name="OPE_TYP_OLD">
        <xsl:variable name="buyer" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:buyerPartyReference/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/id']"/>

    <xsl:variable name="entity" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:entity/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/id']"/>

    <xsl:choose>
      <xsl:when test="$buyer = $entity">
        <xsl:text>AOP</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>AOS</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="OPE_TYP">
    <xsl:choose>
        <xsl:when test="(contains($business_event, 'opening') or contains($business_event, 'continuing')) and contains($business_event, 'long')">
            <xsl:text>AOP</xsl:text>
        </xsl:when>
        <xsl:when test="(contains($business_event, 'opening') or contains($business_event, 'continuing')) and contains($business_event, 'short')">
            <xsl:text>AOS</xsl:text>
        </xsl:when>
        <xsl:when test="contains($business_event, 'long')">
            <xsl:text>AOS</xsl:text>
        </xsl:when>
        <xsl:when test="contains($business_event, 'short')">
            <xsl:text>AOP</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="$OPE_TYP_OLD"/>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="TYP_TRT">
    <xsl:choose>
        <xsl:when test="contains($business_event, 'opening')">
            <xsl:text>O</xsl:text>
        </xsl:when>
        <xsl:when test="contains($business_event, 'continuing')">
            <xsl:text>T</xsl:text>
        </xsl:when>
        <xsl:when test="contains($business_event, 'closing')">
            <xsl:text>C</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>O</xsl:text>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  
  


  <xsl:template name="FileName" match="*">
    <xsl:call-template name="action"/>
    <xsl:call-template name="tradeid"/>
    <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:ident"/>
  </xsl:template>

  <!-- if on market, get broker, if off market, get cpty -->
  <xsl:template name="BrokerID">
    <xsl:variable name="broker" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']/text()"/>
    <xsl:variable name="cpty" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']/text()"/>
    <xsl:choose>
		<xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[3]/text()='EXECUTION'">
		  <xsl:value-of select="$broker"/>
		</xsl:when>
		<xsl:otherwise>
		  <xsl:value-of select="$cpty"/>
		</xsl:otherwise>
    </xsl:choose>
  </xsl:template>

   <xsl:template name="BrokerName">
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
  
  <xsl:template name="brk-des" match="*">
    <xsl:variable name="broker" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']/text()"/>
    <xsl:variable name="cpty" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']/text()"/>
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

  <!--
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
  -->
  
   <xsl:variable name="und_sec_des">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:underlyer/ns5:reference[@ns5:name='Ticker']/text()"/>
   </xsl:variable>
  <xsl:template name="tic_bbg">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:position/ns4:position/ns4:underlyer/ns5:reference[@ns5:name='Bloomberg']/text()"/>
  </xsl:template>
  
  <xsl:variable name="int_ref">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:agentCode"/>
  </xsl:variable>

  <xsl:variable name="int_ref_lib">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:accountAtAgent"/>
  </xsl:variable>
  
  <xsl:variable name="chg_rat">
    <xsl:if  test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex">
        <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex,'#.####')"/>
    </xsl:if>
  </xsl:variable>
  
  <xsl:variable name="folio_id">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:extendedPartyTradeInformation/ns4:identifierAllocationRule/ns4:folio/ns4:portfolioName[@ns4:portfolioNameScheme='http://www.sophis.net/folio/portfolioName/id']"/>
  </xsl:variable>
  
  <xsl:variable name="folio_name">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:extendedPartyTradeInformation/ns4:identifierAllocationRule/ns4:folio/ns4:portfolioName[@ns4:portfolioNameScheme='http://www.sophis.net/folio/portfolioName/name']"/>
  </xsl:variable>
  
  <xsl:variable name="cpty_name">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']"/>
  </xsl:variable>
  
  <xsl:variable name="underlying_id">
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:instrument/*/ns5:underlyer/ns5:reference[@ns5:name='Ticker']">
            <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/*/ns5:underlyer/ns5:reference[@ns5:name='Ticker']"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/*/ns5:underlyer/ns5:reference[@ns5:name='ISIN']"/>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="payer">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment/ns2:payerPartyReference/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/externalReference']"/>
  </xsl:variable>
  <xsl:variable name="receiver">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment/ns2:receiverPartyReference/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/externalReference']"/>
  </xsl:variable>
  <xsl:variable name="payer_receiver">
    <xsl:value-of select="concat($payer,' ',$receiver)"/>
  </xsl:variable>
  
  <xsl:variable name="option_type">
    <xsl:variable name="feature" select="/ns0:sph_otc_message/ns0:instrument/*/ns5:feature"/>
    <xsl:choose>
        <xsl:when test="contains($feature,'Barrier-None')">
            <xsl:choose>
                <xsl:when test="/ns0:sph_otc_message/ns0:instrument/*/*/ns5:digital">
                    <xsl:text>Digital Vanilla</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:text>Vanilla</xsl:text>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:when>
        <xsl:when test="contains($feature,'Digital') and contains($feature, 'Double')">
            <xsl:text>Digital Double Barrier</xsl:text>
        </xsl:when>
        <xsl:when test="contains($feature,'Digital') and contains($feature, 'Barrier')">
            <xsl:text>Digital Barrier</xsl:text>
        </xsl:when>
        <xsl:when test="contains($feature,'Barrier') and contains($feature, 'Double')">
            <xsl:text>Double Barrier</xsl:text>
        </xsl:when>
        <xsl:when test="contains($feature,'Barrier')">
            <xsl:text>Barrier</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>Vanilla</xsl:text>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="premium">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount,'#.######')"/>
  </xsl:variable>
  <xsl:variable name="premium_ccy">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:currency"/>
  </xsl:variable>
  
  <xsl:variable name="quantity">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:numberOfSecurities"/>
  </xsl:variable>
  <xsl:variable name="strike_price">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/*/ns5:strike/ns5:strikeValue/ns5:strikePrice/ns5:amount"/>
  </xsl:variable>
  
    <xsl:template name="NEW-CANC">
    <xsl:variable name="link_reversal" select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:linkreversalid"/>
    <xsl:choose>
        <xsl:when test="$link_reversal = 0">
            <xsl:text>NEW</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>CANC</xsl:text>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:variable name="trade_identifier" select="/ns0:sph_otc_message/ns0:messageAdditionalData/ns0:MessageID"/>
  <xsl:variable name="original_trade_id" select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:linkreversalid"/>
  
  <xsl:variable name="op_cl">
    <xsl:variable name="pos_total" select="/ns0:sph_otc_message/ns0:position/ns4:position/ns4:instrumentCount"/>
    <xsl:choose>
        <xsl:when test="$pos_total = $quantity">
            <xsl:text>Opening</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>Closing</xsl:text>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
 
  <xsl:variable name="payment_frequency">
    <xsl:variable name="period_multiplier" select="/ns0:sph_otc_message/ns0:instrument/ns5:optionSwapped/ns5:rateLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodFrequency/ns5:periodMultiplier"/>
    <xsl:variable name="period_enum" select="/ns0:sph_otc_message/ns0:instrument/ns5:optionSwapped/ns5:rateLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodFrequency/ns5:periodEnum"/>
    <xsl:value-of select="concat($period_multiplier, ' ', $period_enum)"/>
  </xsl:variable>
  
  <xsl:variable name="base_floating_rate">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:optionSwapped/ns5:rateLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:dayCountFraction"/>
  </xsl:variable>
  <xsl:variable name="floating_rate_spread">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:instrument/ns5:optionSwapped/ns5:rateLeg/ns5:swapStream/ns5:floatingFlowsList/ns5:floatingFlow/ns5:spread,'#.######')"/>
  </xsl:variable>
  
  <xsl:variable name="product_type">
    <xsl:variable name="feature" select="/ns0:sph_otc_message/ns0:instrument/*/ns5:feature"/>
    <xsl:variable name="productType" select="/ns0:sph_otc_message/ns0:instrument/*/ns5:productType"/>
    <xsl:choose>
        
        <xsl:when test="contains($feature, 'Underlying-Index')">
            <xsl:text>Index Option</xsl:text>
        </xsl:when>
        <xsl:when test="contains($productType, 'FX') or contains($productType, 'Forex')">
            <xsl:text>FX Option</xsl:text>
        </xsl:when>
        <xsl:when test="contains($feature, 'Future')">
            <xsl:text>Futures Option</xsl:text>
        </xsl:when>
        <xsl:when test="starts-with($productType, 'IR-')">
            <xsl:text>Interest Option</xsl:text>
        </xsl:when>
        <xsl:when test="starts-with($productType, 'COM-')">
            <xsl:text>Commodity Option</xsl:text>
        </xsl:when>
        <xsl:when test="contains($feature, 'Metal')">
            <xsl:text>Precious Metal Option</xsl:text>
        </xsl:when>
        <xsl:otherwise>
           <xsl:text>Equity Option</xsl:text>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
    
  <xsl:param name="barrier_array" select="/ns0:sph_otc_message/ns0:instrument/*/*/ns5:barrier/*/ns5:trigger/ns5:level"/>
    <xsl:template name="barrier_1">
        <xsl:if test="$barrier_array[1]">
            <xsl:value-of select="$barrier_array[1]"/>
        </xsl:if>
    </xsl:template>
    <xsl:template name="barrier_2">
        <xsl:if test="$barrier_array[2]">
            <xsl:value-of select="$barrier_array[2]"/>
        </xsl:if>
    </xsl:template>
    
    <xsl:variable name="Currency1">
        <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:fxOption/ns5:notional/ns5:currency"/>
    </xsl:variable>
    <xsl:variable name="Currency2">
        <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:fxOption/ns5:strike/ns5:strikeValue/ns5:strikePrice/ns5:currency"/>
    </xsl:variable>
    
    <xsl:variable name="Notional">
        <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:numberOfSecurities"/>
    </xsl:variable>
    <xsl:variable name="Notional1">
        <xsl:variable name="amount1" select="/ns0:sph_otc_message/ns0:instrument/ns5:fxOption/ns5:notional/ns5:amount"/>
        <xsl:if test="$amount1">
            <xsl:value-of select="format-number($amount1 * $Notional, '#.####')"/>
        </xsl:if>
    </xsl:variable>
    <xsl:variable name="Notional2">
        <xsl:variable name="amount2" select="/ns0:sph_otc_message/ns0:instrument/ns5:fxOption/ns5:strike/ns5:strikeValue/ns5:strikePrice/ns5:amount"/>
        <xsl:if test="$amount2">
            <xsl:value-of select="format-number($amount2 * $Notional, '#.####')"/>
        </xsl:if>
    </xsl:variable>
    
    <xsl:variable name="BuySell1">
        <xsl:if test="$Currency1">
            <xsl:choose>
                <xsl:when test="contains($OPT_TYP,'Put')">
                    <xsl:text>SELL</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:text>BUY</xsl:text>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:if>
    </xsl:variable>
    
    <xsl:variable name="BuySell2">
        <xsl:if test="$Currency2">
            <xsl:choose>
                <xsl:when test="contains($OPT_TYP,'Call')">
                    <xsl:text>SELL</xsl:text>
                </xsl:when>
                <xsl:otherwise>
                    <xsl:text>BUY</xsl:text>
                </xsl:otherwise>
            </xsl:choose>
        </xsl:if>
    </xsl:variable>
    
  <xsl:variable name="seperator" select="';'"/>
 <!--<xsl:variable name="newline" select=""/>-->


<xsl:template match="*">
  <!-- Document Header -->
  <xsl:text>Portfolio ID AM;Portfolio name;Counterparty Name (compensation broker);Product Type;Underlying ID;Underlying Description;Trade Date;Value Date;Option Expiry;Call/Put (Ccy1);Option Style;Option Type;Barrier1;Barrier2;Currency1;Notional1;Buy/Sell;Currency2;Notional2;Buy/Sell;Premium;Premium Currency;Premium settlement Date;Quantity;Spot price;Strike Price;Trade ID;New/Cancel;Opening/Closing;Trade Time;Linked Trade ID; Manager Code
  </xsl:text>
  <!-- Portfolio ID AM-->
  <xsl:value-of select="$folio_id"/>
  <xsl:value-of select="$seperator"/>
  <!--Portfolio name-->
  <xsl:value-of select="$folio_name"/>
  <xsl:value-of select="$seperator"/>
<!-- Counterparty Name (compensation broker) -->
  <xsl:value-of select="$cpty_name"/>
  <xsl:value-of select="$seperator"/>
<!-- Product Type -->
  <xsl:value-of select="$product_type"/>
  <xsl:value-of select="$seperator"/>
<!-- Underlying ID -->
   <xsl:value-of select="$underlying_id"/>
   <xsl:value-of select="$seperator"/>
<!-- Underlying Description [OPTIONAL. Skipped] -->
   <xsl:value-of select="$seperator"/>
<!-- Trade Date -->
 <xsl:call-template name="trade_date"/>
 <xsl:value-of select="$seperator"/>
<!-- Value Date -->
 <xsl:call-template name="value_date"/>
 <xsl:value-of select="$seperator"/>
<!-- Option Expiry -->
 <xsl:call-template name="maturity_date"/>
 <xsl:value-of select="$seperator"/>
<!-- Call/Put (Ccy1) -->
 <xsl:value-of select="$OPT_TYP"/>
 <xsl:value-of select="$seperator"/>
<!-- Option Style -->
<xsl:call-template name="EUR_US_FLG"/>
<xsl:value-of select="$seperator"/>
<!-- Option Type -->
<xsl:value-of select="$option_type"/>
<xsl:value-of select="$seperator"/>
<!-- Barrier1 -->
<xsl:call-template name="barrier_1"/>
<xsl:value-of select="$seperator"/>
<!-- Barrier2 -->
<xsl:call-template name="barrier_2"/>
<xsl:value-of select="$seperator"/>
<!-- Currency1 For FX options only-->
<xsl:value-of select="$Currency1"/>
<xsl:value-of select="$seperator"/>
<!-- Notional1 For FX options only -->
<xsl:value-of select="$Notional1"/>
<xsl:value-of select="$seperator"/>
<!-- Buy/Sell For FX Options only -->
<xsl:value-of select="$BuySell1"/>
<xsl:value-of select="$seperator"/>
<!-- Currency2 For FX Options only -->
<xsl:value-of select="$Currency2"/>
<xsl:value-of select="$seperator"/>
<!-- Notional2 For FX Options only -->
<xsl:value-of select="$Notional2"/>
<xsl:value-of select="$seperator"/>
<!-- Buy/Sell For FX Options only -->
<xsl:value-of select="$BuySell2"/>
<xsl:value-of select="$seperator"/>
<!-- Premium -->
<xsl:value-of select="$premium"/>
<xsl:value-of select="$seperator"/>
<!-- Premium Currency -->
<xsl:value-of select="$premium_ccy"/>
<xsl:value-of select="$seperator"/>
<!-- Premium settlement Date     -->
 <xsl:call-template name="settlement_date"/>
 <xsl:value-of select="$seperator"/>
<!-- Quantity                    -->
<xsl:value-of select="$quantity"/>
<xsl:value-of select="$seperator"/>
<!-- Spot price                  -->
<xsl:value-of select="$trade_spot"/>
<xsl:value-of select="$seperator"/>
<!-- Strike Price                -->
<xsl:value-of select="$strike_price"/>
<xsl:value-of select="$seperator"/>
<!-- Trade ID                    -->
<xsl:value-of select="$trade_identifier"/>
<xsl:value-of select="$seperator"/>
<!-- New/Cancel                  -->
<xsl:call-template name="NEW-CANC"/>
<xsl:value-of select="$seperator"/>
<!-- Opening/Closing             -->
<xsl:value-of select="$op_cl"/>
<xsl:value-of select="$seperator"/>
<!-- Trade Time                  -->
<xsl:call-template name="trade_time"/>
<xsl:value-of select="$seperator"/>
<!-- Original trade ID           -->
<xsl:value-of select="$original_trade_id"/>
<xsl:value-of select="$seperator"/>
<!-- Manager Code                -->
<xsl:value-of select="$MGP"/>
<xsl:value-of select="$seperator"/>
<!-- Original trade ID           -->
<xsl:value-of select="$seperator"/>
<!-- Manager Code                -->
<xsl:value-of select="$seperator"/>

</xsl:template>

</xsl:stylesheet>