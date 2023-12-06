<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:ms="urn:schemas-microsoft-com:xslt" 
                xmlns:ns0="http://www.sophis.net/bo_xml" 
				xmlns:ns1="http://www.sophis.net/otc_message" 
				xmlns:ns2="http://www.sophis.net/trade" 
				xmlns:ns3="http://www.sophis.net/party" 
				xmlns:ns4="http://www.sophis.net/folio" 
				xmlns:ns5="http://www.sophis.net/Instrument" 
				xmlns:ns6="http://www.sophis.net/SSI" 
				xmlns:ns7="http://www.sophis.net/NostroAccountReference" 
				xmlns:ns8="http://sophis.net/sophis/common" 
				xmlns:api="urn:internal-api"
                exclude-result-prefixes="ns0 ns1 ns2 ns3 ns4 ns5 ns6 ns7 ns8 api"
>
<xsl:output method="text" encoding="ASCII" indent="no"/>


	<!-- If buyer = Entity, then its a BY trade, otherwise SELL (SL)-->
  <xsl:template name="BuySell">
    <xsl:variable name="buyer" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:buyerPartyReference/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/id']"/>
    <xsl:variable name="entity" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:entityPartyReference/ns3:partyId"/>
    <xsl:choose>
      <xsl:when test="$buyer = $entity">
        <xsl:text>BY</xsl:text>
      </xsl:when>
      <xsl:otherwise>
        <xsl:text>SL</xsl:text>
      </xsl:otherwise>
    </xsl:choose>
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
    <xsl:variable name="broker" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:broker/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/legalName']/text()"/>
    <xsl:variable name="cpty" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/legalName']/text()"/>
    <xsl:choose>
		<xsl:when test="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:counterparty/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/name']/text()='EXECUTION'">
		  <xsl:value-of select="$broker"/>
		</xsl:when>
		<xsl:otherwise>
		  <xsl:value-of select="$cpty"/>
		</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  	<xsl:template name="CBAccountID" match="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:nostroAccountReference/ns7:boExternalReferencesList/ns7:externalReference">
    <xsl:variable name="name" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:nostroAccountReference/ns7:boExternalReferencesList/ns7:externalReference/ns7:name"/>
    <xsl:variable name="value" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:nostroAccountReference/ns7:boExternalReferencesList/ns7:externalReference/ns7:value"/>
    <xsl:choose>
		<xsl:when test="$name='GsAccountID'">
		  <xsl:value-of select="$value"/>
		</xsl:when>
		<xsl:otherwise>
		  <xsl:value-of select="''"/>
		</xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template name="SEC-COD">
    <xsl:value-of select="//ns0:sph_otc_message/ns0:instrument/*/ns5:identifier/ns5:reference[@ns5:name='Ticker']"/>
  </xsl:template>
  
   <xsl:template name="InstrCCY">
   <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/*/ns5:currency"/>
  </xsl:template>
  
  <xsl:template name="maturity_date">
    <xsl:value-of select="ms:format-date(//ns5:expirationDate/ns8:adjustableDate/ns8:unadjustedDate, 'yyyyMMdd')"/>
  </xsl:template>
  
  <xsl:variable name="specialChars">!&amp;&lt;|$*;^%_>`#@="'~[]{}\</xsl:variable>
  <xsl:variable name="replacementChar" select="' '" />
  <xsl:template name="replaceSpecialChars">
    <xsl:param name="text"/>
    <xsl:param name="charIndex" select="1"/>
    <xsl:param name="resultText" select="''"/>
    <xsl:choose>
      <xsl:when test="$charIndex &gt; string-length($text)">
        
        <xsl:value-of select="$resultText"/>
      </xsl:when>
      <xsl:otherwise>
        
        <xsl:variable name="char" select="substring($text, $charIndex, 1)"/>
        
        <xsl:variable name="isSpecialChar" select="contains($specialChars, $char)"/>
        
        <xsl:variable name="newChar">
          <xsl:choose>
            <xsl:when test="$isSpecialChar">
              <xsl:value-of select="$replacementChar" />
            </xsl:when>
            <xsl:otherwise>
              <xsl:value-of select="$char"/>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        
        <xsl:call-template name="replaceSpecialChars">
          <xsl:with-param name="text" select="$text"/>
          <xsl:with-param name="charIndex" select="$charIndex + 1"/>
          <xsl:with-param name="resultText" select="concat($resultText, $newChar)"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template match="/">
	<!-- saad Work -->
	<!-- *********************************General Variables Begin********************************-->
	<xsl:variable name="MessageTypeConst">202C</xsl:variable>
	<xsl:variable name="MT202SenderInfoStatic">/BNF/:CW:CASH</xsl:variable>
	<xsl:variable name="BoComment" select="//ns2:tradeHeader/ns2:extendedPartyTradeInformation/ns2:boComment"/>
	<xsl:variable name="FoComment" select="//ns2:tradeHeader/ns2:extendedPartyTradeInformation/ns2:comment"/>
	<xsl:variable name="TradeDate" select="ms:format-date(//ns2:tradeHeader/ns2:tradeDate,'yyMMdd')"/>
	<xsl:variable name="TradeDateDDMMYY" select="ms:format-date(//ns2:tradeHeader/ns2:tradeDate,'ddMMyy')"/>
	<xsl:variable name="SettlementDate" select="ms:format-date(//ns2:tradeHeader/ns2:settlementDate,'yyMMdd')"/>
	<xsl:variable name="OriginalInstDate" select="ms:format-date(//ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:tradeDate,'yyMMdd')"/>
	<xsl:variable name="SettlementCurrency" select="//ns2:amount/ns2:currency"/>
	<xsl:variable name="dealAmount" select="//ns2:amount/ns2:amount"/>
	<xsl:variable name="OrdringInstitutionAdress" select="//ns2:nostroCash/ns6:accountName"/>
	<xsl:variable name="OrdringInstitutionAccount" select="//ns2:nostroSecurity/ns6:accountAtAgent"/>
	<xsl:variable name="MedioSwift" select="//ns1:tradeSource/ns2:trade/ns2:tradeHeader/ns2:extendedPartyTradeInformation/ns2:partyReference/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']"/>
	<xsl:variable name="MedioSwift11Char" select="concat($MedioSwift, substring('XXXXXXXXXXX', 1, 11 - string-length($MedioSwift)))"/>
	
	<xsl:variable name="OrdringInstitutionAdressWSplCar">
		<xsl:call-template name="replaceSpecialChars">
			<xsl:with-param name="text" select="$OrdringInstitutionAdress"/>
		</xsl:call-template>
    </xsl:variable>
	
	<!-- /ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroSecurity/ns6:accountAtAgent -->
	<!-- /ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:accountAtAgent -->
    <xsl:variable name="AccountWInstitBIC" select="//ns2:depositary/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']"/>
	<xsl:variable name="AccountWInstitBIC11Char" select="concat($AccountWInstitBIC, substring('XXXXXXXXXXX', 1, 11 - string-length($AccountWInstitBIC)))"/>
	<!-- *********************************General Variables End *********************************-->
	<xsl:variable name="separator" select="'|'"/>
	<xsl:variable name="emptyVar" select="''"/>
	
	<xsl:variable name="MessRefID" select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:ident"/>
	<xsl:variable name="PrevMessRefID" select="/ns0:sph_otc_message/ns0:otc_message/ns1:message/ns1:linkreversalid"/>
	<xsl:variable name="Recepient" select="//ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:depositary/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']"/>
	<xsl:variable name="RecepientReference" select="//ns1:tradeSource/ns2:trade/ns2:extendedTradeSide/ns2:depositary/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/reference']"/>
	<xsl:variable name="Recepient11Char" select="concat($Recepient, substring('XXXXXXXXXXX', 1, 11 - string-length($Recepient)))"/>
	<xsl:variable name="boCommentFirstElement">
		<xsl:value-of select="substring-before($BoComment, '|')" />
	</xsl:variable>
	<xsl:variable name="boCommentSecondElement">
		<xsl:value-of select="substring-after($BoComment, '|')" />
	</xsl:variable>
	<xsl:variable name="boCommentSecondElementWSplChar">
		<xsl:call-template name="replaceSpecialChars">
			<xsl:with-param name="text" select="$boCommentSecondElement"/>
		</xsl:call-template>
	</xsl:variable>
	
	<xsl:variable name="IsAmmountNegative">
		<xsl:choose>
			<xsl:when test="//ns0:otc_message/ns1:message/ns1:amount &lt; 0">
				<xsl:text>true</xsl:text>
			</xsl:when>
			<xsl:otherwise>
				<xsl:text>false</xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>


	
	<!-- Field 3 Begin-->
	<xsl:variable name="TransSendersRef" select="//ns0:otc_message/ns1:message/ns1:ident"/>
	<xsl:variable name="TransSendersRefLinkRev" select="//ns0:otc_message/ns1:message/ns1:linkreversalid"/>
	<xsl:variable name="TradeVersion" select="//ns2:partyTradeIdentifier/ns2:tradeId[@ns2:tradeIdScheme='http://www.sophis.net/trade/tradeId/version']"/>
	<xsl:variable name="TransSendersRefDyn">
		<xsl:choose>
			<xsl:when test="$MessageTypeConst='202' or $MessageTypeConst='210'">
				<xsl:value-of select="$TransSendersRef"/>
			</xsl:when>
			<xsl:when test="$MessageTypeConst = '202C' or $MessageTypeConst = '210C'">
			  <xsl:value-of select="$TransSendersRef"/>
			</xsl:when>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="TransSendersRefCustodian">
		<xsl:choose>
			<xsl:when test="$RecepientReference='FETALULLISV' or $RecepientReference='BILLIE2D'">
				<xsl:choose>
					<xsl:when test="$MessageTypeConst='202' or $MessageTypeConst='210'">
						<xsl:value-of select="concat($TradeDateDDMMYY, $TransSendersRef)"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="concat($TradeDateDDMMYY, $TransSendersRef)"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="$MessageTypeConst='202' or $MessageTypeConst='210'">
						<xsl:value-of select="$TransSendersRef"/>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$TransSendersRef"/>
					</xsl:otherwise>
				</xsl:choose>
            </xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- Field 3 End-->
	
	<xsl:variable name="PortfolioCode" select="substring(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:nostroAccountReference/ns7:accountAtCustodian,1,14)"/>	
  
	<xsl:variable name="AccountName" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:nostroAccountReference/ns7:accountName"/>

	<xsl:variable name="optionName" select="/ns0:sph_otc_message/ns0:instrument/ns5:equityOption/ns5:name"/>
	<xsl:variable name="futureName" select="/ns0:sph_otc_message/ns0:instrument/ns5:forexFuture/ns5:name"/>
	
	<xsl:variable name="secType">
		<xsl:choose>
			<xsl:when test="$optionName">
				<xsl:value-of select="'OPT'"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="'FUT'" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>

	<xsl:variable name="MessType">
		<xsl:choose>
			<xsl:when test="$PrevMessRefID='0'">
				<xsl:value-of select="'202'"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="'292'" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- SE_ Variable for Dynamic Values Begin -->
	
	<!-- Field 6 Begin-->
	<xsl:variable name="accountNumber">
		<xsl:choose>
			<xsl:when test="$IsAmmountNegative = 'true'">
				<xsl:choose>
				  <xsl:when test="$MessageTypeConst = '202'">
						<xsl:value-of select="$boCommentFirstElement"/>
				  </xsl:when>
				  <xsl:otherwise>
					<xsl:value-of select="$OrdringInstitutionAccount"/>
				  </xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
				  <xsl:when test="$MessageTypeConst = '202'">
						<xsl:value-of select="$OrdringInstitutionAccount"/>
				  </xsl:when>
				  <xsl:otherwise>
						<xsl:value-of select="$boCommentFirstElement"/>
				  </xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- Field 6 End-->
	<!-- Field 7 Begin-->
		<xsl:variable name="RelatedRef">
		<xsl:choose>
			<xsl:when test="$RecepientReference='FETALULLISV' or $RecepientReference='BILLIE2D'">
				<xsl:choose>
					<xsl:when test="$MessageTypeConst='202' or $MessageTypeConst='210'">
						<xsl:text>ACCT</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="concat($TradeDateDDMMYY, $TransSendersRefLinkRev)"/>
					</xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
					<xsl:when test="$MessageTypeConst='202' or $MessageTypeConst='210'">
						<xsl:text>ACCT</xsl:text>
					</xsl:when>
					<xsl:otherwise>
						<xsl:value-of select="$TransSendersRefLinkRev"/>
					</xsl:otherwise>
				</xsl:choose>
            </xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- Field 7 End-->
	<!-- Field 8 Begin-->
	<xsl:variable name="RelatedRefC">
		<xsl:choose>
			<xsl:when test="$MessageTypeConst = '202C' or $MessageTypeConst = '210C'">
				<xsl:text>ACCT</xsl:text>
			</xsl:when>
			<xsl:otherwise>
			  <xsl:text></xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- Field 8 End-->
	<!-- Field 9 Start-->
	<xsl:variable name="TradeDateDyn">
		<xsl:choose>
			<xsl:when test="$MessageTypeConst = '202C' or $MessageTypeConst = '210C'">
			  <xsl:value-of select="$TradeDate"/>
			</xsl:when>
			<xsl:otherwise>
			  <xsl:text></xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<xsl:variable name="SettlementDateDyn">
		<xsl:choose>
			<xsl:when test="$MessageTypeConst = '202C' or $MessageTypeConst = '210C'">
			  <xsl:value-of select="$SettlementDate"/>
			</xsl:when>
			<xsl:otherwise>
			  <xsl:text></xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- Field 9 End-->
	<!-- Field 10 Start-->
		<!-- normal process -->
	<!-- Field 10 End-->
	<!-- Field 11 Start-->
		<!-- normal process -->
	<!-- Field 11 End-->
	<!-- Field 12 Start-->
	<xsl:variable name="DealAmount15Char">
      <xsl:value-of select="substring($dealAmount, 1, 15)"/>
	</xsl:variable>
	<xsl:variable name="DealAmount15CharDyn">
		<xsl:choose>
			<xsl:when test="$MessageTypeConst = '202' or $MessageTypeConst = '202C'">
			  <xsl:value-of select="$DealAmount15Char"/>
			</xsl:when>
			<xsl:otherwise>
			  <xsl:text></xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- Field 12 End-->
	<!-- Field 19 Begin-->
	<xsl:variable name="OrderingInstitBic">
		<xsl:choose>
			<xsl:when test="$RecepientReference = 'SBOSITMLIMO'">
			  <xsl:value-of select="$MedioSwift11Char"/>
			</xsl:when>
			<xsl:otherwise>
			  <xsl:text></xsl:text>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- Field 19 End-->
	<!-- Field 21 Begin-->
	<xsl:variable name="OrderingIstitNameAdress">
	<xsl:choose>
		<xsl:when test="$RecepientReference = 'SBOSITMLIMO'">
			<xsl:text></xsl:text>
		</xsl:when>
		<xsl:otherwise>
		  <xsl:choose>
			<xsl:when test="$IsAmmountNegative = 'true'">
				<xsl:choose>
				  <xsl:when test="$MessageTypeConst = '202'">
						<xsl:value-of select="$boCommentSecondElementWSplChar"/>
				  </xsl:when>
				  <xsl:otherwise>
					<xsl:value-of select="$OrdringInstitutionAdressWSplCar"/>
				  </xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
				  <xsl:when test="$MessageTypeConst = '202'">
						<xsl:value-of select="$OrdringInstitutionAdressWSplCar"/>
				  </xsl:when>
				  <xsl:otherwise>
						<xsl:value-of select="$boCommentSecondElementWSplChar"/>
				  </xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		  </xsl:choose>
		</xsl:otherwise>
	</xsl:choose>
	</xsl:variable>
	<!-- Field 21 End-->
	<!-- Field 22 Begin-->
	<xsl:variable name="OrderingIstitAccount">
	  <xsl:choose>
			<xsl:when test="$IsAmmountNegative = 'true'">
				<xsl:choose>
				  <xsl:when test="$MessageTypeConst = '202'">
						<xsl:value-of select="$boCommentFirstElement"/>
				  </xsl:when>
				  <xsl:otherwise>
					<xsl:value-of select="$OrdringInstitutionAccount"/>
				  </xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
				  <xsl:when test="$MessageTypeConst = '202'">
						<xsl:value-of select="$OrdringInstitutionAccount"/>
				  </xsl:when>
				  <xsl:otherwise>
						<xsl:value-of select="$boCommentFirstElement"/>
				  </xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- Field 22 End-->
	<!-- Field 34 Begin-->
	<xsl:variable name="AccountWInstitBIC11CharDyn">
	<xsl:choose>
		<xsl:when test="$RecepientReference = 'SBOSITMLIMO'">
		<xsl:choose>
			<xsl:when test="$MessageTypeConst = '202' or $MessageTypeConst = '202C'">
			  <xsl:value-of select="$AccountWInstitBIC11Char"/>
			</xsl:when>
			<xsl:otherwise>
			  <xsl:text></xsl:text>
			</xsl:otherwise>
		</xsl:choose>
		</xsl:when>
		<xsl:otherwise>
			  <xsl:text></xsl:text>
		</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- Field 34 End-->
	<!-- Field 41 Begin-->
	<xsl:variable name="BenifInstitNameAdress">
	  <xsl:choose>
			<xsl:when test="$IsAmmountNegative = 'false'">
				<xsl:choose>
				  <xsl:when test="$MessageTypeConst = '202'">
						<xsl:value-of select="$boCommentSecondElementWSplChar"/>
				  </xsl:when>
				  <xsl:otherwise>
					<xsl:value-of select="$OrdringInstitutionAdressWSplCar"/>
				  </xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
				  <xsl:when test="$MessageTypeConst = '202'">
						<xsl:value-of select="$OrdringInstitutionAdressWSplCar"/>
				  </xsl:when>
				  <xsl:otherwise>
						<xsl:value-of select="$boCommentSecondElementWSplChar"/>
				  </xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		  </xsl:choose>
	</xsl:variable>
	<!-- Field 41 End-->
	<!-- Field 42 Begin-->
	<xsl:variable name="BenifInstitNameAccount">
	  <xsl:choose>
			<xsl:when test="$IsAmmountNegative = 'false'">
				<xsl:choose>
				  <xsl:when test="$MessageTypeConst = '202'">
						<xsl:value-of select="$boCommentFirstElement"/>
				  </xsl:when>
				  <xsl:otherwise>
					<xsl:value-of select="$OrdringInstitutionAccount"/>
				  </xsl:otherwise>
				</xsl:choose>
			</xsl:when>
			<xsl:otherwise>
				<xsl:choose>
				  <xsl:when test="$MessageTypeConst = '202'">
						<xsl:value-of select="$OrdringInstitutionAccount"/>
				  </xsl:when>
				  <xsl:otherwise>
						<xsl:value-of select="$boCommentFirstElement"/>
				  </xsl:otherwise>
				</xsl:choose>
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	<!-- Field 42 End-->
	<!-- saad work Variable End -->
	<xsl:variable name="OPT_TYP">
    <xsl:choose>
        <xsl:when test="contains(/ns0:sph_otc_message/ns0:instrument/*/ns5:optionType,'Call')">
            <xsl:text>CAL</xsl:text>
        </xsl:when>
        <xsl:when test="contains(/ns0:sph_otc_message/ns0:instrument/*/ns5:optionType,'Put')">
            <xsl:text>PUTO</xsl:text>
        </xsl:when>
        <xsl:otherwise>
            <xsl:text></xsl:text>
        </xsl:otherwise>
    </xsl:choose>
 </xsl:variable>
	
	
	<!--TBD-->
	
	<xsl:variable name="InstrName" select="/ns0:sph_otc_message/ns0:instrument/*/ns5:name"/>
	<xsl:variable name="NbOfSec" select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:numberOfSecurities,'#')"/>
	<xsl:variable name="DealPrice" select="format-number(/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:spot,'#0.######')"/>
	<xsl:variable name="ISOSettlementCurrency" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:grossAmount/ns2:currency"/>
	
	<xsl:variable name="ChargesFees" select="''"/>
	<xsl:variable name="SECOtherAmounts" select="''"/>
	<xsl:variable name="netStlAmount" select="'0'"/>

	
	<xsl:variable name="CountryofSettleISOCode" select="'XX'"/>	
	<xsl:variable name="ClearingBrokerBIC" select="/ns0:sph_otc_message/ns0:source/ns1:tradeSource/ns2:trade/ns2:tradeProduct/ns2:principalPayment/ns2:principalSettlement/ns2:nostroCash/ns6:custodian"/>

	<xsl:variable name="UnderlyingSecID">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:instrument/*/ns5:underlyer/ns5:reference[@ns5:name='Ticker']/text()"/>
	</xsl:variable>
	
	<xsl:variable name="trade_contract_size">
  	<xsl:choose>
		<xsl:when test="/ns0:sph_otc_message/ns0:instrument/ns5:equityFuture/ns5:contractSize">
			<xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:instrument/ns5:equityFuture/ns5:contractSize, '#.####')"/>
		</xsl:when>
		<xsl:when test="/ns0:sph_otc_message/ns0:instrument/*/ns5:contractSize">
			<xsl:value-of select="format-number(/ns0:sph_otc_message/ns0:instrument/*/ns5:contractSize, '#.####')"/>
		</xsl:when>
		<xsl:otherwise>
			<xsl:value-of select="1"/>
		</xsl:otherwise>
	</xsl:choose>
  </xsl:variable>
	
	<xsl:variable name="priceType">
		<xsl:choose>
			<xsl:when test="$secType='OPT'">
				<xsl:value-of select="'PREM'"/>
			</xsl:when>
			<xsl:otherwise>
				<xsl:value-of select="'ACTU'" />
			</xsl:otherwise>
		</xsl:choose>
	</xsl:variable>
	
	<xsl:variable name="MessRecipientID">
    <xsl:value-of select="/ns0:sph_otc_message/ns0:depositary/ns3:party/ns3:partyId[@ns3:partyIdScheme='http://www.sophis.net/party/partyId/swiftCode']/text()"/>
	</xsl:variable>
	
	
	<xsl:variable name="MessRecIDFormatted">
    <xsl:value-of select="substring(concat($MessRecipientID, 'XXXXXXXXXXX'), 1, 11)"/>
	</xsl:variable>
	<!--1 Message Type-->
	<!-- To do seperate template -->
	<!-- <xsl:value-of select="$MessType" />  -->
	<xsl:value-of select="$MessageTypeConst" />
	<xsl:value-of select="$separator" />
	
	<!-- 2 Recipient-->
	<xsl:value-of select="$Recepient11Char" />
	<xsl:value-of select="$separator" />
	
	<!--3 Transaction/Senders Reference Number-->
	<xsl:value-of select="$TransSendersRefCustodian" />
	<!-- <xsl:value-of select="$MessRefID" /> -->
	<xsl:value-of select="$separator" />
	<!--4 Time Indication code-->
	<xsl:value-of select="$separator" />
	<!--5 Time Indication Time-->
	<xsl:value-of select="$separator" />
	<!--6 Account Number-->
	<xsl:value-of select="$accountNumber" />
	<xsl:value-of select="$separator" />
	<!--7 Related Reference-->
	<xsl:value-of select="$RelatedRef" />
	<xsl:value-of select="$separator" />
	<!--8 Related Reference of the Original Message-->
	<xsl:value-of select="$RelatedRefC" />
	<!-- <xsl:value-of select="$PrevMessRefID" />  -->
	<xsl:value-of select="$separator" />
	<!--9 Original Instruction Date-->
	<xsl:value-of select="$TradeDateDyn" />
	<xsl:value-of select="$separator" />
	<!--10 Value Date-->
	<xsl:value-of select="$SettlementDate" /> <!--TO CHECK $SettlementDate-->
	<xsl:value-of select="$separator" />
	<!--11 Currency code of Settled Amount-->
	<xsl:value-of select="$SettlementCurrency" />
	<xsl:value-of select="$separator" />
	<!--12 Settled Amount-->
	<xsl:value-of select="$DealAmount15CharDyn" /> <!--TO CHECK -->
	<xsl:value-of select="$separator" />
	<!--13 Ordering Customer - BIC/BEI-->
	<!--TO CHECK -->
	<xsl:value-of select="$separator" />
	<!--14 Ordering Customer – Account-->
	<!--TO CHECK -->
	<xsl:value-of select="$separator" />
	<!--15 Ordering Customer – Account-->
	<!--TO CHECK -->
	<xsl:value-of select="$separator" />
	<!--16-->
	<xsl:value-of select="$separator" />
	<!--17-->
	<xsl:value-of select="$separator" />
	<!--18-->
	<xsl:value-of select="$separator" />
	<!--19 OrdringInstBIC-->
	<xsl:value-of select="$OrderingInstitBic" />
	<xsl:value-of select="$separator" />
	<!--20-->
	<xsl:value-of select="$separator" />
	<!--21 Ordering Institution Name & Address-->
	<xsl:value-of select="$OrderingIstitNameAdress" />
	<!-- <xsl:value-of select="$OrdringInstitutionAdress" /> -->
	<xsl:value-of select="$separator" />
	<!--22 Ordering Institution Account-->
	<xsl:value-of select="$OrderingIstitAccount" />
	<xsl:value-of select="$separator" />
	<!--23-->
	<xsl:value-of select="$separator" />
	<!--24-->
	<xsl:value-of select="$separator" />
	<!--25-->
	<xsl:value-of select="$separator" />
	<!--26-->
	<xsl:value-of select="$separator" />
	<!--27-->
	<xsl:value-of select="$separator" />
	<!--28-->
	<xsl:value-of select="$separator" />
	<!--29 Intermediary Institution - National Clearing Code-->
	<xsl:value-of select="$separator" />
	<!--30 Intermediary Institution – BIC-->
	<xsl:value-of select="$separator" />
	<!--31 Intermediary Institution – Clearing Code-->
	<xsl:value-of select="$separator" />
	<!--32 Intermediary Institution - Name & Address-->
	<xsl:value-of select="$separator" />
	<!--33 Intermediary Institution – Account-->
	<xsl:value-of select="$separator" />
	<!--34 Account with Institution - National Clearing-->
	<xsl:value-of select="$AccountWInstitBIC11CharDyn"/>
	<xsl:value-of select="$separator" />
	<!--35 Account with Institution –BIC-->
	<xsl:value-of select="$separator" />
	<!--36 Account with Institution -->
	<xsl:value-of select="$separator" />
	<!--37 Account with Institution - Clearing Code-->
	<xsl:value-of select="$separator" />
	<!--38 Account with Institution – Name & Address // still requiered ?//-->
	<!-- <xsl:value-of select="$AccountWInstitAcc" /> -->
	<xsl:value-of select="$separator" />
	<!--39 Account with Institution-->
	<xsl:value-of select="$separator" />
	<!--40 Beneficiary Customer/Institution - Clearing Code-->
	<xsl:value-of select="$separator" />
	<!--41 Beneficiary Customer/Institution – BIC-->
	<xsl:value-of select="$BenifInstitNameAdress" />
	<!-- <xsl:value-of select="$OrdringInstitutionAdress" /> -->
	<xsl:value-of select="$separator" />
	<!--42 Beneficiary Customer/|Institution - Clearing Code-->
	<xsl:value-of select="$BenifInstitNameAccount" />
	<!-- <xsl:value-of select="$accountName" /> -->
	<xsl:value-of select="$separator" />
	<!--43 Beneficiary Customer/|Institution - Name & Address-->
	<!-- <xsl:value-of select="$MT202SenderInfoStatic" /> -->
	<xsl:value-of select="$separator" />
	<!--44 Beneficiary Customer/|Institution – Account-->
	<xsl:value-of select="$separator" />
	<!--45 Sender to Receiver Information - Code-->
	<xsl:value-of select="$separator" />
	<!--46 Sender to Receiver Information - Code-->
	<xsl:value-of select="$separator" />
	<!--47 Sender to Receiver Information - Code-->
	<xsl:value-of select="$separator" />
	<!--48 Sender to Receiver Information - Code-->
	<xsl:value-of select="$separator" />
	<!--49 Sender to Receiver Information - Code-->
	<xsl:text>&#10;</xsl:text>	
	<!-- <xsl:value-of select="$separator" /> -->
  </xsl:template>
</xsl:stylesheet>
