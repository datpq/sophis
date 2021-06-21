using System.Configuration;
using sophis.configuration;

namespace sophis.quotesource.configuration
{
    public class CFGAdapterConfigurationNode : ConfigurationNode
    {
        public override void RegisterSectionsAndGroups(Configuration configuration)
        {   
           
                QuoteSourceCoreConfigurationGroup coreGroup = new QuoteSourceCoreConfigurationGroup();
                RecorderConfigurationGroup recoderGroup = new RecorderConfigurationGroup();
                CFGAdapterConfigurationGroup adapterGroup = new CFGAdapterConfigurationGroup();
               // AdapterConfigurationGroup adapterCommonGroup = new AdapterConfigurationGroup();

                configuration.SectionGroups.Add(QuoteSourceCoreConfigurationGroup.GROUP_NAME, coreGroup);
                configuration.SectionGroups.Add(RecorderConfigurationGroup.GROUP_NAME, recoderGroup);
                configuration.SectionGroups.Add(CFGAdapterConfigurationGroup.GROUP_NAME, adapterGroup);
                //configuration.SectionGroups.Add(AdapterConfigurationGroup.GROUP_NAME, adapterCommonGroup);

                coreGroup.RegisterSection();
                recoderGroup.RegisterSection();
                adapterGroup.RegisterSection();
                //adapterCommonGroup.RegisterSection();


                //CommonAdapterSection commonSection = adapterCommonGroup.FindOrCreateSection<CommonAdapterSection>(CommonAdapterSection.SECTION_NAME);
                //commonSection.forceValues("CFG", "CFG", false, "", "");        
        }

      
        public override string Name
        {
            get { return "QuoteSource CFG"; }
        }

    }
}
