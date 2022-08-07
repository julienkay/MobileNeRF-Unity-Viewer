using Newtonsoft.Json;

public partial class Mlp {
    [JsonProperty("0_weights")]
    public double[][] _0Weights;

    [JsonProperty("1_weights")]
    public double[][] _1Weights;

    [JsonProperty("2_weights")]
    public double[][] _2Weights;

    [JsonProperty("0_bias")]
    public double[] _0Bias;

    [JsonProperty("1_bias")]
    public double[] _1Bias;

    [JsonProperty("2_bias")]
    public double[] _2Bias;

    [JsonProperty("obj_num")]
    public int ObjNum;
}