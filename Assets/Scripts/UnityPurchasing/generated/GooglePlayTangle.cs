// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("mxJKQ6bocyeS7M9lYCYPO/aSP80+p2Q3o9Ix0mMjxmyJHiXeaZ0gS8Q3Dh8qbGjivO3rMhEyXZOT8MKnyq5VcO/Qp5pkRCSYf5bshEPYCqvZefcVQn6zUxkgXKbf9itWJ7S9U2pFXjdctKr3jEkgMys6ZGWK8fMhxpCktP6myqa82cD4kT7fnbcR2OmNDgAPP40OBQ2NDg4PvK1tVCOr7j+NDi0/AgkGJYlHifgCDg4OCg8M6tRYLyZ1Wb9MMBFyXXf3xwSqND7X1Uk1wC63akLdcuHJcMa6FSKMrO3sYb8fAGTUFzqgmwLxfWG5sYqduplBhMqCYsuDfLhE8b/g+6ysUOru5trJ2UFF2wDqWAM9iTPoO0+PMvPGOk71Z4Armg0MDg8O");
        private static int[] order = new int[] { 5,13,11,11,7,12,11,13,12,11,11,11,13,13,14 };
        private static int key = 15;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
