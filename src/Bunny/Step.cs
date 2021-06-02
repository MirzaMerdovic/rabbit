using System;

namespace Bunny
{
    [Flags]
    public enum Step
    {
        None = 0,
        GenerateImagePreview = 1,
        GenerateMetadata = 2,
        CalculateWeight = 4
    }
}
