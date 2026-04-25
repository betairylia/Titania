using Unity.Mathematics;

namespace Titania.WorldGenUtils
{
    public static class Noise
    {
        public enum SimplexModifiers
        {
            None,
            Ridge,
            Billow
        }
        
        public static float Simplex2D(
            float2 p,
            float scale,
            SimplexModifiers modifier = SimplexModifiers.None,
            int seed = 20170715,
            int octaves = 4,
            float persistence = 0.5f,
            float _octaveScale = 0.5f)
        {
            float result = 0;
            
            float scaleCurrent = scale;
            float totalCoeff = 0;
            float coeffCurrent = 1.0f;
            
            for (int i = 0; i < octaves; i++)
            {
                switch (modifier)
                {
                    case SimplexModifiers.None:
                        result += noise.snoise(p / scaleCurrent) * coeffCurrent;
                        break;
                    case SimplexModifiers.Ridge:
                        result -= math.abs(noise.snoise(p / scaleCurrent)) * coeffCurrent;
                        break;
                    case SimplexModifiers.Billow:
                        result += noise.snoise(p / scaleCurrent) * coeffCurrent;
                        break;
                }

                totalCoeff += coeffCurrent;
                coeffCurrent *= persistence;
                scaleCurrent *= _octaveScale;
            }

            return result / totalCoeff;
        }
    }

    public static class HMUtils
    {
        /// <summary>
        /// Clamp a value to [-1, 1]
        /// </summary>
        /// <param name="val">value to clamp</param>
        /// <returns>clamped value</returns>
        public static float Clamp1(float val)
        {
            return math.clamp(val, -1.0f, 1.0f);
        }

        /// <summary>
        /// |x|^a * sgn(x)
        /// </summary>
        /// <param name="val">x (Recommended range [-1, 1])</param>
        /// <param name="steepness">a (Positive)</param>
        /// <returns></returns>
        public static float Steepness(float val, float steepness)
        {
            return math.pow(math.abs(val), steepness) * math.sign(val);
        }
    }
}