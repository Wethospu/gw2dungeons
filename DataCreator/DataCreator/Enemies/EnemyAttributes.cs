using DataCreator.Enemies;
using DataCreator.Utility;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace DataCreator
{
  public class EnemyAttributes
  {
    public EnemyAttributes()
    {
      Name = "";
      Occurrences = null;
      Description = "";
      Rank = "";
      Flag = 0;
      Multipliers = new Multipliers();
      Gender = "";
      Size = 1.0;
      Family = "";
    }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("occurrences")]
    public Occurrence[] Occurrences { get; set; }

    [JsonProperty("rank")]
    public string Rank { get; set; }

    [JsonProperty("flag")]
    public int Flag { get; set; }

    [JsonProperty("multipliers")]
    public Multipliers Multipliers { get; set; }

    [JsonProperty("sex")]
    public string Gender { get; set; }

    [JsonProperty("size")]
    public double Size { get; set; }

    [JsonProperty("family")]
    public string Family { get; set; }

    public string ToHtml()
    {
      var htmlBuilder = new StringBuilder();
      htmlBuilder.Append(" data-power=\"").Append(Multipliers.Power).Append("\"");
      htmlBuilder.Append(" data-precision=\"").Append(Multipliers.Precision).Append("\"");
      htmlBuilder.Append(" data-toughness=\"").Append(Multipliers.Toughness).Append("\"");
      htmlBuilder.Append(" data-vitality=\"").Append(Multipliers.Vitality).Append("\"");
      htmlBuilder.Append(" data-ferocity=\"").Append(Multipliers.Ferocity).Append("\"");
      htmlBuilder.Append(" data-condition=\"").Append(Multipliers.ConditionDamage).Append("\"");
      htmlBuilder.Append(" data-healing=\"").Append(Multipliers.HealingPower).Append("\"");
      htmlBuilder.Append(" data-health=\"").Append(Multipliers.HealthMultiplier).Append("\"");
      return htmlBuilder.ToString();
    }
  }

  public class Multipliers
  {

    [JsonProperty("power")]
    public double Power { get; set; }

    [JsonProperty("precision")]
    public double Precision { get; set; }

    [JsonProperty("toughness")]
    public double Toughness { get; set; }

    [JsonProperty("vitality")]
    public double Vitality { get; set; }

    [JsonProperty("ferocity")]
    public double Ferocity { get; set; }

    [JsonProperty("healing_power")]
    public double HealingPower { get; set; }

    [JsonProperty("condition_damage")]
    public double ConditionDamage { get; set; }

    [JsonProperty("health_multiplier")]
    public double HealthMultiplier { get; set; }
  }

  public class Occurrence
  {

    [JsonProperty("location")]
    public string Location { get; set; }

    [JsonProperty("level")]
    public int Level { get; set; }
  }
}
