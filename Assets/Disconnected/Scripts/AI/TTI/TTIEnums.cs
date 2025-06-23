
/// <summary>
/// Target for image generation - what the user wants to do with the image
/// </summary>
public enum ImageTarget
{
    ImageTo3D = 0,
    WallArt = 1
}

/// <summary>
/// Detail level for image generation
/// </summary>
public enum ImageDetailLevel
{
    MoreDetailed = 0,
    LessDetailed = 1,
    AsIs = 2,
    Default = 3
}

/// <summary>
/// Creativity level for image generation
/// </summary>
public enum ImageCreativityLevel
{
    MoreCreative = 0,
    LessCreative = 1,
    AsIs = 2,
    Default = 3
}


public enum ImageShape
{
    Square = 0,
    Horizontal = 1,
    Vertical = 2
}