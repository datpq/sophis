<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:ms="urn:schemas-microsoft-com:xslt" 
                xmlns:ns0="http://www.sophis.net/bo_xml" xmlns:ns1="http://www.sophis.net/otc_message" xmlns:ns2="http://www.sophis.net/trade" xmlns:ns3="http://www.sophis.net/party" xmlns:ns4="http://www.sophis.net/folio" xmlns:ns5="http://www.sophis.net/Instrument" xmlns:ns6="http://sophis.net/sophis/common" xmlns:ns7="http://www.sophis.net/SSI" xmlns:ns8="http://www.sophis.net/NostroAccountReference" xmlns:api="urn:internal-api"
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
  
  <xsl:variable name="ben_ref">
    <xsl:variable name="broker" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']"/>
    <xsl:variable name="cpty" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']"/>
  <xsl:choose>
    <xsl:when test="not($broker)">
      <xsl:value-of select="$cpty"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="$broker"/>
    </xsl:otherwise>
  </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="ben_ref_lib">
    <xsl:variable name="broker_name" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']"/>
    <xsl:variable name="cpty_name" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']"/>
  <xsl:choose>
    <xsl:when test="not($broker_name)">
      <xsl:value-of select="$cpty_name"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="$broker_name"/>
    </xsl:otherwise>
  </xsl:choose>
  </xsl:variable>

  <!-- Instrument -->
  
  <xsl:template name="instrument_ref">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:instrument/ns5:reference[@ns5:name='Ticker']"/>
  </xsl:template>

  <xsl:template name="instrument_isin">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:instrument/ns5:reference[@ns5:name='Isin']"/>
  </xsl:template>

  <xsl:template name="instrument_market">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:share/ns5:market/ns5:sophis"/>
  </xsl:template>
 

  <!-- Economics -->
  <xsl:template name="trade_qty">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:numberOfSecurities,'#')"/>
  </xsl:template>
  <xsl:template name="trade_gross">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount,'#.####')"/>
  </xsl:template>
  <xsl:template name="trade_net">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:amount,'#.####')"/>
  </xsl:template>
  <xsl:template name="trade_spot">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:spot,'#.####')"/>
  </xsl:template>

  <xsl:template name="trade_settlement_ccy">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:currency"/>
  </xsl:template>

  <xsl:template name="trade_forex">
	<xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:exchangeRate/ns2:rate,'#.####')"/>
  </xsl:template>

  <!-- Fees -->
  <xsl:template name="market_fees">
    <xsl:if test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/brokerFees']/ns2:paymentAmount/ns5:amount">
      <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/marketFees']/ns2:paymentAmount/ns5:amount"/>
    </xsl:if>
  </xsl:template>

  <xsl:template name="broker_fees">
    <xsl:if test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/brokerFees']/ns2:paymentAmount/ns5:amount">
      <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/brokerFees']/ns2:paymentAmount/ns5:amount,'#.######')"/>
    </xsl:if>
  </xsl:template>

  <xsl:template name="counterparty_fees">
    <xsl:if test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/brokerFees']/ns2:paymentAmount/ns5:amount">
      <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/couterpartyFees']/ns2:paymentAmount/ns5:amount,'#.######')"/>
    </xsl:if>
  </xsl:template>


  <!-- Settlements -->
  <xsl:template name="nostro_pset_id">
    <xsl:value-of select="//ns3:advancedData/ns3:settlementInstruction/ns3:ssiPathId[text()=/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:ssiPathId]/../ns3:settlementPlace"/>
  </xsl:template>

  <xsl:template name="lostro_custodian_bic">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:custodian"/>
  </xsl:template>

  <xsl:template name="lostro_custodian_acc">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:accountAtCustodian"/>
  </xsl:template>


  <xsl:template name="lostro_agent_bic">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:lostroCash/ns7:agentCode"/>
  </xsl:template>

  <xsl:template name="lostro_agent_acc">
  
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:lostroCash/ns7:accountAtAgent"/>
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
  
  <xsl:variable name="ben_ref_safe">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:lostroSecurity/ns7:accountAtCustodian"/>
  </xsl:variable>


  <!-- Times and Dates -->
  <xsl:template name="trade_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:tradeDate,'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="settlement_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:settlementDate,'yyyyMMdd')"/>
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

  <xsl:template name="FileName" match="*">
    <xsl:call-template name="action"/>
    <xsl:call-template name="tradeid"/>
    <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:ident"/>
  </xsl:template>

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
  
  <xsl:template name="BRK-REF">
    <xsl:variable name="broker" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/externalReference']/text()"/>
    <xsl:variable name="cpty" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/externalReference']/text()"/>
    <xsl:choose>
		<xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']/text()='EXECUTION'">
		  <xsl:value-of select="$broker"/>
		</xsl:when>
		<xsl:otherwise>
		  <xsl:value-of select="$cpty"/>
		</xsl:otherwise>
    </xsl:choose>
  </xsl:template>  

  <xsl:variable name="seperator" select="';'"/>
 <!--<xsl:variable name="newline" select=""/>-->
<!-- Variable names -->
  <xsl:template name="buy_currency">
      <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:fxLegBuySellInfo/ns2:Buy"/>
  </xsl:template>
  <xsl:template name="sell_currency">
      <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:fxLegBuySellInfo/ns2:Sell"/>
  </xsl:template>
  <xsl:template name="SEL-AMT">
   <xsl:variable name="sel_cur" select="//ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:fxLegBuySellInfo/ns2:Sell"/>
   <xsl:choose>
    <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:exchangedCurrency1/ns2:paymentAmount/ns5:currency=$sel_cur">
        <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:exchangedCurrency1/ns2:paymentAmount/ns5:amount,'#.####')"/>
    </xsl:when>
    <xsl:otherwise>
        <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:exchangedCurrency2/ns2:paymentAmount/ns5:amount,'#.####')"/>
    </xsl:otherwise>
   </xsl:choose>
  </xsl:template>
    <xsl:template name="BUY-AMT">
   <xsl:variable name="buy_cur" select="//ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:fxLegBuySellInfo/ns2:Buy"/>
   <xsl:choose>
    <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:exchangedCurrency1/ns2:paymentAmount/ns5:currency=$buy_cur">
        <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:exchangedCurrency1/ns2:paymentAmount/ns5:amount,'#.####')"/>
    </xsl:when>
    <xsl:otherwise>
        <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:exchangedCurrency2/ns2:paymentAmount/ns5:amount,'#.####')"/>
    </xsl:otherwise>
   </xsl:choose>
  </xsl:template>
  
  <xsl:template name="user_id">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:userid/ns1:name"/>
  </xsl:template>
  <xsl:template name="operation_type">
	  <xsl:choose>
			<xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:contractType = 'Spot'">
				<xsl:text>FGX</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>FWD</xsl:text>
			</xsl:otherwise>
	  </xsl:choose>
  </xsl:template>
  <xsl:template name="broker_ext_ref">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:entity/ns3:party/ns3:description/ns3:names/ns3:externalReference"/>
  </xsl:template>
  <xsl:template name="depositary_ext_ref">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:depositary/ns3:party/ns3:description/ns3:names/ns3:externalReference"/>
  </xsl:template>
  
  <xsl:template name="depositary_swift_buy">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:sender/ns1:field2"/>
  </xsl:template>
  <xsl:template name="depositary_reference_buy">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:sender/ns1:field3"/>
  </xsl:template>
  <xsl:template name="depositary_account_buy">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:sender/ns1:account1"/>
  </xsl:template>
  <!--
  <xsl:template name="depositary_town_buy">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:receiver/ns1:account1"/>
  </xsl:template>/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:messagetype/ns1:name
  -->
  <xsl:template name="depositary_rbcis_buy">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:sender/ns1:field1"/>
  </xsl:template>
  
  
  <xsl:template name="depositary_swift_sell">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:receiver/ns1:field2"/>
  </xsl:template>
  <!--
  <xsl:template name="depositary_designation_sell">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:receiver/ns1:account1"/>
  </xsl:template>
  -->
  <xsl:template name="depositary_account_sell">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:receiver/ns1:account1"/>
  </xsl:template>
  <xsl:template name="depositary_rbcis_sell">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:receiver/ns1:field1"/>
  </xsl:template>
  
  <xsl:template name="BEN-REF">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment/ns2:receiverPartyReference/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']"/>
  </xsl:template>
  
  <xsl:template name="BEN-REF-LIB">
    <xsl:value-of select="//ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxSwap/ns2:fxLeg1/ns2:exchangedCurrency1/ns2:receiverPartyReference/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/externalReference']"/>
  </xsl:template>
  
  <xsl:variable name="CUS_BEN_REF_SAFE">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxSwap/ns2:fxLeg1/ns2:exchangedCurrency1/ns2:receiverPartyReference/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']"/>
  </xsl:variable>
  
    <xsl:variable name="MGP">
      <!-- [SA] In all cases the Fund Identifier is the accountAtCustodian for the Nostro side. Since fx has multiple legs, we take whatever value for that field we can get -->
        <!-- <xsl:value-of select="//ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxSwap/ns2:fxLeg2/ns2:nostroCash/ns7:nostroAccountReference/ns8:accountId"/> -->
        <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/*//ns2:nostroCash/ns7:nostroAccountReference/ns8:accountAtCustodian[1]/text()"/>
    </xsl:variable> 
  

<xsl:template match="*">



<!-- Begin Forex implementation -->
 <xsl:value-of select="'MEDIO'"/>
 <xsl:value-of select="$seperator"/>
 <xsl:value-of select="'RBC'"/> 
 <xsl:value-of select="$seperator"/>
 <xsl:call-template name="message_date"/>
 <xsl:value-of select="$seperator"/>
 <xsl:call-template name="message_time"/>
 <xsl:value-of select="$seperator"/>
 <!--<xsl:value-of select="'PATH'"/> -->
 <xsl:value-of select="$seperator"/>
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
 <xsl:text>MEDIO</xsl:text>
 <xsl:value-of select="$seperator"/>
 <!-- REF-EXT -->
 <xsl:call-template name="tradeid"/>
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
 <!-- OPE_TYP -->
 <xsl:call-template name="operation_type"/>
 <xsl:value-of select="$seperator"/>
 <!-- MGP nostro account[CHECK]-->
 <!-- <xsl:call-template name="nostro_id"/> -->
 <xsl:value-of select="$MGP"/>
 <!--<xsl:call-template name="MGP"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- SEL-CUR -->
 <xsl:call-template name="sell_currency"/>
 <xsl:value-of select="$seperator"/>
 <!-- SEL-AMT -->
 <xsl:call-template name="SEL-AMT"/>
 <xsl:value-of select="$seperator"/>
 <!-- BUY-CUR -->
 <xsl:call-template name="buy_currency"/>
 <xsl:value-of select="$seperator"/>
 <!-- BUY-AMT -->
 <xsl:call-template name="BUY-AMT"/>
 <xsl:value-of select="$seperator"/>
 <!-- CLI-RATE -->
 <xsl:call-template name="trade_forex"/>
 <xsl:value-of select="$seperator"/>
 <!-- BRK-REF -->
 <xsl:call-template name="BRK-REF"/>
 <xsl:value-of select="$seperator"/>
 <!-- BRK-DES -->
 <xsl:call-template name="BrokerName"/>
 <!--<xsl:call-template name="depositary_ext_ref"/>-->
 <xsl:value-of select="$seperator"/>
 <!-- TRA-DAT -->
 <xsl:call-template name="trade_date"/>
 <xsl:value-of select="$seperator"/>
 <!-- SET-DAT -->
 <xsl:call-template name="settlement_date"/>
 <xsl:value-of select="$seperator"/>
 <!-- INT-TXT -->
 
 <xsl:value-of select="$seperator"/>
 <!-- COM-TXT-->
 <xsl:call-template name="tradeid"/>
 <xsl:value-of select="$seperator"/>
 <!-- FM-TXT -->
 
 <xsl:value-of select="$seperator"/>
 <!-- PAY-INS [MISSING][NEED MORE INFO]-->
 <xsl:value-of select="'Y'"/> 
 <xsl:value-of select="$seperator"/>
 <!-- DEX-CUS-BUY-REF -->
 <!-- <xsl:call-template name="depositary_swift_buy"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- DEX-CUS-BUY-REF-LIB -->
 
 <xsl:value-of select="$seperator"/>
 <!-- DEX-CUS-BUY-REF-SAFE -->
 <!-- <xsl:call-template name="depositary_account_buy"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- DEX-CUS-BUY-REF-CITY -->
 
 <xsl:value-of select="$seperator"/>
 <!-- DEX-CUS-BUY-REF-NCSC -->
 <xsl:call-template name="depositary_reference_buy"/>
 <xsl:value-of select="$seperator"/>
 <!-- DEX-CUS-SALE-REF -->
 <!-- <xsl:call-template name="depositary_swift_sell"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- DEX-CUS-SALE REF-LIB -->
 
 <xsl:value-of select="$seperator"/>
 <!-- DEX-CUS-SALE-REF-SAFE -->
 <!-- <xsl:call-template name="depositary_account_sell"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- DEX-CUS-SALE-REFNCS -->
 <!-- <xsl:call-template name="depositary_rbcis_sell"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- INT-REF [TODO][CHECK] -->
 <!-- <xsl:call-template name="BrokerID"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- INT-REF-LIB [TODO][CHECK] -->
 <!-- <xsl:call-template name="BrokerName"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- CUS-BEN-REF -->
 <xsl:call-template name="depositary_swift_sell"/>
 <!-- <xsl:call-template name="lostro_agent_bic"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- CUS-BEN-LIB -->
 <!--<xsl:call-template name="lostro_agent_acc"/>-->
 <!-- <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:lostroCash/ns7:thirdParty"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- CUS-BEN-REF-NCSC -->
 
 <xsl:value-of select="$seperator"/>
 <!-- CUS-BEN-REF-SAFE -->
 <xsl:call-template name="lostro_agent_acc"/>
 <!--<xsl:value-of select="$CUS_BEN_REF_SAFE"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- BEN-REF [RBC: broker or ctpy id]-->
 <xsl:call-template name="BrokerID"/>
  <!--<xsl:value-of select="$ben_ref"/>-->
 <xsl:value-of select="$seperator"/>
 <!-- BEN-REF-LIB [RBC: broker or ctpy name]-->
 <xsl:call-template name="BrokerName"/>
 <!--<xsl:value-of select="$ben_ref_lib"/>-->
 <!-- <xsl:call-template name="BEN-REF-LIB"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- BEN-REF-SAFE -->
 <!-- <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxLeg/ns2:lostroSecurity/ns7:accountAtCustodian"/> -->
 <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:receiver/ns1:account1"/>
 <xsl:value-of select="$seperator"/>
 <!-- BEN-REF-NCSC -->
 
 <xsl:value-of select="$seperator"/>
 <!-- HDG-SEC-TYP -->
 
 <xsl:value-of select="$seperator"/>
 <!-- HDG-SEC-CODE -->
 
<!-- End Forex implementation -->

</xsl:template>

</xsl:stylesheet>