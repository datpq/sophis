<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:ms="urn:schemas-microsoft-com:xslt" 
                xmlns:ns0="http://www.sophis.net/bo_xml" xmlns:ns1="http://www.sophis.net/otc_message" xmlns:ns2="http://www.sophis.net/trade" xmlns:ns3="http://www.sophis.net/party" xmlns:ns4="http://www.sophis.net/folio" xmlns:ns5="http://www.sophis.net/Instrument" xmlns:ns6="http://www.sophis.net/SSI" xmlns:ns7="http://www.sophis.net/NostroAccountReference" xmlns:ns8="http://sophis.net/sophis/common" xmlns:api="urn:internal-api"
                exclude-result-prefixes="ns0 ns1 ns2 ns3 ns4 ns5 ns6 ns7 ns8 api"
>

<!-- API Function definition -->

<!-- -->

<ms:script language="C#" implements-prefix="api">

<ms:assembly name="SophisDotNetToolkit"/>
<ms:assembly name="Sophis.Core"/>
<ms:assembly name="Sophis.Core.Data"/>


<ms:assembly name="System.Data"/>
<ms:assembly name="Oracle.DataAccess, Version=2.112.1.0, Culture=neutral, PublicKeyToken=89b483f429c47342"/>


    <ms:assembly name="System.Windows.Forms"/>


    <ms:using namespace="System.Windows.Forms"/>
    
<![CDATA[


public class ConnectionManager
{
    public static Oracle.DataAccess.Client.OracleConnection con = null; 
    public static void Connect() 
    { 
        con = new Oracle.DataAccess.Client.OracleConnection(); 
        con.ConnectionString = "User Id=SANDBOX_71311;Password=PASS;Data Source=PARQA02"; 
        con.Open();
        Console.WriteLine("Connected to Oracle" + con.ServerVersion); 
    }

    public static void Close() 
    {
        con.Close(); 
        con.Dispose(); 
    } 

	public static Oracle.DataAccess.Client.OracleConnection GetInstance()
	{
		if(con == null)
		{
			Connect();
		}
		return con;
	}
}

public string retrieveSQLData(String SQLQuery)
{
	try
	{
		Oracle.DataAccess.Client.OracleCommand cmdOracle = new Oracle.DataAccess.Client.OracleCommand(SQLQuery, Sophis.DataAccess.DBContext.Connection /*ConnectionManager.GetInstance()*/);  
		Oracle.DataAccess.Client.OracleDataReader reader = cmdOracle.ExecuteReader();
		string SQLresult = "";
		if(reader.Read())
		{
			SQLresult = reader[0].ToString();
		}
		reader.Close(); 
		return SQLresult;
	}
	catch(Exception e)
	{
		return e.Message;
	}
}

public string getCount()
{

	return retrieveSQLData("SELECT COUNT(*) FROM HISTOMVTS");
}

public string getCDSSpread(string entitysicovam)
{
	return retrieveSQLData("select def_cdsrate from credit_model where code = " + entitysicovam );
}

public string getOpIncCl(string posId)
{
    string op_inc_cl = retrieveSQLData("select decode(count(refcon),1,'OP',decode(sum(quantite),0,'CL','INC')) from histomvts where mvtident= to_number('" + posId +"')");
    MessageBox.Show("select decode(count(refcon),1,'OP',decode(sum(quantite),0,'CL','INC')) from histomvts where mvtident=" + posId);
    
    return op_inc_cl;
}

]]>


</ms:script>

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
		reader.Close(); 
		return SQLresult;
	}
	catch(Exception e)
	{
		return e.Message;
	}
}

public string getCountHistomvts()
{
	
	return retrieveSQLData("SELECT COUNT(*) FROM HISTOMVTS");
}

public string getCDSSpread(string entitysicovam)
{
	return retrieveSQLData("select def_cdsrate from credit_model where code = " + entitysicovam );
}

]]>


</ms:script>
-->

  <xsl:output method="text" indent="no"/>
  

  

  
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
  <xsl:variable name="cpty_name" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']"/>
  
  <xsl:variable name="folio">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:extendedPartyTradeInformation/ns4:identifierAllocationRule/ns4:folio/ns4:portfolioName[@ns4:portfolioNameScheme='http://www.sophis.net/folio/portfolioName/name']"/>
  </xsl:variable>
  
  <xsl:variable name="folio_id">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:accountAtCustodian"/>
  </xsl:variable>
  <!-- Instrument -->
  
  <xsl:variable name="deal_type">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:pricing/ns5:family"/> 
  </xsl:variable>
  <!--
  <xsl:variable name="fund_position">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:swapTransactionType"/>
  </xsl:variable>
  -->
  
  <xsl:template name="sec-des">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityFuture/ns5:name"/>
  </xsl:template>
  
  <xsl:template name="CDS_TYPE">
    <xsl:variable name="underlyer" select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:obligations/ns5:underlyer/ns5:reference"/>
    <xsl:choose>
        <xsl:when test="contains($underlyer, 'index') or contains($underlyer, 'idx') or contains($underlyer, 'indx')">
            <xsl:text>INDEX</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>SINGLE</xsl:text>
        </xsl:otherwise>
    </xsl:choose>
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
  
  <xsl:template name="reference_entity_name">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:position/ns4:position/ns4:underlyer/ns5:reference[@ns5:name='Markit']"/>
  </xsl:template>
  
  <xsl:variable name="underlying_id"> <!-- possible path for underlying ISIN -->
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:obligations/ns5:underlyer/ns5:reference[@ns5:name='Ticker']"/>
  </xsl:variable>
  <xsl:variable name="underlying_red">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:obligations/ns5:underlyer/ns5:reference[@ns5:name='ISIN']"/>
  </xsl:variable>
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
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:currency"/>
  </xsl:template>

  <xsl:template name="trade_forex">
    <xsl:if test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex">
       <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:forex,'0.######')"/>
    </xsl:if>
   
  </xsl:template>
  
  <xsl:variable name="rate_spread">
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:fixedRateSchedule/ns5:initialValue">
             <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:fixedRateSchedule/ns5:initialValue,'#.####')"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:floatingRateCalculation/ns5:initialValue,'#.####')"/>
        </xsl:otherwise>
    </xsl:choose> 
  </xsl:variable>
  
  <xsl:variable name="rate_spread_floating">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:floatingRateCalculation/ns5:spread,'#.####')"/>
  </xsl:variable>

  <xsl:variable name="str_pri">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:strike/ns5:strikeValue/ns5:strikePrice/ns5:amount"/>
  </xsl:variable>
  
  <xsl:variable name="notional">
    <!--<xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:notional/ns5:amount"/>-->
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:notional, '#.####')"/>
  </xsl:variable>
  
  <xsl:variable name="fund_position">
    <xsl:choose>
        <xsl:when test="$notional &gt; 0">
            <xsl:text>BUY</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>SELL</xsl:text>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="instrument_count">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:position/ns4:position/ns4:instrumentCount"/>
  </xsl:variable>
  
  <xsl:variable name="nominal">
    <!--<xsl:value-of select="format-number($notional * $instrument_count, '#.####')"/>-->
    <xsl:value-of select="format-number($notional, '#.####')"/>
  </xsl:variable>
  
  <xsl:variable name="rate_margin">
    <xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:accruedCoupon/ns2:rate,  '#.####')"/>
  </xsl:variable>
  
  <xsl:variable name="period_base">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodFrequency/ns5:periodEnum"/>
  </xsl:variable>
  <xsl:variable name="period_multiplier">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodFrequency/ns5:periodMultiplier"/>
  </xsl:variable>
  
  <!-- Business Day Convention? -->
  <xsl:variable name="day_convention">
    <!-- <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:calculationPeriodDatesAdjustments/ns5:businessDayConvention"/>-->
    <xsl:variable name="dc" select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:generalTerms/ns5:dateAdjustments"/>
    <xsl:choose>
        <xsl:when test="starts-with($dc, 'MODFOLLOWING')">
            <xsl:value-of select="'MODIFIED FOLLOWING'"/>
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

<xsl:variable name="base">
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:dayCountFraction">
            <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:dayCountFraction"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:payingLeg/ns5:swapStream/ns5:calculationPeriodAmount/ns5:calculation/ns5:dayCountFraction"/>
        </xsl:otherwise>
    </xsl:choose>    
</xsl:variable>

<xsl:variable name="calculation_method">
    <xsl:choose>
        <xsl:when test="not(starts-with($base, '30/'))">
            <xsl:value-of select="substring($base, 0, 8)"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="substring($base,0,7)"/>
        </xsl:otherwise>
    </xsl:choose>
</xsl:variable>

<xsl:variable name="settlement_type">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:obligations/ns5:delivery"/>
</xsl:variable>

  <!-- Settlements -->
  <!-- [SA] Take Settlement Currency from the Trade? -->
  <xsl:template name="set-cur">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:equityFuture/ns5:currency"/>
  </xsl:template>
  
  <xsl:template name="set-net-amt">
    <xsl:variable name="absolute_amount" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount"/>
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount[@ns2:negative='true']">
            <xsl:value-of select="-format-number($absolute_amount, '#.######')"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="format-number($absolute_amount, '#.######')"/>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <!--
  <xsl:template name="nostro_pset_id">
    <xsl:variable name="pset_id" select="//ns3:advancedData/ns3:settlementInstruction/ns3:ssiPathId[text()=/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:ssiPathId]/../ns3:settlementPlace"/>
    <xsl:value-of select="api:getPlaceOfSettlementBIC($pset_id)"/>
  </xsl:template>
  -->
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
  <xsl:variable name="value_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:principalSettlement/ns2:valueDate/ns8:unadjustedDate,'yyyyMMdd')"/>
  </xsl:variable>
  <xsl:template name="trade_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:tradeDate,'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="settlement_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:settlementDate,'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="maturity_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:swapStream/ns5:calculationPeriodDates/ns5:terminationDate/ns8:adjustableDate/ns8:unadjustedDate, 'yyyyMMdd')"/>
  </xsl:template>
  
  <xsl:variable name="scheduled_termination_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:generalTerms/ns5:scheduledTerminationDate/ns8:adjustableDate/ns8:unadjustedDate, 'yyyMMdd')"/>
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
  
  <xsl:template name="last_accrual_date_raw">
    <xsl:for-each select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:swapProduct/ns2:principalPayment/ns2:accruedCoupon">
      <xsl:value-of select="ns2:date/ns8:unadjustedDate"/>    								
      <xsl:choose>
        <xsl:when test="position() != last()"></xsl:when>
      </xsl:choose>
    </xsl:for-each>    
  </xsl:template>
  
  <xsl:template name="last_accrual_date">
    <xsl:variable name="raw_date">
        <xsl:call-template name="last_accrual_date_raw"/>
    </xsl:variable>
    <xsl:value-of select="ms:format-date($raw_date,'yyyyMMdd')"/>
 </xsl:template>

 <xsl:template name="TRY_THIS">
    <MY_NODE desc="my description" />
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
  <xsl:template name="seniority">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:obligations/ns5:seniority"/>
  </xsl:template>
  <xsl:template name="underlying_red_pair">
    <!-- The Underlying RED is an indentifer for the underlying instrument/entity that the CDS is hedging against -->
    <!-- RED is an identifer used by MARKIT, like BO.CPH for Bang Olufsen on the Copenhagen Exchange -->
   <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:obligations/ns5:underlyer/ns5:reference[@ns5:name='Markit']/text()"/>
  </xsl:template>

  <xsl:template name="payment_frequency_swap">
    <xsl:variable name="payment_frequency_value" select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:frequency/ns5:periodMultiplier"/>
    <xsl:variable name="payment_frequency_unit" select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:frequency/ns5:periodEnum"/>
    <xsl:choose>
      <xsl:when test="$payment_frequency_unit = 'Month' and $payment_frequency_value = '1'">
        <xsl:text>M</xsl:text>
      </xsl:when>
	  <xsl:when test="$payment_frequency_unit = 'Month' and $payment_frequency_value = '3'">
        <xsl:text>Q</xsl:text>
      </xsl:when>
	  <xsl:when test="$payment_frequency_unit = 'Month' and $payment_frequency_value = '6'">
        <xsl:text>S</xsl:text>
      </xsl:when>
	  <xsl:when test="$payment_frequency_unit = 'Year' and $payment_frequency_value = '1'">
        <xsl:text>A</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text></xsl:text>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:variable name="UTI">
    <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:linkId[@ns2:linkIdScheme='http://www.sophis.net/trade/linkId/usi']">
            <xsl:value-of select="substring-after(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:linkId[@ns2:linkIdScheme='http://www.sophis.net/trade/linkId/usi'], '|')"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:tradeId[@ns2:tradeIdScheme='http://www.sophis.net/trade/tradeId/USI_ident']/text()"/>
        </xsl:otherwise>
    </xsl:choose>
    
  </xsl:variable>
  
   <xsl:variable name="USI">
   <xsl:choose>
        <xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:linkId[@ns2:linkIdScheme='http://www.sophis.net/trade/linkId/usi']">
            <xsl:value-of select="substring-before(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:linkId[@ns2:linkIdScheme='http://www.sophis.net/trade/linkId/usi'], '|')"/>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:tradeId[@ns2:tradeIdScheme='http://www.sophis.net/trade/tradeId/USI_namespace']/text()"/>
        </xsl:otherwise>
    </xsl:choose>
    
  </xsl:variable>
  
  
  <xsl:variable name="entitysicovam" select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:obligations/ns5:refEntity"/>
  <xsl:template name="CDSSpread">
    <xsl:value-of select="api:getCDSSpread($entitysicovam)"/>
  </xsl:template>
  
  <!--
  <xsl:template name="period_count">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:frequency/ns5:periodEnum"/>
  </xsl:template>
  <xsl:template name="period_enum">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/ns5:swap/ns5:receivingLeg/ns5:creditLeg/ns5:frequency/ns5:periodEnum"/>
  </xsl:template>
  -->
  
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
  
  <xsl:variable name="op_inc_cl">
    <xsl:variable name="pos_id" select="/ns0:sph_otc_message/ns0:position/ns4:position/ns4:id"/>
    <xsl:value-of select="api:getOpIncCl($pos_id)"/>    
  </xsl:variable>
  
  <xsl:variable name="seperator" select="';'"/>
 <!--<xsl:variable name="newline" select=""/>-->


<xsl:template match="*">
  <!-- Document Header -->
  <xsl:text>FROM;TO;DATE;TIME;PATH;NAME;EXTNAME;ANSWER1;COUNT &#xa;</xsl:text>
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
  <xsl:text>ACTION; EMETTEUR; EXT-REF; INTERNAL_ORIGID; INTERNAL_ID; INTERNAL_STATUS; EXTERNAL_ORIGID; EXTERNAL_ID; EXTERNAL_STATUS; DATE_OUT; TIME_OUT; ERROR_MESSAGE; Portfolio ID AM; Portfolio Name; Counterparty name (compensation broker); CDS Type; Reference Entity Name; Underlying ID; Underlying RED Pair code; Index Series; Index Version; Scheduled Termination date; Fixed rate/Deal Spread; Busines Days Convention; Payment frequency; Deal Currency; Day Count; Settlement type; Trade ID; NEW/CANC; OP/INC/CL; Trade date; Last Accrual date; Actual spread; Fund position; Notional Amount; Upfron Payer; Payment CCY; Upfront Payment; Upfront Payment Settlement Date; CDS Seniority; Trade Time; Trade Identifier DTCC; Original Trade ID; UTI (Unique trade identifier) &#xa;</xsl:text>
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
  
  <!-- Portfolio ID AM -->
  <xsl:value-of select="$folio_id"/>
  <xsl:value-of select="$seperator"/>
  <!-- Portfolio name -->
  <xsl:value-of select="$folio"/>
  <xsl:value-of select="$seperator"/>
  <!-- Counterparty Name -->
  <xsl:value-of select="$cpty_name"/>
  <xsl:value-of select="$seperator"/>
  <!-- CDS Type [TODO: pending RBC, optional]-->
  <xsl:call-template name="CDS_TYPE"/>
  <xsl:value-of select="$seperator"/>
  <!-- Reference Entity Name -->
  <xsl:call-template name="reference_entity_name"/>
  <xsl:value-of select="$seperator"/>
  <!-- Underlying ID -->
  <xsl:value-of select="$underlying_id"/>
  <xsl:value-of select="$seperator"/>
  <!-- Underlying RED Pair Code [TODO][CHECK]-->
  <xsl:call-template name="underlying_red_pair"/>
  <xsl:value-of select="$seperator"/>
  <!-- Index Series [TODO][NOTFOUND] -->
  <xsl:value-of select="$seperator"/>
  <!-- Index Version [TODO][NOTFOUND] -->
  <xsl:value-of select="$seperator"/>
  <!-- Scheduled Termination Date -->
  <xsl:value-of select="$scheduled_termination_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Fixed rate/deal spread [TODO][CHECK] -->
  <xsl:value-of select="$rate_spread"/>
  <xsl:value-of select="$seperator"/>
  <!-- Business day convention [TODO][CHECK] -->
  <xsl:value-of select="$day_convention"/>
  <xsl:value-of select="$seperator"/>
  <!-- Payment frequency [TODO][CHECK] -->
  <!--<xsl:value-of select="concat($period_multiplier,' ', $period_base)" />-->
  <xsl:call-template name="payment_frequency_swap"/>
  <xsl:value-of select="$seperator"/>
  <!-- Deal currency [TODO][CHECK] -->
  <xsl:call-template name="trade_settlement_ccy"/>
  <xsl:value-of select="$seperator"/>
  <!-- Day count -->
  <xsl:value-of select="$calculation_method"/>
  <xsl:value-of select="$seperator"/>
  <!-- Settlement Type -->
  <xsl:value-of select="$settlement_type"/>
  <xsl:value-of select="$seperator"/>
  <!-- Trade ID -->
  <xsl:call-template name="tradeid"/>
  <xsl:value-of select="$seperator"/>
  <!-- NEW/CANC [TODO][NOT FOUND]-->
  <xsl:call-template name="NEW-CANC"/>
  <xsl:value-of select="$seperator"/>
  <!-- OP/INC/CL [TODO][NOT FOUND] -->
  <xsl:value-of select="$op_inc_cl"/>
  <xsl:value-of select="$seperator"/>
  <!-- Trade Date -->
  <xsl:call-template name="trade_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Last Accrual Date  [TODO][CHECK]-->
  <xsl:call-template name="last_accrual_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- Actual spread [TODO][NOTFOUND]-->
  <xsl:call-template name="CDSSpread"/>
  <xsl:value-of select="$seperator"/>
  <!-- Fund Position [TODO][NOTFOUND] -->
  <xsl:value-of select="$fund_position"/>
  <xsl:value-of select="$seperator"/>
  <!-- Notional Amount -->
  <xsl:value-of select="$notional"/>
  <xsl:value-of select="$seperator"/>
  <!-- Upfront Payer [TODO][CHECK] -->

  <xsl:value-of select="$seperator"/>
  <!-- Payment CCY [TODO][CHECK]-->
  <xsl:call-template name="trade_settlement_ccy"/>
  <xsl:value-of select="$seperator"/>
  <!-- Upfront Payment Amount -->
  <xsl:call-template name="set-net-amt"/>
  <xsl:value-of select="$seperator"/>
  
  <!-- Upfront Payment settlement date -->
  <xsl:call-template name="settlement_date"/>
  <xsl:value-of select="$seperator"/>
  <!-- CDS Seniority [CHECK]-->
  <xsl:call-template name="seniority"/>
  <xsl:value-of select="$seperator"/>
  <!-- Trade time [TODO][CHECK]-->
  <xsl:call-template name="trade_time"/>
  <xsl:value-of select="$seperator"/>
  <!-- Trade identifier [TODO][NOT FOUND] -->
  <!-- [SA] Lets reference the messageID here, see below -->
  <xsl:value-of select="$trade_identifier"/>
  <xsl:value-of select="$seperator"/>
  <!-- Original trade ID [TODO][NOT FOUND] -->
  <!-- [SA] I think if this was a Cancel message we would need to reference the trade id of the previous trade. Since SOphis trade id's dont change from one version to a next, lets just reference the messageid found in reversalLinkID -->
  <xsl:value-of select="$original_trade_id"/>
  <xsl:value-of select="$seperator"/>
  <!-- UTI (Unique trade identifier) -->
  <xsl:value-of select="$UTI"/>
  <xsl:value-of select="$seperator"/>
  <!-- ************ -->
  
  <!-- - - - - - -->
  
</xsl:template>

</xsl:stylesheet>