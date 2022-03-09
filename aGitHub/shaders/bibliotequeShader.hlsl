float2 RepereVector(float2 value, float2 max)
    {
    int x = frac(value.x / max.x) * max.x;
    int y = frac(value.y / max.y) * max.y;
    return float2(x, y);
    }

float distLine(float3 a, float3 b, float3 c)
    {
    float3 v = b - a;
    float3 w = c - a;

    float c1 = dot(w, v);
    float c2 = dot(v, v);

    if (c1 <= 0)
        {
        return distance(c, a);
        }

    if (c2 <= c1)
        {
        return distance(c, b);
        }

    float bd = c1 / c2;
    float3 Pb = a + bd * v;

    return distance(c, Pb);
    }