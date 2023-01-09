<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:ms="urn:schemas-microsoft-com:xslt" 
                xmlns:ns0="http://www.sophis.net/bo_xml" xmlns:ns1="http://www.sophis.net/otc_message" xmlns:ns2="http://www.sophis.net/trade" xmlns:ns3="http://www.sophis.net/party" xmlns:ns4="http://www.sophis.net/folio" xmlns:ns5="http://www.sophis.net/Instrument" xmlns:ns6="http://www.sophis.net/SSI" xmlns:ns7="http://www.sophis.net/NostroAccountReference" xmlns:ns8="http://sophis.net/sophis/common" xmlns:api="urn:internal-api"
                exclude-result-prefixes="ns0 ns1 ns2 ns3 ns4 ns5 ns6 ns7 ns8 api"
>

  <xsl:output method="text" indent="no"/>
  <xsl:variable name="seperator" select="';'"/>
  <!-- Basics -->
  <xsl:template name="tradeid">
      <xsl:value-of select="//ns2:trade/ns2:tradeHeader/ns2:partyTradeIdentifier/ns2:tradeId[@ns2:tradeIdScheme='http://www.sophis.net/trade/tradeId/id']"/>
  </xsl:template>
  
  <xsl:variable name="MGP">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:nostroAccountReference/ns7:accountAtCustodian"/>
  </xsl:variable>
  <xsl:variable name="MUL_FUND">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroSecurity/ns6:accountAtCustodian"/>
  </xsl:variable>
  
  <!-- Times and Dates -->
  <xsl:template name="trade_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:tradeDate,'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="message_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:messageCreationTimestamp,'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="message_time">
    <xsl:value-of select="ms:format-time(/ns0:sph_otc_message/ns0:messageCreationTimestamp,'hhmmss')"/>
  </xsl:template>
  <xsl:template name="debit_date">
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:paymentDate,'yyyyMMdd')"/>
  </xsl:template>
  <xsl:template name="credit_date"> 
    <xsl:value-of select="ms:format-date(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:settlementDate,'yyyyMMdd')"/>
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
    <xsl:template name="user_id">
      <xsl:value-of select="//ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:userid/ns1:name"/>
  </xsl:template>
  
  <xsl:template name="FileName" match="*">
    <xsl:call-template name="action"/>
    <xsl:call-template name="tradeid"/>
    <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:ident"/>
  </xsl:template>
  
  <xsl:variable name="cash_amount">
    <xsl:variable name="signed_value" select="format-number(/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:amount,'#.##')"/>
    <xsl:value-of select="$signed_value*($signed_value >=0) - $signed_value*($signed_value &lt; 0)"/>
   <!-- [CHECK] /ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:amount/ns2:amount -->
  </xsl:variable>
  
  <xsl:variable name="amount_ccy">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:movement/ns2:currency"/>
  </xsl:variable>
  
  <!-- 
  <xsl:variable name="ben_country">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:entity/ns3:party/ns3:description/ns3:names/ns3:location"/>
  </xsl:variable>
  -->
  
  <xsl:template name="customer_indicator">
	<xsl:variable name="isBank" select="/ns0:sph_otc_message/ns0:counterparty/ns3:party/ns3:description/ns3:category/ns3:bank"/>
	  <xsl:choose>
		<xsl:when test="$isBank = 'true'">
		  <xsl:text>Y</xsl:text>
		</xsl:when>
		<xsl:otherwise>
		  <xsl:text>N</xsl:text>
		</xsl:otherwise>
	  </xsl:choose>
  </xsl:template>
    
  <xsl:variable name="external_status">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:externalstatus"/>
  </xsl:variable>
  
  <xsl:variable name="internal_status">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:initialstatus"/>
  </xsl:variable>
  
  <xsl:variable name="book_cat">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:bookid"/>
  </xsl:variable>
  
  <xsl:variable name="smallcase" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />



  <xsl:variable name="paiem_mth">
    <xsl:variable name="payment_method" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:paymentMethod"/>
    <xsl:choose>
        <xsl:when test="translate($payment_method, $smallcase, $uppercase) = 'SWIFT'">
            <xsl:text>S</xsl:text>
        </xsl:when>
        <xsl:when test="translate($payment_method, $smallcase, $uppercase) = 'CHECKS'">
            <xsl:text>C</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>N</xsl:text>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="msg_ind">
    <xsl:choose>
        <xsl:when test="$paiem_mth = 'S'">
            <xsl:text>Y</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text>N</xsl:text>
        </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  
  <xsl:variable name="deb_acc_nr">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:nostroAccountReference/ns7:accountAtCustodian"/>
  </xsl:variable>
  <xsl:variable name="cre_acc_nr">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:lostroCash/ns6:accountAtCustodian"/>
  </xsl:variable>
  <xsl:variable name="deb_acc_fmt" select="'IBAN'"/>
  <xsl:variable name="cre_acc_fmt" select="'IBAN'"/>
  
  <xsl:variable name="creditor_id">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:creditor/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/id']"/>
  </xsl:variable>
  <xsl:variable name="debtor_id">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:orderer/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/id']"/>
  </xsl:variable>
  <xsl:variable name="account1_id">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:movement/ns2:account1/ns5:accountId"/>
  </xsl:variable>
  <xsl:variable name="account2_id">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:movement/ns2:account2/ns5:accountId"/>
  </xsl:variable>
  <xsl:variable name="ccy_amount">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:currencyamount"/>
  </xsl:variable>
  <xsl:variable name="sender_ssi_path">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:senderssipath"/>
  </xsl:variable>
  <xsl:variable name="receiver_ssi_path">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:receiverssipath"/>
  </xsl:variable>
  
  <xsl:variable name="sender_mgp">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:sender/ns1:account1"/>
  </xsl:variable>
  <xsl:variable name="receiver_mgp">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:receiver/ns1:account1"/>
  </xsl:variable>
  
  <xsl:variable name="creditor_iban">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:entity/ns3:party/ns3:description/ns3:advancedData/ns3:settlementInstruction/ns3:accountCustodian[text() =$sender_mgp]/../ns3:UserColumnsData/ns3:UserColumn/ns3:ColumnName[text()='Fund IBAN']/../ns3:Value"/>
  </xsl:variable>
  <xsl:variable name="debtor_iban">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:entity/ns3:party/ns3:description/ns3:advancedData/ns3:settlementInstruction/ns3:accountCustodian[text() =$receiver_mgp]/../ns3:UserColumnsData/ns3:UserColumn/ns3:ColumnName[text()='Fund IBAN']/../ns3:Value"/>
  </xsl:variable>
  
  <!-- first two chars of the debtor iban -->
  <xsl:variable name="ben_country">
    <xsl:value-of select="substring($debtor_iban,1,2)"/>
  </xsl:variable>
 <xsl:variable name="scus_ben">
    <xsl:if test="$msg_ind = 'Y'">
    
    </xsl:if>
 </xsl:variable>
  
 <xsl:template match="*">
 
 <!-- Begin Cash implementation -->
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
 <!-- <xsl:call-template name="user_id"/> [REM: hardcoded because it should always be MEDIO] -->
 <xsl:value-of select="'MEDIO'"/>
 <xsl:value-of select="$seperator"/>
 <!-- REF-EXT -->
 <xsl:call-template name="tradeid"/>
 <xsl:value-of select="$seperator"/>
 <!-- INTERNAL_ORIGID -->
 
 <xsl:value-of select="$seperator"/>
 <!-- INTERNAL_ID -->
 
 <xsl:value-of select="$seperator"/>
 <!-- INTERNAL_STATUS -->
 <xsl:value-of select="$internal_status"/>
 <xsl:value-of select="$seperator"/>
 <!-- EXTERNAL_ORIGID -->
 
 <xsl:value-of select="$seperator"/>
 <!-- EXTERNAL_ID -->
 
 <xsl:value-of select="$seperator"/>
 <!-- EXTERNAL_STATUS -->
 <xsl:value-of select="$external_status"/>
 <xsl:value-of select="$seperator"/>
 <!-- DATE_OUT -->
 <xsl:call-template name="message_date"/>
 <xsl:value-of select="$seperator"/>
 <!-- TIME_OUT -->
 <xsl:call-template name="message_time"/>
 <xsl:value-of select="$seperator"/>
 <!-- ERROR_MESSAGE -->
 
 <xsl:value-of select="$seperator"/>
 <!-- ISS-REF [NOTE: leave blank for this kind of trade]-->
 <xsl:value-of select="$seperator"/>
 <!-- MGP [CHECK]-->
 <xsl:value-of select="$sender_mgp"/>
 <xsl:value-of select="$seperator"/>
 <!-- OPE-TYP [Note: You can leave this field blank – It is noted as mandatory in the specifications only for a particular type of trade. ]-->
 <xsl:value-of select="'TPD'"/>
 <xsl:value-of select="$seperator"/>
 <!-- BOOK-CAT [CHECK]-->
 <!--<xsl:value-of select="$book_cat"/>-->
 <xsl:value-of select="'PAIEM'"/>
 <xsl:value-of select="$seperator"/>
 <!-- SEC-TYPE -->
 
 <xsl:value-of select="$seperator"/>
 <!-- SEC-NUM -->
 
 <xsl:value-of select="$seperator"/>
 <!-- SEC-DES -->
 
 <xsl:value-of select="$seperator"/>
 <!-- DEB-CASH-PRD -->
 <xsl:value-of select="'CABR'"/>
 <xsl:value-of select="$seperator"/>
 <!-- DEB-ACC-FMT [TODO]-->
 <xsl:value-of select="$deb_acc_fmt"/>
 <xsl:value-of select="$seperator"/>
 <!-- DEB-ACC-NR [TODO]-->
 <xsl:value-of select="$debtor_iban"/>
 <xsl:value-of select="$seperator"/>
 <!-- CRE-ACC-CL -->
 
 <xsl:value-of select="$seperator"/>
 <!-- CRE-ACC-FMT [TODO]-->
 <xsl:value-of select="$cre_acc_fmt"/>
 <xsl:value-of select="$seperator"/>
 <!-- CRE-ACC-NR [TODO]-->
 <xsl:value-of select="$creditor_iban"/>
 <xsl:value-of select="$seperator"/>
 <!-- CRE-BAL-SHEETACC -->
 
 <xsl:value-of select="$seperator"/>
 <!-- CASH-AMT-CUR -->
 <xsl:value-of select="$amount_ccy"/>
 <xsl:value-of select="$seperator"/>
 <!-- CASH-AMT [CHECK]-->
 <xsl:value-of select="$cash_amount"/>
 <xsl:value-of select="$seperator"/>
 <!-- CASH-AMT-INITCURR -->
 
 <xsl:value-of select="$seperator"/>
 <!-- CASH-AMT-INIT -->
 
 <xsl:value-of select="$seperator"/>
 <!-- EXCH-RATE -->
 
 <xsl:value-of select="$seperator"/>
 <!-- DEB-VALUE-DATE [CHECK]-->
 <xsl:call-template name="debit_date"/>
 <xsl:value-of select="$seperator"/>
 <!-- CRE-VALUE-DATE [CHECK]-->
 <xsl:call-template name="credit_date"/>
 <xsl:value-of select="$seperator"/>
 <!-- FINAN-TRT -->
 
 <xsl:value-of select="$seperator"/>
 <!-- PAIEM-MTH -->
 <xsl:value-of select="$paiem_mth"/>
 <xsl:value-of select="$seperator"/>
 <!-- BEN-COUNTRY [CHECK]--> 
 <xsl:value-of select="$ben_country"/>
 <xsl:value-of select="$seperator"/>
 <!-- ADR-SWI-BEN -->
 
 <xsl:value-of select="$seperator"/>
 <!-- MSG-IND [TODO]-->
 <xsl:value-of select="$msg_ind"/>
 <xsl:value-of select="$seperator"/>
 <!-- FEE-IND -->
 
 <xsl:value-of select="$seperator"/>
 <!-- INT-BEN -->
 
 <xsl:value-of select="$seperator"/>
 <!-- SCUS-BEN -->
 <xsl:value-of select="$scus_ben"/>
 <xsl:value-of select="$seperator"/>
 <!-- NCSC-SCUS-BEN -->
 
 <xsl:value-of select="$seperator"/>
 <!-- ACC-SCUS-BEN -->
 
 <xsl:value-of select="$seperator"/>
 <!-- BEN-CUS-IND [Leave blank - RBC request/suggestion]-->
 <!-- <xsl:call-template name="customer_indicator"/> -->
 <xsl:value-of select="$seperator"/>
 <!-- BEN-CUS -->
 
 <xsl:value-of select="$seperator"/>
 <!-- NCSC-BEN-CUS -->
 
 <xsl:value-of select="$seperator"/>
 <!-- ACC-BEN-CUS -->
 
 <xsl:value-of select="$seperator"/>
 <!-- BEN-COMM-I -->
 
 <xsl:value-of select="$seperator"/>
 <!-- BEN-COMM-II -->
 
 <xsl:value-of select="$seperator"/>
 <!-- BEN-COMM-III -->
 
 <xsl:value-of select="$seperator"/>
 <!-- BEN-COMM-IV -->
 
 <xsl:value-of select="$seperator"/>
 <!-- BANK-COMM-I -->
 
 <xsl:value-of select="$seperator"/>
 <!-- BANK-COMM-II -->
 
 <xsl:value-of select="$seperator"/>
 <!-- BANK-COMM-III -->
 
 <xsl:value-of select="$seperator"/>
 <!-- BANK-COMM-IV -->
 
 <xsl:value-of select="$seperator"/>
 <!-- MUL-FUND [TODO]-->
 <xsl:value-of select="$receiver_mgp"/>
 <xsl:value-of select="$seperator"/>
 <!-- DATTAC -->
 <xsl:call-template name="trade_date"/>
 
 <!-- End Cash implementation -->
 </xsl:template>
</xsl:stylesheet>