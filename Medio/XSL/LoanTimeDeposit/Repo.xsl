<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:ms="urn:schemas-microsoft-com:xslt" 
                xmlns:ns0="http://www.sophis.net/bo_xml" xmlns:ns1="http://www.sophis.net/otc_message" xmlns:ns2="http://www.sophis.net/trade" xmlns:ns3="http://www.sophis.net/party" xmlns:ns4="http://www.sophis.net/folio" xmlns:ns5="http://sophis.net/sophis/common" xmlns:ns6="http://www.sophis.net/Instrument" xmlns:ns7="http://www.sophis.net/SSI" xmlns:ns8="http://www.sophis.net/NostroAccountReference" xmlns:api="urn:internal-api"
                exclude-result-prefixes="ns0 ns1 ns2 ns3 ns4 ns5 ns6 ns7 ns8 api"
>

  <xsl:output method="text" indent="no"/>
<!-- -->
  <ms:script language="C#" implements-prefix="api">  

  <ms:assembly name="SophisDotNetToolkit"/>
    <ms:assembly name="Sophis.Core"/>
    <ms:assembly name="Sophis.Core.Data"/>
    <ms:assembly name="System.Data"/>
    <ms:assembly name="System.Windows.Forms"/>
    <ms:assembly name="Oracle.DataAccess"/>
    <ms:using namespace="sophis" />
    <ms:using namespace="sophis.portfolio" />
    <ms:using namespace="sophis.instrument" />
    <ms:using namespace="System.Windows.Forms"/>
    
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
      
      public string getIntAmt(string refcon, string sicovam)
      {
        
            MessageBox.Show("refcon = " + refcon + " sicovam = " + sicovam);
            //return "0";
            /**/
            try
            {
                double result = 0;

                int i_refcon = 0;
                int i_sicovam = 0;
                if (Int32.TryParse(refcon, out i_refcon) == false || Int32.TryParse(sicovam, out i_sicovam) == false)
                {
                    return result.ToString();
                }

                CSMTransaction trans = CSMTransaction.newCSRTransaction(i_refcon);
                CSMInstrument instrument = CSMInstrument.GetInstance(i_sicovam);
                if (instrument != null)
                {
                    int startDate = trans.GetPariPassuDate();

                    CSMDebtInstrument debtInstrument = instrument;
                    CSMLoanAndRepo repoInstrument = instrument;
                    
                    if (repoInstrument != null)
                    {
                        int floatingRateOnCommission = repoInstrument.GetFloatingRateOnCommission();
                        result = trans.GetCommission();
                        result += CSMInstrument.GetLast(floatingRateOnCommission);
                        MessageBox.Show("Commission tr " +  trans.GetCommission() + " comm instrum " + CSMInstrument.GetLast(floatingRateOnCommission));
                        
                    }
                    else
                    {
                        result = 0.117;
                    }
                    
                    //result = trans.GetAccruedAmount() + trans.GetAccruedCoupon() + trans.GetCommission() * trans.GetGrossAmount() / 100.0;
                    
                }

                return result.ToString();
            }
            catch (Exception e)
            {
                return e.Message;
            }
            /**/
      }
    
   ]]>

</ms:script> 

 <!-- -->

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
  
  <xsl:template name="MGP">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:repo/ns2:nostroCash/ns7:accountAtCustodian"/>
  </xsl:template>

  <!-- Instrument -->
  
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
 
  <!-- Accounts -->
  <xsl:variable name="MGP">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:fxSwap/ns2:fxLeg1/ns2:nostroSecurity/ns7:nostroAccountReference/ns8:accountAtCustodian"/>
  </xsl:variable>

  <!-- Economics -->
  <xsl:variable name="amt">
    <!-- <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount,'#.####')"/> -->
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:repo/ns2:cashPrincipal/ns2:amount/ns6:amount,'#.####')"/>
  </xsl:variable>
  
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
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:instrument/ns5:equityFuture/ns5:contractSize, '#.####')"/>
  </xsl:variable>

  <xsl:template name="trade_settlement_ccy">
    <!-- <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:currency"/> -->
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:repo/ns2:cashPrincipal/ns2:amount/ns6:currency"/>
  </xsl:template>

  <xsl:template name="currency">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/*/ns6:currency"/>
  </xsl:template>
  
  <xsl:template name="trade_forex">
    <xsl:if test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex">
       <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex,'0.######')"/>
    </xsl:if>
   
  </xsl:template>
  
  <xsl:variable name="rate">
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:repo/ns2:repoCommission/ns2:interest/ns6:floating/ns6:spread">
            <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:repo/ns2:repoCommission/ns2:interest/ns6:floating/ns6:spread, '#.####')"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/*/ns5:interestType/*/ns5:nominalRate"/>
        </xsl:otherwise>
    </xsl:choose>
    
  </xsl:variable>

  <xsl:variable name="base_n_days">
    <xsl:variable name="raw_base" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:repo/ns2:repoCommission/ns2:interest/*/ns6:dayCountBasis"/>
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
   <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityFuture/ns5:currency"/>
  </xsl:template>
  
  <xsl:template name="set-net-amt">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount"/>
  </xsl:template>
  
  <xsl:template name="nostro_pset_id">
    <xsl:variable name="pset_id" select="//ns3:advancedData/ns3:settlementInstruction/ns3:ssiPathId[text()=/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:ssiPathId]/../ns3:settlementPlace"/>
    <!-- <xsl:value-of select="api:getPlaceOfSettlementBIC($pset_id)"/> -->
  </xsl:template>

  <xsl:template name="BRK-CUST-REF">
    <!--<xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:custodian"/> -->
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:repo/ns2:lostroCash/ns7:custodian"/>
  </xsl:template>

  <xsl:template name="BEN-REF-SAFE">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:repo/ns2:lostroCash/ns7:accountAtCustodian"/>
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
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/ns5:equityFuture/ns5:expirationDate, 'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="value_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:valuedate, 'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="redemption_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/ns5:debtInstrument/ns5:cashFlowGeneration/ns5:redemptionDate, 'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="end_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:repo/ns2:termination/ns2:endDate/ns5:unadjustedDate,'yyyyMMdd')"/>
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
  
  <xsl:template name="ope_typ" >
            <xsl:value-of select="'DEP'"/>
  </xsl:template>
  
  
  <xsl:variable name="brk_ref">
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker">
            <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']"/>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="brk_des">
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker">
            <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']"/>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
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
  
  <xsl:variable name="ben_ref_safe">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:entity/ns3:party/ns3:description/ns3:advancedData/ns3:settlementInstruction/ns3:accountCustodian"/>
  </xsl:variable>

  <xsl:variable name="int_amt">
    <xsl:variable name="refcon" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:tradeId[@ns2:tradeIdScheme='http://www.sophis.net/trade/tradeId/id']"/>
    <xsl:variable name="sicovam" select="/ns0:sph_otc_message/ns0:instrument/ns6:instrument/ns6:identifier/ns6:sophis"/>
    <xsl:value-of select="api:getIntAmt($refcon, $sicovam)"/>
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
  <!-- OPE-TYP: LOA/DEP-->
  <xsl:call-template name="ope_typ"/>
  <xsl:value-of select="$seperator"/>
  <!-- MGP -->
  <xsl:call-template name="MGP"/>
  <xsl:value-of select="$seperator"/>
  <!-- TRA-DAT -->
  <xsl:call-template name="trade_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- SET-DAT -->
  <xsl:call-template name="settlement_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- END-DAT -->
  <xsl:call-template name="end_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- AMT [@Sean: is this correctly retrieved?] -->
  <xsl:value-of select="$amt"/>
  <xsl:value-of select="$seperator"/>
  <!-- CUR [CHECK] -->
  <xsl:call-template name="currency"/>
  <xsl:value-of select="$seperator"/>
  <!-- SET-CUR -->
  <xsl:call-template name="trade_settlement_ccy"/>
  <xsl:value-of select="$seperator"/>
  <!-- INT-AMT [MISSING INFO] - accrued? -->
  <xsl:value-of select="$int_amt"/>
  <xsl:value-of select="$seperator"/>
  <!-- RATE -->
  <xsl:value-of select="$rate"/>
  <xsl:value-of select="$seperator"/>
  <!-- BASE-N-DAYS -->
  <xsl:value-of select ="$base_n_days"/>
  <xsl:value-of select="$seperator"/>
  <!-- BRK-REF [TODO] the broker reference is not a swift/bic code in the xml file. Get it from the DB instead? -->
  <xsl:value-of select="$brk_ref"/>
  <xsl:value-of select="$seperator"/>
  <!-- BRK-DES -->
  <xsl:value-of select="$brk_des"/>
  <xsl:value-of select="$seperator"/>
  <!-- BRK-CUST-REF [TODO][CHECK] Is this ok? -->
  <!--<xsl:value-of select="$brk_ref"/>-->
  <xsl:call-template name="BRK-CUST-REF"/>
  <xsl:value-of select="$seperator"/>
  <!-- BRK-CUST-LIB -->
  <xsl:value-of select="$seperator"/>
  <!-- BRK-CUST-SAFE -->
  <xsl:value-of select="$seperator"/>
  <!-- BRK-CUST-CITY -->
  <xsl:value-of select="$seperator"/>
  <!-- BRK-CUST-NSCS -->
  <xsl:value-of select="$seperator"/>
  <!-- BRK-INT-CUS-REF -->
  <xsl:value-of select="$seperator"/>
  <!-- BRK-INT-CUS-LIB -->
  <xsl:value-of select="$seperator"/>
  <!-- BRK-INT-CUS-SAF -->
  <xsl:value-of select="$seperator"/>
  <!-- BRK-INT-CUS-CIT -->
  <xsl:value-of select="$seperator"/>
  <!-- BRK-INT-CUS-NCS -->
  <xsl:value-of select="$seperator"/>
  <!-- RBCISCUS-REF -->
  <xsl:value-of select="$seperator"/>
  <!-- RBCIS-CUS-LIB -->
  <xsl:value-of select="$seperator"/>
  <!-- RBCIS-CUS-SAFE -->
  <xsl:value-of select="$seperator"/>
  <!-- RBCIS-CUS-CITY -->
  <xsl:value-of select="$seperator"/>
  <!-- RBCIS-CUS-NCSC -->
  <xsl:value-of select="$seperator"/>
  <!-- FM-TXT -->
  <xsl:value-of select="$seperator"/>
  <!-- BEN-REF-SAFE -->
  <xsl:call-template name="BEN-REF-SAFE"/>
  <!--<xsl:value-of select="$ben_ref_safe"/>-->
  <xsl:value-of select="$seperator"/>
  <!-- BEN-REF-NCSC -->
  <!-- - - - - - - -  -->
  
  
</xsl:template>

</xsl:stylesheet>