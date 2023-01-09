<?xml version="1.0" encoding="Windows-1252" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"                
                xmlns="urn:schemas-microsoft-com:office:spreadsheet"
                xmlns:x="urn:schemas-microsoft-com:office:excel"
				xmlns:reporting="http://www.sophis.net/reporting"
				xmlns:msxsl="urn:schemas-microsoft-com:xslt"
				xmlns:ms="urn:schemas-microsoft-com:xslt"
                xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
                xmlns:xlsx="http://www.stylusstudio.com/XSLT/XLSX" 
				exclude-result-prefixes="xlsx ss"
				xmlns:html="http://www.w3.org/TR/REC-html40"
                xmlns:csv="csv:csv"                                
                xmlns:ns0="http://sophis.net/sophis/gxml/dataExchange"
                xmlns:ns1="http://www.fpml.org/2005/FpML-4-2"
                xmlns:ns10="http://www.sophis.net/execution" 
                xmlns:ns2="http://www.sophis.net/trade"
                xmlns:ns3="http://www.sophis.net/party" 
                xmlns:ns4="http://www.sophis.net/folio"
                xmlns:ns5="http://www.sophis.net/Instrument"
                xmlns:ns6="http://www.sophis.net/SSI"
                xmlns:ns7="http://www.sophis.net/NostroAccountReference" 
                xmlns:ns8="http://sophis.net/sophis/common"
                xmlns:ns9="http://www.fixprotocol.org/FIXML-4-4"
                xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                xmlns:api="urn:internal-api">
  
  
  <xsl:output method="xml" encoding="UTF-8" standalone="no" indent="yes"/>

  <ms:script language="C#" implements-prefix="api">  

  <ms:assembly name="SophisDotNetToolkit"/>
  <ms:assembly name="Sophis.Core"/>
  <ms:assembly name="Sophis.Core.Data"/>
  <ms:assembly name="System.Data"/>
  <ms:assembly name="System.Windows.Forms"/>
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
    
      public string GetFolioID(string FundName, int FundID)
      {
        try
        {  
          String result;          
          String SQLQuery = "select IDENT from FOLIO where NAME = 'MAML' start with ident = (select mnemo from titres where code_emet in (select code from tiersproperties where name = '"+FundName+"' and value = "+FundID+")) CONNECT BY mgr = prior ident";
          result = retrieveSQLData(SQLQuery);
          return result;        
        }
        catch(Exception e)
        {
            return e.Message;
        }
      }
      
      public string GetBusinessDay(string TradeDate, string BBGCode)
      {
          string replacedString = TradeDate.Replace(",", "");

          DateTime sophisTradeDate = Convert.ToDateTime(replacedString);
          int currentDate = 0;
          using (sophisTools.CSMDay dateObj = new sophisTools.CSMDay(sophisTradeDate.Day, sophisTradeDate.Month, sophisTradeDate.Year))
          {
            currentDate = dateObj.toLong();
          }
          
          currentDate++;
          
          int instrCode = sophis.instrument.CSMInstrument.GetCodeWithExternalTypeAndRef("BLOOMBERG",BBGCode);          
          sophis.instrument.CSMInstrument inst = sophis.instrument.CSMInstrument.GetInstance(instrCode);          
          int ccyCode = 0;
          
          if(inst!=null)
          {
            ccyCode = inst.GetCurrencyCode();            
            int marketCode = 0;
            marketCode = inst.GetMarketCode();            
            sophis.static_data.CSMMarket market = sophis.static_data.CSMMarket.GetCSRMarket(ccyCode, marketCode);            
            currentDate = market.MatchingBusinessDay(currentDate);
          }
          
          sophisTools.CSMDay lastDate = new sophisTools.CSMDay(currentDate);
          string newDate = lastDate.fYear.ToString() + '-' + lastDate.fMonth.ToString("00") + '-' + lastDate.fDay.ToString("00");
          
          return newDate;
      }
      
      public string datenow()
     {
        return(DateTime.Now.ToString("yyyy'-'MM'-'dd"));
     }

    
   ]]>

</ms:script> 
  
  <xsl:template name="getbusinessDate">
  <xsl:param name="TradeDate"/>
  <xsl:param name="BBGCode"/>
    <xsl:value-of select="api:GetBusinessDay($TradeDate,$BBGCode)"/>
  </xsl:template>
  
  <!--<xsl:call-template name="getbusinessDate" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
        <xsl:with-param name="TradeDate" select="$TradeDate"/>
        <xsl:with-param name="BBGCode" select="$BBGCode"/>
      </xsl:call-template>-->
  	
  
    <xsl:template name="getDate">
      
      <xsl:variable name="TradeDate" select="./value1"/>      
      <xsl:variable name="BBGCode" select="./value7"/>   
      
      <xsl:call-template name="getbusinessDate" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
        <xsl:with-param name="TradeDate" select="$TradeDate"/>
        <xsl:with-param name="BBGCode" select="$BBGCode"/>
      </xsl:call-template>
      
      <!--<xsl:variable name="Day" select="substring-before(substring-after($TradeDate,' '),',')"/>
      <xsl:variable name="Month" select="substring-before($TradeDate,' ')"/>
      <xsl:variable name="Year" select="substring-after(substring-after($TradeDate,','), ' ')"/>
      
      <xsl:choose>
            <xsl:when test="$Month = 'Jan'">
              <xsl:value-of select="concat($Year,'-','01','-',$Day)"/>
            </xsl:when>
            <xsl:when test="$Month = 'Feb'">
              <xsl:value-of select="concat($Year,'-','02','-',$Day)"/>       
            </xsl:when>
            <xsl:when test="$Month = 'Mar'">
              <xsl:value-of select="concat($Year,'-','03','-',$Day)"/>
            </xsl:when>
            <xsl:when test="$Month = 'Apr'"><xsl:value-of select="concat($Year,'-','04','-',$Day)"/></xsl:when>
            <xsl:when test="$Month = 'May'"><xsl:value-of select="concat($Year,'-','05','-',$Day)"/></xsl:when>
            <xsl:when test="$Month = 'Jun'"><xsl:value-of select="concat($Year,'-','06','-',$Day)"/></xsl:when>
            <xsl:when test="$Month = 'Jul'"><xsl:value-of select="concat($Year,'-','07','-',$Day)"/></xsl:when>
            <xsl:when test="$Month = 'Aug'"><xsl:value-of select="concat($Year,'-','08','-',$Day)"/></xsl:when>
            <xsl:when test="$Month = 'Sep'"><xsl:value-of select="concat($Year,'-','09','-',$Day)"/></xsl:when>
            <xsl:when test="$Month = 'Oct'"><xsl:value-of select="concat($Year,'-','10','-',$Day)"/></xsl:when>
            <xsl:when test="$Month = 'Nov'"><xsl:value-of select="concat($Year,'-','11','-',$Day)"/></xsl:when>
            <xsl:when test="$Month = 'Dec'"><xsl:value-of select="concat($Year,'-','12','-',$Day)"/></xsl:when>            
       </xsl:choose>-->
      </xsl:template>
    
    
  <xsl:template name="lines">
  
    
	<!-- correct -->
    <xsl:variable name="CreationID" select="384"/>
    <xsl:variable name="TradeDate" select="./value1"/>
    <xsl:variable name="FundID" select="./value2"/>
    <xsl:variable name="FundName" select="./value3"/>
	  <xsl:variable name="Currency" select="./value4"/>
  	<xsl:variable name="Martket" select="./value5"/>
    <xsl:variable name="BBGCode" select="./value7"/>
    <xsl:variable name="NumberOfSecurities" select="./value8"/>
    <xsl:variable name="Amount" select="./value9"/>
    
    <xsl:text>&#xd;</xsl:text>
    
    <xsl:text disable-output-escaping="yes">  &lt;trade:trade common:persistenceType=&quot;Universal&quot; trade:creationWorkflowEventId=&quot;</xsl:text>
    <xsl:value-of select="$CreationID"/>
    <xsl:text disable-output-escaping="yes">&quot;&gt;</xsl:text>
    
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">    &lt;trade:tradeHeader&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">      &lt;trade:partyTradeIdentifier&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">        &lt;party:partyReference&gt;</xsl:text>
    
    <xsl:text>&#xd;</xsl:text>
    
    <xsl:text disable-output-escaping="yes">          &lt;party:customPartyId party:name=&quot;GS Account&quot; party:partyIdScheme=&quot;http://www.sophis.net/party/partyId/custom&quot;&gt;</xsl:text>
    <xsl:value-of select="$FundID"/>
    <xsl:text disable-output-escaping="yes">&lt;/party:customPartyId&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    
    
    <xsl:text disable-output-escaping="yes">        &lt;/party:partyReference&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>  
    
    <xsl:text disable-output-escaping="yes">      &lt;/trade:partyTradeIdentifier&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>    
    
    <xsl:text disable-output-escaping="yes">      &lt;trade:extendedPartyTradeInformation&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>    
    <xsl:text disable-output-escaping="yes">        &lt;trade:partyReference&gt;</xsl:text>
    
    <xsl:text>&#xd;</xsl:text>  
    <xsl:text disable-output-escaping="yes">          &lt;party:customPartyId party:name=&quot;GS Account&quot; party:partyIdScheme=&quot;http://www.sophis.net/party/partyId/custom&quot;&gt;</xsl:text>
    <xsl:value-of select="$FundID"/>
    <xsl:text disable-output-escaping="yes">&lt;/party:customPartyId&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>  
    <xsl:text disable-output-escaping="yes">        &lt;/trade:partyReference&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>

    <xsl:text disable-output-escaping="yes">        &lt;trade:trader trade:traderScheme=&quot;http://www.sophis.net/user/userId/name&quot;&gt;MANAGER&lt;/trade:trader&gt;</xsl:text>
    
    
    <xsl:text>&#xd;</xsl:text>     
    <xsl:text disable-output-escaping="yes">        &lt;folio:identifierAllocationRule&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>  
    <xsl:text disable-output-escaping="yes">          &lt;folio:folio&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    
    <xsl:text disable-output-escaping="yes">            &lt;folio:portfolioName folio:portfolioNameScheme=&quot;http://www.sophis.net/folio/portfolioName/fullName&quot;&gt;</xsl:text>
    <xsl:value-of select="api:GetFolioID($FundName,$FundID)"/>
    <xsl:text disable-output-escaping="yes">&lt;/folio:portfolioName&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">          &lt;/folio:folio&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">        &lt;/folio:identifierAllocationRule&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>  
    <xsl:text disable-output-escaping="yes">          &lt;trade:businessEvent&gt;</xsl:text>
    <xsl:text>Variation Margin</xsl:text> 
    <xsl:text disable-output-escaping="yes">&lt;/trade:businessEvent&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    
    
    <xsl:text disable-output-escaping="yes">      &lt;trade:origin&gt;Automatic&lt;/trade:origin&gt;</xsl:text>
        
    <xsl:text>&#xd;</xsl:text>  
    <xsl:text disable-output-escaping="yes">      &lt;trade:tradeDate&gt;</xsl:text>
    <xsl:call-template name="getDate"/>
    <xsl:text disable-output-escaping="yes">&lt;/trade:tradeDate&gt;</xsl:text>
    
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">      &lt;trade:settlementDate&gt;</xsl:text>
    <xsl:call-template name="getDate"/>
    <xsl:text disable-output-escaping="yes">&lt;/trade:settlementDate&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">      &lt;trade:tradeTime&gt;</xsl:text>
    <xsl:value-of select="api:datenow()"/>
    <xsl:text disable-output-escaping="yes">&lt;/trade:tradeTime&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">      &lt;trade:paymentDate&gt;</xsl:text>
    <xsl:call-template name="getDate"/>
    <xsl:text disable-output-escaping="yes">&lt;/trade:paymentDate&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">    &lt;/trade:tradeHeader&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>    
    
    <xsl:text disable-output-escaping="yes">    &lt;trade:tradeProduct&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>    
    <xsl:text disable-output-escaping="yes">      &lt;trade:instrument&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>   
   
    <xsl:text disable-output-escaping="yes">        &lt;instrument:reference instrument:modifiable=&quot;UniqueNotPrioritary&quot; instrument:name=&quot;Bloomberg&quot;&gt;</xsl:text>
    <xsl:value-of select="$BBGCode"/>
    <xsl:text disable-output-escaping="yes">&lt;/instrument:reference&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">      &lt;/trade:instrument&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">      &lt;trade:principalPayment&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">          &lt;trade:buyerPartyReference&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">          &lt;party:customPartyId party:name=&quot;GS Account&quot; party:partyIdScheme=&quot;http://www.sophis.net/party/partyId/custom&quot;&gt;</xsl:text>
    <xsl:value-of select="FundID"/>
    <xsl:text disable-output-escaping="yes">&lt;/party:customPartyId&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">          &lt;/trade:buyerPartyReference&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    
    <xsl:text disable-output-escaping="yes">          &lt;trade:sellerPartyReference&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">          &lt;party:partyId party:partyIdScheme=&quot;http://www.sophis.net/party/partyId/externalReference&quot;&gt;SLKCUS31&lt;/party:partyId&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">          &lt;/trade:sellerPartyReference&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">          &lt;trade:principalSettlement&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">            &lt;trade:numberOfSecurities&gt;</xsl:text>
    <xsl:value-of select="$NumberOfSecurities"/>
    <xsl:text disable-output-escaping="yes">&lt;/trade:numberOfSecurities&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">            &lt;trade:spot trade:inSettlementCurrency=&quot;false&quot; trade:type=&quot;InAmount&quot;&gt;</xsl:text>
    <xsl:value-of select="format-number($Amount div $NumberOfSecurities,'####.############')"/>
    <xsl:text disable-output-escaping="yes">&lt;/trade:spot&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">            &lt;trade:amount&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;trade:currency&gt;</xsl:text>
    <xsl:value-of select="$Currency"/>
    <xsl:text disable-output-escaping="yes">&lt;/trade:currency&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">              &lt;trade:amount trade:negative=&quot;true&quot;&gt;</xsl:text>
    <xsl:value-of select="$Amount"/>
    <xsl:text disable-output-escaping="yes">&lt;trade:amount&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">            &lt;/trade:amount&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">            &lt;trade:valueDate&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">              &lt;common:unadjustedDate&gt;</xsl:text>
    <xsl:call-template name="getDate"/>
    <xsl:text disable-output-escaping="yes">&lt;common:unadjustedDate&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">              &lt;common:dateAdjustments&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">                &lt;common:businessDayConvention&gt;NONE&lt;/common:businessDayConvention&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">              &lt;/common:dateAdjustments&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">            &lt;trade:valueDate&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">          &lt;/trade:principalSettlement&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">      &lt;/trade:principalPayment&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">    &lt;trade:tradeProduct&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    
    <xsl:text disable-output-escaping="yes">    &lt;trade:entityPartyReference&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>    
    
    <xsl:text disable-output-escaping="yes">      &lt;party:customPartyId party:name=&quot;GS Account&quot; party:partyIdScheme=&quot;http://www.sophis.net/party/partyId/custom&quot;&gt;</xsl:text>
    <xsl:value-of select="$FundID"/>
    <xsl:text disable-output-escaping="yes">&lt;/party:customPartyId&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>    
    <xsl:text disable-output-escaping="yes">    &lt;/trade:entityPartyReference&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>    
    <xsl:text disable-output-escaping="yes">    &lt;trade:extendedTradeSide&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>    
    <xsl:text disable-output-escaping="yes">      &lt;trade:orderer&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>   
    <xsl:text disable-output-escaping="yes">        &lt;party:party&gt;</xsl:text>
   
    <xsl:text>&#xd;</xsl:text>   
    <xsl:text disable-output-escaping="yes">          &lt;party:customPartyId party:name=&quot;GS Account&quot; party:partyIdScheme=&quot;http://www.sophis.net/party/partyId/custom&quot;&gt;</xsl:text>
    <xsl:value-of select="$FundID"/>
    <xsl:text disable-output-escaping="yes">&lt;/party:customPartyId&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>   
    <xsl:text disable-output-escaping="yes">        &lt;/party:party&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>   
    
    <xsl:text disable-output-escaping="yes">      &lt;/trade:orderer&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>   
    <xsl:text disable-output-escaping="yes">      &lt;trade:creditor&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">        &lt;party:party&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">          &lt;party:partyId party:partyIdScheme=&quot;http://www.sophis.net/party/partyId/externalReference&quot;&gt;</xsl:text>
    <xsl:text>SLKCUS31</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;/party:partyId&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">        &lt;/party:party&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">      &lt;/trade:creditor&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    
    <xsl:text disable-output-escaping="yes">      &lt;trade:entity&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">        &lt;party:party&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">          &lt;party:customPartyId party:name=&quot;GS Account&quot; party:partyIdScheme=&quot;http://www.sophis.net/party/partyId/custom&quot;&gt;</xsl:text>
    <xsl:value-of select="$FundID"/>
    <xsl:text disable-output-escaping="yes">&lt;/party:customPartyId&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">        &lt;/party:party&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">      &lt;/trade:entity&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    
    
    <xsl:text disable-output-escaping="yes">      &lt;trade:counterparty&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">        &lt;party:party&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>     
    <xsl:text disable-output-escaping="yes">          &lt;party:partyId party:partyIdScheme=&quot;http://www.sophis.net/party/partyId/externalReference&quot;&gt;</xsl:text>
    <xsl:text>SLKCUS31</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;/party:partyId&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>     
    <xsl:text disable-output-escaping="yes">      &lt;/trade:counterparty&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    
    
    <xsl:text disable-output-escaping="yes">      &lt;trade:depositary&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">        &lt;party:party&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">          &lt;party:partyId party:partyIdScheme=&quot;http://www.sophis.net/party/partyId/externalReference&quot;&gt;</xsl:text>
    <xsl:text>SLKCUS31</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;/party:partyId&gt;</xsl:text>
    
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">        &lt;/party:party&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    <xsl:text disable-output-escaping="yes">      &lt;/trade:depositary&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    
    
    <xsl:text disable-output-escaping="yes">    &lt;/trade:extendedTradeSide&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>    
    <xsl:text disable-output-escaping="yes">    &lt;trade:documentation/&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>        
    <xsl:text disable-output-escaping="yes">  &lt;/trade:trade&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text> 
    
    <xsl:text>&#xd;</xsl:text>
    
  </xsl:template>
  
  
  <xsl:template match="/">
    <xsl:variable name="lines" select="/lines/line"/>
   
    
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;exch:import version=&quot;4-2&quot; xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">             xmlns:exch=&quot;http://sophis.net/sophis/gxml/dataExchange&quot; xmlns:fpml=&quot;http://www.fpml.org/2005/FpML-4-2&quot;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">             xmlns:dsig=&quot;http://www.w3.org/2000/09/xmldsig#&quot; xmlns:common=&quot;http://sophis.net/sophis/common&quot;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">             xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns:party=&quot;http://www.sophis.net/party&quot;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">             xmlns:trade=&quot;http://www.sophis.net/trade&quot; xmlns:instrument=&quot;http://www.sophis.net/Instrument&quot;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">             xmlns:sph=&quot;http://www.sophis.net/Instrument&quot; xmlns:folio=&quot;http://www.sophis.net/folio&quot;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text disable-output-escaping="yes">             xmlns:user=&quot;http://www.sophis.net/user&quot; exch:batchType=&quot;AllRegardlessOfErrors&quot;></xsl:text>
    <xsl:text>&#xd;</xsl:text>

    <xsl:text>&#x9;</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;fpml:header&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;fpml:conversationId conversationIdScheme=&quot;&quot;/&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;fpml:messageId messageIdScheme=&quot;http://www.sophis.net/gxml/exchange/messageIdScheme/simple&quot;/&gt;</xsl:text>
    <xsl:text disable-output-escaping="yes">001&lt;/fpml:messageId&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;fpml:sentBy partyIdScheme=&quot;http://www.sophis.net/party/partyId/name&quot;/&gt;</xsl:text>
    <xsl:text disable-output-escaping="yes">GS&lt;/fpml:sentBy&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;fpml:sendTo partyIdScheme=&quot;http://www.sophis.net/party/partyId/name&quot;/&gt;</xsl:text>
    <xsl:text disable-output-escaping="yes">GS&lt;/fpml:sendTo&gt;</xsl:text>
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;fpml:creationTimestamp&gt;</xsl:text>
    <xsl:value-of  select="api:datenow"/>
    <xsl:text disable-output-escaping="yes">&lt;/fpml:creationTimestamp&gt;</xsl:text>
    
    
    <xsl:text>&#xd;</xsl:text>
    <xsl:text>&#x9;</xsl:text>
    <xsl:text disable-output-escaping="yes">&lt;/fpml:header&gt;</xsl:text>
    
    <xsl:for-each select="$lines/value1[text() != 'COB Date']/..">
       <xsl:call-template name="lines" xmlns="urn:schemas-microsoft-com:office:spreadsheet">
       </xsl:call-template>
    </xsl:for-each>

    <xsl:text disable-output-escaping="yes">&lt;/exch:import&gt;</xsl:text>
  
  </xsl:template>
</xsl:stylesheet>