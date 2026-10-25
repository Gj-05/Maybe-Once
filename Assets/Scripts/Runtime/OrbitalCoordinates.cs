using UnityEngine;
using OctoEarth.Core;

namespace OctoEarth
{
    public static class OrbitalCoordinates
    {
        public const float UnitsPerAU = 20;
        // Physics XY plane maps onto Unity XZ plane. Visual floats never feed the solver.
        public static Vector3 ToUnity(Vector3d metres) => new Vector3((float)(metres.X/OrbitalMechanics.AU)*UnitsPerAU,(float)(metres.Z/OrbitalMechanics.AU)*UnitsPerAU,(float)(metres.Y/OrbitalMechanics.AU)*UnitsPerAU);
        public static Vector3 RelativeTo(Vector3d metres,Vector3d origin,double unitsPerMetre)
        {
            var relative=metres-origin;
            return new Vector3((float)(relative.X*unitsPerMetre),(float)(relative.Z*unitsPerMetre),(float)(relative.Y*unitsPerMetre));
        }
    }
}
