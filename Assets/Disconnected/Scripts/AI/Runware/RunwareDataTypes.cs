
// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);

using System;

namespace Runware
{
    [Serializable]
    public class TextToImageRequest
    {
        public string taskType { get; set; }
        public string taskUUID { get; set; }
        public string outputType { get; set; }
        public string outputFormat { get; set; }
        public bool? checkNSFW {get; set;}
        public bool? includeCost {get; set;}
        public string positivePrompt { get; set; }
        public string negativePrompt { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public string model { get; set; }
        public int steps { get; set; }
        public double? CFGScale { get; set; }
        public int numberResults { get; set; }
    }

    [Serializable]
    public class TextToImageResponse
    {
        public string taskType { get; set; }
        public string imageUUID { get; set; }
        public string taskUUID { get; set; }
        public double cost { get; set; }
        public long seed { get; set; }
        public string imageURL {get; set; }
        public string imageBase64Data { get; set; }
        public string positivePrompt { get; set; }
        public bool? NSFWContent {get; set; }
    }

    // TODO: Image to Image for refinement 
}