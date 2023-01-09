<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:ms="urn:schemas-microsoft-com:xslt" 
                xmlns:ns0="http://www.sophis.net/bo_xml" xmlns:ns1="http://www.sophis.net/otc_message" xmlns:ns2="http://www.sophis.net/trade" xmlns:ns3="http://www.sophis.net/party" xmlns:ns4="http://www.sophis.net/folio" xmlns:ns5="http://www.sophis.net/Instrument" xmlns:ns6="http://www.sophis.net/SSI" xmlns:ns7="http://www.sophis.net/NostroAccountReference" xmlns:ns8="http://sophis.net/sophis/common" xmlns:api="urn:internal-api"
                exclude-result-prefixes="ns0 ns1 ns2 ns3 ns4 ns5 ns6 ns7 ns8 api"
>

  <xsl:output method="text" indent="no"/>
<!-- [TODO: uncomment after dev tests]
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
  <xsl:variable name="QTY">
    <xsl:variable name="noSec" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:numberOfSecurities"/>
    <xsl:variable name="instrumentNotional">
        <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:instrument/*/ns5:notional/ns5:amount">
            <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:bond/ns5:notional/ns5:amount"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="'1'"/>
        </xsl:otherwise>
        </xsl:choose>
    </xsl:variable>
    <xsl:value-of select="format-number($noSec * $instrumentNotional, '#.####')"/>
  </xsl:variable>
  
  <xsl:template name="trade_qty">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:numberOfSecurities,'#')"/>
  </xsl:template>
  <xsl:template name="trade_net">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount,'#.####')"/>
  </xsl:template>
  <xsl:template name="trade_gross">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:amount,'#.####')"/>
  </xsl:template>
  <xsl:template name="trade_spot">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:spot,'#.####')"/>
  </xsl:template>
  <xsl:variable name="trs_gro_amt">
   <xsl:variable name="qty" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:numberOfSecurities"/>
   <xsl:variable name="pri">
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:spot[@ns2:type='InAmount']">
            <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:spot"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:spot div 100"/>
        </xsl:otherwise>
    </xsl:choose>
   </xsl:variable>
    <xsl:value-of select="format-number($pri * $qty,'#.##')"/>
  </xsl:variable>

  <xsl:variable name="total_fees">
    <xsl:value-of select="sum(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment/ns2:paymentAmount/ns5:amount)"/>
  </xsl:variable>

  <xsl:variable name="trs_net_amt">
    <xsl:variable name="buy_sel">
      <xsl:call-template name="BuySell"/>
    </xsl:variable>
    <xsl:choose>
      <xsl:when test="buy_sel='BUY'">
        <xsl:value-of select="format-number($trs_gro_amt + $total_fees,'#.##')"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="format-number($trs_gro_amt - $total_fees,'#.##')"/>
      </xsl:otherwise>
    </xsl:choose>

  </xsl:variable>
  
  <xsl:variable name="qte_typ">
      <xsl:choose>
            <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:spot[@ns2:type='InAmount']">
                <xsl:value-of select="'FAMT'"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="'UNIT'"/>
            </xsl:otherwise>
        </xsl:choose>
  </xsl:variable>
  

  <xsl:template name="trade_settlement_ccy">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:currency"/>
  </xsl:template>

  <xsl:template name="trade_forex">
    <xsl:if test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex">
       <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex,'0.######')"/>
    </xsl:if>
   
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
    <xsl:if test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/couterpartyFees']/ns2:paymentAmount/ns5:amount">
      <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:otherPartyPayment[@ns2:paymentTypeScheme='http://www.sophis.net/trade/paymentType/couterpartyFees']/ns2:paymentAmount/ns5:amount,'#.######')"/>
    </xsl:if>
  </xsl:template>


  <!-- Settlements -->
  <xsl:template name="nostro_pset_id">
  <xsl:variable name="ssiPathId" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:ssiPathId"/>
  <!--
    <xsl:variable name="pset_id" select="//ns3:advancedData/ns3:settlementInstruction/ns3:ssiPathId[text()=/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:ssiPathId]/../ns3:settlementPlace"/>
    -->
    <xsl:variable name="pset_id"
    select="/ns0:sph_otc_message/*/ns3:party/ns3:description/ns3:advancedData/ns3:settlementInstruction/ns3:ssiPathId[text()=$ssiPathId]/../ns3:settlementPlace"/>
    <!-- [TODO: uncomment after dev tests <xsl:value-of select="api:getPlaceOfSettlementBIC($pset_id)"/>-->
    <xsl:value-of select="$pset_id"/>
  </xsl:template>

  <xsl:template name="lostro_custodian_bic">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:accountAtCustodian"/>
    <!--<xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:custodian"/>-->
  </xsl:template>

  <xsl:template name="lostro_custodian_acc">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:accountAtCustodian"/>
  </xsl:template>


  <xsl:template name="lostro_agent_bic">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:agentCode"/>
  </xsl:template>

  <xsl:template name="lostro_agent_acc">
    <xsl:value-of select="number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:accountAtAgent)"/>
  </xsl:template>


  <xsl:template name="lostro_clearing_code">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:counterparty/ns3:party/ns3:description/ns3:advancedData/ns3:settlementInstruction/ns3:ssiPathId[text()=/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:ssiPathId]/../ns3:UserColumnsData/ns3:UserColumn[1]/ns3:Value"/>
  </xsl:template>

  <xsl:template name="lostro_clearing_acc">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:counterparty/ns3:party/ns3:description/ns3:advancedData/ns3:settlementInstruction/ns3:ssiPathId[text()=/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:ssiPathId]/../ns3:UserColumnsData/ns3:UserColumn/ns3:ColumnName[text()='Safe Account']/../ns3:Value"/>
  </xsl:template>


  <xsl:template name="nostro_custodian_bic">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:custodian"/>
  </xsl:template>

   <xsl:template name="nostro_custodian_acc">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:accountAtCustodian"/>
  </xsl:template>
  
  <xsl:template name="SUB-REF">
    <xsl:variable name="ssiPathId" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:ssiPathId"/>
    <xsl:value-of select="/ns0:sph_otc_message/ns0:counterparty/ns3:party/ns3:description/ns3:advancedData/ns3:settlementInstruction/ns3:ssiPathId[text()=$ssiPathId]/../ns3:agentCode"/>
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

  <xsl:variable name="BLB_CODE">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/*/ns5:identifier/ns5:reference[@ns5:name='Bloomberg']"/>
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
  <xsl:value-of select="$seperator"/>
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
  <!-- OPE_TYP -->
  <xsl:call-template name="BuySell"/>
  <xsl:value-of select="$seperator"/>
  <!-- MGP -->
  <xsl:call-template name="nostro_id"/>
  <xsl:value-of select="$seperator"/>
  <!-- CASH_COR -->
  <xsl:value-of select="$seperator"/>
  <!-- BRK-REF -->
  <xsl:call-template name="BrokerID"/>
  <xsl:value-of select="$seperator"/>
  <!-- BRK-NAM -->
  <xsl:call-template name="BrokerName"/>
  <xsl:value-of select="$seperator"/>
  <!-- BRK-ACC -->
  <xsl:call-template name="lostro_custodian_bic"/>
  <xsl:value-of select="$seperator"/>
  <!-- SUB-CLR-ACC -->
  <xsl:value-of select="$seperator"/>
  <!-- SUB-CLR-REF -->
  <xsl:value-of select="$seperator"/>
  <!-- SUB-REF -->
  <!--<xsl:call-template name="lostro_agent_bic"/>-->
  <xsl:call-template name="SUB-REF"/>
  <xsl:value-of select="$seperator"/>
  <!-- SUB-DES -->
  <xsl:value-of select="$seperator"/>
  <!-- NIV-CPT1 [blank]-->

  <xsl:value-of select="$seperator"/>
  <!-- CLR-ACC -->
  <xsl:call-template name="lostro_agent_acc"/>
   <!-- SAFE ACCOUNT ACTUALLY - SEE XSL CODE !! <xsl:call-template name="lostro_clearing_acc"/> -->
  <xsl:value-of select="$seperator"/>
  <!-- CLR-CODE -->
  <xsl:call-template name="lostro_clearing_code"/>
  <xsl:value-of select="$seperator"/>
  <!-- NIV-BIC3 [leave blank 26]-->
  <!-- <xsl:call-template name="lostro_custodian_bic"/> -->
  <xsl:value-of select="$seperator"/>
  <!-- NIV-LIB3 [leave blank 27] -->
  <xsl:value-of select="$seperator"/>
  <!-- NIV-CPT3 [leave blank 28] -->
  <!-- <xsl:call-template name="lostro_custodian_acc"/> -->
  <xsl:value-of select="$seperator"/>
  <!-- NIV-CLR-ACC3 [leave blank 29] -->
  <xsl:value-of select="$seperator"/>
  <!-- NIV-CLR-COD3 [leave blank 30] -->
  <xsl:value-of select="$seperator"/>
  <!-- NIV-BIC4 [leave blank 31] -->
  <!-- <xsl:call-template name="lostro_agent_bic"/> -->
  <xsl:value-of select="$seperator"/>
  <!-- NIV-LIB4 [leave blank 32] -->
  <xsl:value-of select="$seperator"/>
  <!-- NIV-CPT4 [leave blank 33] -->
  <!-- <xsl:call-template name="lostro_agent_acc"/> -->
  <xsl:value-of select="$seperator"/>
  <!-- NIV-CLR-ACC4 [leave blank 34] -->
  <xsl:value-of select="$seperator"/>
  <!-- NV-CLR-COD4 [leave blank 35]-->
  <xsl:value-of select="$seperator"/>
  <!-- TRA-DATE -->
  <xsl:call-template name="trade_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- SET-DATE -->
  <xsl:call-template name="settlement_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- TRS-CUR -->
  <xsl:call-template name="trade_settlement_ccy"/>
  <xsl:value-of select="$seperator"/>
  <!-- 
      SEC-TYP 
        IC  ISIN
        GB  SEDOL
        TK  TELEKURS
        US  CUSIP
  -->
  <xsl:value-of select="'IC'"/>
  <xsl:value-of select="$seperator"/>
  <!-- SEC-COD -->
  <xsl:call-template name="instrument_isin"/>
  <xsl:value-of select="$seperator"/>
  <!-- SEC-DES -->
  <xsl:call-template name="instrument_ref"/>
  <xsl:value-of select="$seperator"/>
  <!-- QTY -->
  <xsl:call-template name="trade_qty"/>
  <xsl:value-of select="$seperator"/>
  <!-- PRI -->
  <xsl:call-template name="trade_spot"/>
  <xsl:value-of select="$seperator"/>
  <!-- INT-MOD -->
  <xsl:value-of select="$seperator"/>
  <!-- FEE-CUR -->
  <xsl:call-template name="trade_settlement_ccy"/>
  <xsl:value-of select="$seperator"/>
  <!-- STK-EXC -->
  <xsl:call-template name="instrument_market"/>
  <xsl:value-of select="$seperator"/>
  <!-- SET-CUR -->
  <xsl:call-template name="trade_settlement_ccy"/>
  <xsl:value-of select="$seperator"/>
  <!-- CHG-RAT -->
  <xsl:call-template name="trade_forex"/>
  <xsl:value-of select="$seperator"/>
  <!-- BRK-FEE -->
  <xsl:choose>
    <xsl:when test="$OnMarket = 'TRUE'">
      <xsl:call-template name="broker_fees"/>
    </xsl:when>
    <xsl:when test="$OnMarket = 'FALSE'">
      <xsl:call-template name="counterparty_fees"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:value-of select="'G'"/>
    </xsl:otherwise>
  </xsl:choose>

  <xsl:value-of select="$seperator"/>
  <!-- TAX-FEE -->
  <xsl:value-of select="$seperator"/>
  <!-- OTH-FEE -->
  <xsl:call-template name="market_fees"/>
  <xsl:value-of select="$seperator"/>
  <!-- INT-AMT -->
  <xsl:value-of select="$seperator"/>
  <!-- INT-TAX -->
  <xsl:value-of select="$seperator"/>
  <!-- INT-DAY -->
  <xsl:value-of select="$seperator"/>
  <!-- TRS-GRO-AMT -->
  <xsl:value-of select="$trs_gro_amt"/>
  <!--<xsl:call-template name="trade_gross"/>-->
  <xsl:value-of select="$seperator"/>
  <!-- TRS-NET-AMT -->
  <!--<xsl:call-template name="trade_net"/>-->
  <xsl:value-of select="$trs_net_amt"/>
  <xsl:value-of select="$seperator"/>
  <!-- TRS-SET-AMT -->
  <xsl:call-template name="trade_net"/>
  <xsl:value-of select="$seperator"/>
  <!-- FEE-AMT -->
  <xsl:value-of select="$seperator"/>
  <!-- FM-TXT -->
  <xsl:value-of select="$seperator"/>
  <!-- COM-TXT -->
  <xsl:call-template name="tradeid"/>
  <xsl:value-of select="$seperator"/>
  <!-- 
      EVEN-TYP
        N  Execution
        Y  Announcement
  --> 
  <xsl:value-of select="'N'"/>
  <xsl:value-of select="$seperator"/>
  <!-- PSET-BIC -->
  <xsl:call-template name="nostro_pset_id"/>
  <xsl:value-of select="$seperator"/>
  <!-- PSET-PAYS -->
  <xsl:value-of select="$seperator"/>
  <!-- CUS-FEE -->
  <xsl:value-of select="$seperator"/>
  <!-- SCUS-FEE -->
  <xsl:value-of select="$seperator"/>
  <!-- STAMP-FEE -->
  <xsl:value-of select="$seperator"/>
  <!-- OPC-IO-TOT -->
  <xsl:value-of select="$seperator"/>
  <!-- OPC-IO-REF -->
  <xsl:value-of select="$seperator"/>
  <!-- OPC-IO-ACQ -->
  <xsl:value-of select="$seperator"/>
  <!-- FACTOR -->
  <xsl:value-of select="$seperator"/>
  <!-- QUOT-TYP -->
  <xsl:value-of select="$qte_typ"/>
  <xsl:value-of select="$seperator"/>
  <!-- TYP-TAU -->
  <xsl:value-of select="$seperator"/>
  <!-- VAT -->
  <xsl:value-of select="$seperator"/>
  <!-- SET-FLG -->
  <xsl:value-of select="$seperator"/>
  <!-- 
      DEL-COD 
        A   DVP
        F   FOP
  -->
  <xsl:call-template name="smdt"/>
  <xsl:value-of select="$seperator"/>
  <!-- DEP-FIN-REF -->
  <xsl:value-of select="$seperator"/>
  <!-- DEP-FIN-DES -->
  <xsl:value-of select="$seperator"/>
  <!-- DEP-FIN-ACC -->
  <xsl:value-of select="$seperator"/>
  <!-- PTG-CSH-ACC-TYP -->
  <xsl:value-of select="$seperator"/>
  <!-- PTG-CSH-ACC-NBR -->
  <xsl:call-template name="nostro_id"/>
  <xsl:value-of select="$seperator"/>
  <!-- SPREAD -->
  <xsl:value-of select="$seperator"/>
  <!-- MARKET-CLAIM-OPT-OUT -->
  <xsl:value-of select="$seperator"/>
  <!-- EX-CUM-COUPON -->
  <xsl:value-of select="$seperator"/>
  <!-- PSAF-BIC -->
  <xsl:value-of select="$seperator"/>
  <!-- INDIC1 -->
  <xsl:value-of select="$seperator"/>
  <!-- BLB-CODE -->
  <xsl:value-of select="$BLB_CODE"/>
</xsl:template>

</xsl:stylesheet>