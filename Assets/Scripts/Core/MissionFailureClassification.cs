namespace OctoEarth.Core
{
    public static class MissionFailureClassification
    {
        // A1+ are declared assumptions, never engineering proof or additional E1-E3 enablers.
        public static bool HasMeasuredFailure(MissionFlags flags)
        {
            foreach(var f in flags.All) if(f.code!="FocusingOpticsUnresolved" && f.code!="MaterialStrengthUnvalidated" && f.code!="ThermalStructuralModelIncomplete") return true;
            return false;
        }
    }
}
