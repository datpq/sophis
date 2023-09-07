using MEDIO.OrderAutomation.NET.Source.GUI;
using sophis.scenario;

namespace MEDIO.OrderAutomation.net.Source.Scenarios
{
    public class CSxFXAutomationSettingScenario : CSMScenario
    {
        public override eMProcessingType GetProcessingType()
        {
            return eMProcessingType.M_pUserPreference;
        }

        public override void Initialise()
        {
        }

        public override void Run()
        {
            CSxFXAutomationSetting.Display();
        }

        public override void Done()
        {
        }
    }
}
