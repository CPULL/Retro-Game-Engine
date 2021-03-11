
public class Squirrel3 {
  const uint NOISE1 = 0xb5297a4d;
  const uint NOISE2 = 0x68e31da4;
  const uint NOISE3 = 0x1b56c4e9;
  const uint CAP = 1 << 30;
  const float CAP2 = 1 << 29;

  public static float Norm(int pos, uint seed = 0) {
    uint n = (uint)pos;
    n *= NOISE1;
    n += seed;
    n ^= n >> 8;
    n += NOISE2;
    n ^= n << 8;
    n *= NOISE3;
    n ^= n >> 8;
    float res = (n % CAP) / CAP2 - 1f;
    return res;
  }

  public static int Int(int pos, int seed = 0) {
    uint n = (uint)pos;
    n *= NOISE1;
    n += (uint)seed;
    n ^= n >> 8;
    n += NOISE2;
    n ^= n << 8;
    n *= NOISE3;
    n ^= n >> 8;
    return (int)(n % CAP);
  }

}

