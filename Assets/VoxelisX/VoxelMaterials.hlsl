#include "Packages/com.voxelis.voxelisx/Runtime/Rendering/Shaders/VoxelisXUtils.hlsl"

#define GET_MATERIAL GetMaterial_Color

struct VoxelMaterial
{
    float3 albedo;
    float3 emission;
    
    float smoothness;
    float metallic;

    // Transparency
    float IOR;
    float extinction;
};

static VoxelMaterial MatTableOpaque[2] = {
    {{0.2, 0.2, 0.2}, {0, 0, 0}, 0.0, 0, 1.5, 0},
    {{.5, .5, .5}, {0, 0, 0}, 0.98, 1, 1.5, 0.5}
};

static VoxelMaterial MatTableTransparent[1] = {
    {{.7, .8, .9}, {0, 0, 0}, 0.98, 1, 1.5, 1.0}
};

static VoxelMaterial fallback = {{1, 0, 1}, {0.2, 0, 0.2}, 0, 0, 1, 0};

static uint MatNumOpaque = 2;
static uint MatNumTransparent = 1;

static VoxelMaterial GetMaterial(int ID)
{
    const bool isOpaque = IsOpaque(ID);

    if(isOpaque)
    {
        // OpaqueID / 0x1000 ~ 0xFFFF
        int OpaqueID = ID - 0x1000;
        if(OpaqueID > MatNumOpaque){ return fallback; }
        return MatTableOpaque[OpaqueID];
    }
    else
    {
        // TransparentID == ID / 0x0000 ~ 0x0FFF
        // 0x0000 is always Air (Empty)
        if(ID > MatNumTransparent){ return fallback; }
        return MatTableTransparent[ID - 1];
    }
}

// Pure color voxels
static VoxelMaterial GetMaterial_Color(int ID)
{
    VoxelMaterial mat;
    
    float3 color = float3(((ID >> 11) & 0x1F) / 31.0, ((ID >> 6) & 0x1F) / 31.0, ((ID >> 1) & 0x1F) / 31.0);
    mat.albedo = color * (1 - (ID & 0b01));
    mat.emission = color * (ID & 0b01);

    mat.smoothness = 0;
    mat.metallic = 0;
    mat.IOR = 1;
    mat.extinction = 0;

    return mat;
}
