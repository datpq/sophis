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
    <xsl:choose>
        <xsl:when test="contains(/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:optionType,'Call')">
            <xsl:text>CAL</xsl:text>
        </xsl:when>
        <xsl:when test="contains(/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:optionType,'Put')">
            <xsl:text>PUT</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text></xsl:text>
        </xsl:otherwise>
    </xsl:choose>
 </xsl:variable>
 
 <xsl:template name="EUR_US_FLG">
     <xsl:choose>
         <xsl:when test="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:exercise/ns5:europeanExercise">EUR</xsl:when>
         <xsl:otherwise>US</xsl:otherwise>
    </xsl:choose>
 </xsl:template>
 
 <xsl:variable name="UND_SEC_COD">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:underlyer/ns5:reference[@ns5:name='Bloomberg']"/>
 </xsl:variable>
 <xsl:variable name="UND_SEC_TYP" select="'BL'"/>
 <xsl:variable name="UND_SEC_TYPx">
    <xsl:variable name="feature" select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:feature"/>
    <xsl:variable name="underlyer_string" select="substring-after($feature, 'Underlying-')"/>
    <xsl:value-of select="substring-before($underlyer_string,'|')"/>
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
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:strike/ns5:strikeValue/ns5:strikePrice/ns5:amount,'#.######')"/>
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
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount, '#.####')"/>
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
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:tradeDate,'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="settlement_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:settlementDate,'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="maturity_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:exercise/ns5:europeanExercise/ns5:expirationDate/ns8:adjustableDate/ns8:unadjustedDate, 'yyyyMMdd')"/>
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
  <xsl:variable name="seperator" select="';'"/>
 <!--<xsl:variable name="newline" select=""/>-->


<xsl:template match="*">
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
  <xsl:text>&#xa;</xsl:text>

  <!-- Document Body -->
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
  <!-- LNK-TRD [OPTIONAL]-->
  <xsl:value-of select="'N'"/> 
  <xsl:value-of select="$seperator"/>
  <!-- STG-NME [IGNORED] -->
  <xsl:value-of select="$seperator"/>
  <!-- LOT-ID [IGNORED] -->
  <xsl:value-of select="$seperator"/>
  <!-- TRD-NBR [IGNORED] -->
  <xsl:value-of select="$seperator"/>
  <!-- TOT-TRD [IGNORED] -->
  <xsl:value-of select="$seperator"/>
  <!-- OPE_TYP [NEED MORE INFO] business event linked to it WHEN option type -->
  <xsl:value-of select="$OPE_TYP"/>
  <xsl:value-of select="$seperator"/>
  <!-- MGP [NEED MORE INFO] fund id  nostro account number - is this correct?? -->
  <xsl:call-template name="nostro_id"/>
  <xsl:value-of select="$seperator"/>
  <!-- FM-TXT [IGNORED]-->
  <xsl:value-of select="$seperator"/>
  <!-- ISS-TYP [TODO][NEED MORE INFO] Need  to get back to RBC to figure this one out -->
  <xsl:value-of select="'EFM'"/>
  <xsl:value-of select="$seperator"/>
  <!-- SEC-COD -->
  <xsl:call-template name="SEC-COD"/>
  <xsl:value-of select="$seperator"/>
  <!-- ISS-REF [TODO][NEED MORE INFO] not found?-->
  <xsl:value-of select="$seperator"/>
  <!-- INST-CODE [TODO][NEED MORE INFO] -->
  <xsl:value-of select="'B1'"/>
  <!--<xsl:value-of select="$INST_CODE"/>-->
  <xsl:value-of select="$seperator"/>
  <!-- OPT-TYP [TODO][NEED MORE INFO] Only for options-->
  <xsl:value-of select="$OPT_TYP"/>
  <xsl:value-of select="$seperator"/>
  <!-- SEC-DES -->
  <xsl:call-template name="sec-des"/>
  <xsl:value-of select="$seperator"/>
  <!-- TYP-TRT [TODO][NEED MORE INFO] Related to the business event - to be discussed.. -->
  <xsl:value-of select="$TYP_TRT"/>
  <xsl:value-of select="$seperator"/>
  <!-- TRA-DAT -->
  <xsl:call-template name="trade_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- SET-DAT -->
  <xsl:call-template name="settlement_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- MAT-DAT [TODO][Must account for non-european options] -->
  <xsl:call-template name="maturity_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- QTY -->
  <xsl:value-of select="$trade_qty"/>
  <xsl:value-of select="$seperator"/>
  <!-- TRS-CUR -->
  <xsl:call-template name="trade_settlement_ccy"/>
  <xsl:value-of select="$seperator"/>
  <!-- PRI  -->
  <xsl:value-of select="$trade_spot"/>
  <xsl:value-of select="$seperator"/>
  <!-- TRS-GRO-AMT  -->
  <xsl:value-of select="$trade_gross"/>
  <xsl:value-of select="$seperator"/>
  <!-- INI-MAR -->
  <xsl:value-of select="$trade_contract_size"/>
  <xsl:value-of select="$seperator"/>
  <!-- FEE-CUR [TODO][CHECK] -->
  <xsl:call-template name="set-cur"/>
  <xsl:value-of select="$seperator"/>
  <!-- BRK-FEE [TODO][CHECK] -->
  <xsl:choose>
    <xsl:when test="$OnMarket = 'TRUE'">
      <xsl:value-of select="$broker_fees"/>
    </xsl:when>
    <xsl:when test="$OnMarket = 'FALSE'">
      <xsl:value-of select="$counterparty_fees"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="'G'"/>
    </xsl:otherwise>
  </xsl:choose>
  <xsl:value-of select="$seperator"/>
  <!-- CLR-FEE [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- NFA-FEE [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- CUS-FEE [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- FEE-AMT [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- SCUS-FEE [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- TRS-NET-AMT [TODO][CHECK] -->
  <xsl:value-of select="$trade_net"/>
  <xsl:value-of select="$seperator"/>
  <!-- SET-CUR -->
  <xsl:call-template name="set-cur"/>
  <xsl:value-of select="$seperator"/>
  <!-- CHG-RAT [TODO][NOT FOUND] -->
  <xsl:value-of select="$chg_rat"/>
  <xsl:value-of select="$seperator"/>
  <!-- SET-NET-AMT  -->
  <xsl:call-template name="set-net-amt"/>
  <xsl:value-of select="$seperator"/>
  <!-- BRK-REF -->
  <xsl:call-template name="BrokerID"/>
  <xsl:value-of select="$seperator"/>
  <!-- BRK-DES -->
  <xsl:call-template name="brk-des"/>
  <xsl:value-of select="$seperator"/>
  <!-- CLR-BRK-REF [TODO][NEED MORE INFO] -->
  <xsl:call-template name="futures_acc"/>
  <xsl:value-of select="$seperator"/>
  <!-- CLR-BRK-DES -->
  <xsl:value-of select="$seperator"/>
  <!-- INT-REF [TODO: blank, as suggested by RBC] -->
  <!--<xsl:value-of select="$int_ref"/>-->
  <xsl:value-of select="$seperator"/>
  <!-- INT-REF-LIB [TODO: blank, as suggested by RBC] -->
  <!--<xsl:value-of select="$int_ref_lib"/>-->
  <xsl:value-of select="$seperator"/>
  <!-- CUS-BEN-REF [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- CUS-BEN-LIB [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- CUS-BEN-REF-NCSC [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- CUS-BEN-REF-SAFE [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- BEN-REF [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- BEN-REF-SAFE [TODO][NEEDMOREINFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- BEN-REF-LIB [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- BEN-REF-NCSC [TODO][NEED MORE INFO] -->
  <xsl:value-of select="$seperator"/>
  <!-- COMT-TXT [CHECK] -->
  <xsl:call-template name="tradeid"/>
  <xsl:value-of select="$seperator"/>
  <!-- DEAL-TYP -->
  <!-- [SA] Hard code to T, we've no way to link Options Hedging another trade in Sophis -->
  <xsl:value-of select="'T'"/>
  <xsl:value-of select="$seperator"/>
  <!-- EUR_US_FLG  -->
  <!-- this is Option Type, American or Euro. Found in the Instrument -->
  <xsl:call-template name="EUR_US_FLG"/>
  <xsl:value-of select="$seperator"/>
  <!-- STR-PRI -->
  <!-- Strike of the Option. Found in the Instrument. [TODO] Why is the xpath not working here? -->
  <xsl:value-of select="$str_pri"/>
  <xsl:value-of select="$seperator"/>
  <!-- QUO-PLC [TODO][NEED MORE INFO] not in the xml-->
  <xsl:value-of select="$seperator"/>
  <!-- UND_SEC_COD [CHECK] -->
  <!-- Underlying Security of the Option -->
  <xsl:value-of select="$UND_SEC_COD"/>
  <xsl:value-of select="$seperator"/>
  <!-- UND_SEC_TYPE [TODO][NOT FOUND] -->
  <xsl:value-of select="$UND_SEC_TYP"/>
  <!-- Hard code to IC for ISIN -->
  <xsl:value-of select="$seperator"/>
  <!-- UND_SEC_DES [CHECK] -->
  <xsl:value-of select="$und_sec_des"/>
  <xsl:value-of select="$seperator"/>
  <!-- TIC_BBG [TODO][CHECK] -->
  <xsl:call-template name="tic_bbg"/>
  <xsl:value-of select="$seperator"/>
  <!-- TYPE PARTS [TODO] optional-->
</xsl:template>

</xsl:stylesheet>