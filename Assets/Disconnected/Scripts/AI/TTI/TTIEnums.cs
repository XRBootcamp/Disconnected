
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

public static class TTIEnumExtensions
{
    public static ImageShape? ParseImageShape(string imageShape)
    {
        return imageShape switch
        {
            "Square" => ImageShape.Square,
            "Horizontal" => ImageShape.Horizontal,
            "Vertical" => ImageShape.Vertical,
            _ => null
        };
    }

    public static ImageTarget? ParseImageTarget(string target)
    {
        return target switch
        {
            "ImageTo3D" => ImageTarget.ImageTo3D,
            "WallArt" => ImageTarget.WallArt,
            _ => null
        };
    }

    /// <summary>
    /// These have math values and calculations. 
    /// for this reason null is not exactly a good outcome as info is lost
    /// </summary>
    /// <param name="editDetail"></param>
    /// <returns></returns>
    public static ImageDetailLevel ParseImageDetailLevel(string editDetail)
    {
        return editDetail switch
        {
            "MoreDetailed" => ImageDetailLevel.MoreDetailed,
            "LessDetailed" => ImageDetailLevel.LessDetailed,
            "AsIs" => ImageDetailLevel.AsIs,
            "Default" => ImageDetailLevel.Default,
            _ => ImageDetailLevel.Default
        };
    }

    public static ImageCreativityLevel ParseImageCreativityLevel(string t2iCreativity)
    {
        return t2iCreativity switch
        {
            "MoreCreative" => ImageCreativityLevel.MoreCreative,
            "LessCreative" => ImageCreativityLevel.LessCreative,
            "AsIs" => ImageCreativityLevel.AsIs,
            "Default" => ImageCreativityLevel.Default,
            _ => ImageCreativityLevel.Default
        };
    }

}
