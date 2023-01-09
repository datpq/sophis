<!--
	Test Purpose : Format a simple bond xml from a sample non-Sophis format to Sophis format.
-->
<xsl:stylesheet version="1.0" 
	xmlns:mhsc="http://www.sophis.net/MizuhoSC" xmlns:xsd="http://www.w3.org/2001/XMLSchema" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:sophis="http://www.sophis.net/sophis"
	xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:reporting="http://www.sophis.net/reporting" 
	xmlns:common="http://sophis.net/sophis/common" xmlns:instrument="http://www.sophis.net/Instrument" 
	xmlns:trade="http://www.sophis.net/trade" xmlns:party="http://www.sophis.net/party" 
	xmlns:folio="http://www.sophis.net/folio" xmlns:exch="http://sophis.net/sophis/gxml/dataExchange" 
	xmlns:fpml="http://www.fpml.org/2005/FpML-4-2" xmlns:dsig="http://www.w3.org/2000/09/xmldsig#" 
	xmlns:mapping="http://www.sophis.net/mapping" xmlns:valuation="http://www.sophis.net/valuation" 
	xmlns:scenario="http://www.sophis.net/scenario" 
	xmlns:var="http://www.sophis.net/varReporting" xmlns:msxsl="urn:schemas-microsoft-com:xslt"
  xmlns:ns0="http://www.sophis.net/Instrument"
  >

  <xsl:param name="pNewType" select="'OnlyForUpdate'"/>
 
  <xsl:template match="node()|@*">
    <xsl:copy>
      <xsl:apply-templates select="node()|@*"/>
    </xsl:copy>
  </xsl:template>

  

  <xsl:template match="/ns0:notionalFuture/ns0:identifier/ns0:reference[@ns0:name='isin']/@ns0:modifiable">
    <xsl:attribute name="ns0:modifiable">
      <xsl:value-of select="$pNewType"/>
    </xsl:attribute>
  </xsl:template>

</xsl:stylesheet>