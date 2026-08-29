namespace Mephi.Cybernetics.Race.DataAccessTests.DataStructures
{
    public class Frame : SemanticConstruction
    {
        new public Frame Parent
        {
            get { return GetValue<Frame>("Parent"); }
            set { SetValue("Parent", value); }
        }

        public FrameValuation Valuation
        {
            get { return GetValue<FrameValuation>("Valuation"); }
            set { SetValue("Valuation", value); }
        }
    }

}
