using System.Numerics;
using System.Windows.Media;

namespace Mephi.Cybernetics.Race.Gui.Wpf
{
    public static class VectorData
    {
        private static Vector3 _vector3 = new Vector3(111, 222, 3333333);


        public static float x
        {
            get => _vector3.X;
        }

        public static float y
        {
            get => _vector3.Y;
        }

        public static float z
        {
            get => _vector3.Z;
        }

        public static Vector3 vector
        {
            get => _vector3;
        }
    }

    public class VectorWrapper
    {
        private Vector3 _vector3;
        public override string ToString()
        {
            return _vector3.ToString();
        }

        public VectorWrapper(Vector3 vector3)
        {
            _vector3 = vector3;
        }

        public float VectorX
        {
            get => _vector3.X;
            set => _vector3.X = value;
        }
        
        public float VectorY
        {
            get => _vector3.Y;
            set => _vector3.Y = value;
        }
        
        public float VectorZ
        {
            get => _vector3.Z;
            set => _vector3.Z = value;
        }
    }
}